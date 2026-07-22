namespace SAI.Service.Core.Domain.Monitoreo;

/// <summary>
/// Deriva los eventos de una muestra recién tomada evaluando reglas versionadas sobre la ventana
/// reciente de muestras (BT-19, US-09, RC-09). Es una función pura: no toca la base; recibe la
/// ventana (más reciente primero, con la actual en el índice 0) y las reglas vigentes, y devuelve
/// los eventos a persistir. Cada evento queda atado a la regla y versión con la que se derivó.
/// <para>El disparo del apagado (BT-20) se decide por tiempo en batería y <c>battery.voltage</c>,
/// <b>nunca</b> por el flag LB ni por <c>battery.charge</c> (ADR-12).</para>
/// </summary>
public static class DerivadorEventos
{
    /// <summary>Código de la regla de corte/retorno/microcorte.</summary>
    public const string ReglaCorte = "regla-corte";

    /// <summary>Código de la regla de tensión fuera de rango.</summary>
    public const string ReglaTension = "regla-tension";

    /// <summary>Código de la regla de desconexión (sondeos perdidos).</summary>
    public const string ReglaDesconexion = "regla-desconexion";

    /// <summary>Código de la regla de disparo del apagado (BT-20).</summary>
    public const string ReglaDisparo = "regla-disparo";

    /// <summary>
    /// Deriva los eventos de la muestra <paramref name="recientes"/>[0] (la más reciente). El
    /// generador <paramref name="nuevoCodigo"/> produce el código de cada evento.
    /// </summary>
    public static IReadOnlyList<Evento> Derivar(
        string dispositivoCodigo,
        IReadOnlyList<Muestra> recientes,
        IReadOnlyDictionary<string, ReglaDerivacion> reglas,
        Func<string> nuevoCodigo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dispositivoCodigo);
        ArgumentNullException.ThrowIfNull(recientes);
        ArgumentNullException.ThrowIfNull(reglas);
        ArgumentNullException.ThrowIfNull(nuevoCodigo);

        if (recientes.Count == 0)
        {
            return [];
        }

        var eventos = new List<Evento>();
        var actual = recientes[0];
        var previa = recientes.Count > 1 ? recientes[1] : null;

        DerivarCorteYRetorno(dispositivoCodigo, actual, previa, recientes, reglas, nuevoCodigo, eventos);
        DerivarTension(dispositivoCodigo, actual, previa, recientes, reglas, nuevoCodigo, eventos);
        DerivarDesconexion(dispositivoCodigo, actual, recientes, reglas, nuevoCodigo, eventos);
        DerivarDisparo(dispositivoCodigo, actual, previa, recientes, reglas, nuevoCodigo, eventos);

        return eventos;
    }

    private static void DerivarCorteYRetorno(
        string disp, Muestra actual, Muestra? previa, IReadOnlyList<Muestra> recientes,
        IReadOnlyDictionary<string, ReglaDerivacion> reglas, Func<string> nuevoCodigo, List<Evento> eventos)
    {
        if (!reglas.TryGetValue(ReglaCorte, out var regla))
        {
            return;
        }

        var estadoActual = Estado(actual);
        var estadoPrevio = previa is null ? null : Estado(previa);
        if (estadoActual is null || estadoPrevio is null)
        {
            return;
        }

        if (estadoPrevio == Variables.CodigoEnLinea && estadoActual == Variables.CodigoEnBateria)
        {
            eventos.Add(new Evento(nuevoCodigo(), disp, TipoEvento.CorteSuministro, actual.Instante, regla));
        }
        else if (estadoPrevio == Variables.CodigoEnBateria && estadoActual == Variables.CodigoEnLinea)
        {
            var inicio = InicioDelCorte(recientes);
            var duracion = inicio is { } i ? (actual.Instante - i).TotalSeconds : (double?)null;
            eventos.Add(new Evento(nuevoCodigo(), disp, TipoEvento.RetornoRed, actual.Instante, regla, duracion));

            var maxMicro = regla.Parametro("microcorteMaxSeg");
            if (duracion is { } d && maxMicro is { } max && d < max)
            {
                // La incertidumbre es del orden del intervalo entre muestras (CL-10).
                eventos.Add(new Evento(nuevoCodigo(), disp, TipoEvento.Microcorte, actual.Instante, regla, d, EstimarIntervalo(recientes)));
            }
        }
    }

    private static void DerivarTension(
        string disp, Muestra actual, Muestra? previa, IReadOnlyList<Muestra> recientes,
        IReadOnlyDictionary<string, ReglaDerivacion> reglas, Func<string> nuevoCodigo, List<Evento> eventos)
    {
        if (!reglas.TryGetValue(ReglaTension, out var regla))
        {
            return;
        }

        var min = regla.Parametro("min");
        var max = regla.Parametro("max");
        var sostenido = regla.Parametro("sostenidoSeg");
        if (min is null || max is null || sostenido is null)
        {
            return;
        }

        if (!FueraDeRango(actual, min.Value, max.Value))
        {
            return;
        }

        // Emite una sola vez, al cruzar el umbral sostenido: la duración fuera de rango recién ahora
        // alcanza 'sostenidoSeg' y en la muestra previa aún no lo alcanzaba.
        var inicio = InicioTramoFueraDeRango(recientes, min.Value, max.Value);
        if (inicio is null)
        {
            return;
        }

        var durActual = (actual.Instante - inicio.Value).TotalSeconds;
        var durPrevia = previa is not null && FueraDeRango(previa, min.Value, max.Value)
            ? (previa.Instante - inicio.Value).TotalSeconds
            : -1;

        if (durActual >= sostenido.Value && durPrevia < sostenido.Value)
        {
            eventos.Add(new Evento(nuevoCodigo(), disp, TipoEvento.TensionFueraDeRango, actual.Instante, regla, durActual));
        }
    }

    private static void DerivarDesconexion(
        string disp, Muestra actual, IReadOnlyList<Muestra> recientes,
        IReadOnlyDictionary<string, ReglaDerivacion> reglas, Func<string> nuevoCodigo, List<Evento> eventos)
    {
        if (!reglas.TryGetValue(ReglaDesconexion, out var regla))
        {
            return;
        }

        var n = (int)(regla.Parametro("sondeosPerdidos") ?? 3);
        if (n <= 0 || recientes.Count < n)
        {
            return;
        }

        // Las últimas n muestras son perdidas y la anterior a esa racha no lo era (o no existe):
        // se emite una sola vez, justo al cruzar la racha.
        var ultimasN = recientes.Take(n).All(m => m.Calidad == CalidadMuestra.Perdida);
        var anteriorNoPerdida = recientes.Count == n || recientes[n].Calidad != CalidadMuestra.Perdida;
        if (ultimasN && anteriorNoPerdida)
        {
            eventos.Add(new Evento(nuevoCodigo(), disp, TipoEvento.DesconexionUsb, actual.Instante, regla));
        }
    }

    private static void DerivarDisparo(
        string disp, Muestra actual, Muestra? previa, IReadOnlyList<Muestra> recientes,
        IReadOnlyDictionary<string, ReglaDerivacion> reglas, Func<string> nuevoCodigo, List<Evento> eventos)
    {
        if (!reglas.TryGetValue(ReglaDisparo, out var regla))
        {
            return;
        }

        // Solo con el equipo en batería (nunca se decide por battery.charge/LB, ADR-12).
        if (Estado(actual) != Variables.CodigoEnBateria)
        {
            return;
        }

        var umbralSeg = regla.Parametro("umbralObSeg");
        if (umbralSeg is null)
        {
            return;
        }

        var inicio = InicioDelCorte(recientes);
        if (inicio is null)
        {
            return;
        }

        var durActual = (actual.Instante - inicio.Value).TotalSeconds;
        var durPrevia = previa is not null && Estado(previa) == Variables.CodigoEnBateria
            ? (previa.Instante - inicio.Value).TotalSeconds
            : -1;

        // Cruce del umbral de tiempo en batería (una sola vez).
        var cruzaTiempo = durActual >= umbralSeg.Value && durPrevia < umbralSeg.Value;

        // Alarma dura de tensión de batería (química), independiente del tiempo.
        var batV = actual.Valor(Variables.TensionBateria);
        var batMin = regla.Parametro("batVoltMin");
        var batPreviaBaja = previa is not null && batMin is not null
            && previa.Valor(Variables.TensionBateria) is { } bp && bp < batMin.Value;
        var cruzaTension = batMin is not null && batV is { } bv && bv < batMin.Value && !batPreviaBaja;

        if (cruzaTiempo || cruzaTension)
        {
            eventos.Add(new Evento(nuevoCodigo(), disp, TipoEvento.DisparoApagado, actual.Instante, regla, durActual));
        }
    }

    private static double? Estado(Muestra muestra) => muestra.Valor(Variables.EstadoUps);

    private static bool FueraDeRango(Muestra muestra, double min, double max)
    {
        var v = muestra.Valor(Variables.TensionEntrada);
        return v is { } tension && (tension < min || tension > max);
    }

    // Instante en que empezó el corte vigente (la transición OL→OB más reciente).
    private static DateTimeOffset? InicioDelCorte(IReadOnlyList<Muestra> recientes)
    {
        for (var i = 0; i + 1 < recientes.Count; i++)
        {
            if (Estado(recientes[i]) == Variables.CodigoEnBateria && Estado(recientes[i + 1]) == Variables.CodigoEnLinea)
            {
                return recientes[i].Instante;
            }
        }

        // Si toda la ventana está en batería, el inicio conocido es la muestra más antigua.
        return recientes.All(m => Estado(m) == Variables.CodigoEnBateria) ? recientes[^1].Instante : null;
    }

    // Instante en que empezó el tramo fuera de rango vigente.
    private static DateTimeOffset? InicioTramoFueraDeRango(IReadOnlyList<Muestra> recientes, double min, double max)
    {
        DateTimeOffset? inicio = null;
        for (var i = 0; i < recientes.Count; i++)
        {
            if (FueraDeRango(recientes[i], min, max))
            {
                inicio = recientes[i].Instante;
            }
            else
            {
                break;
            }
        }

        return inicio;
    }

    private static double? EstimarIntervalo(IReadOnlyList<Muestra> recientes) =>
        recientes.Count > 1 ? Math.Abs((recientes[0].Instante - recientes[1].Instante).TotalSeconds) : null;
}
