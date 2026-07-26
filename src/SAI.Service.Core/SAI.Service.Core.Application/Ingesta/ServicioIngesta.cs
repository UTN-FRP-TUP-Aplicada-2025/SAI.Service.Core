using SAI.Service.Core.Domain.Intervenciones;
using SAI.Service.Core.Domain.Valores;

namespace SAI.Service.Core.Application.Ingesta;

/// <summary>
/// Ingesta idempotente de intervenciones externas (CU-11, US-21/US-22): "la UI propone, el sistema valida",
/// pero acá el emisor es una máquina. Resuelve la idempotencia por clave contra la historia append-only
/// (reintento idéntico vs. conflicto por huella sha256, RN-09), valida los invariantes <b>antes</b> de
/// registrar —dinero completo (RN-07), cuadre de costos (RN-08) y coherencia temporal (RN-12)— y asigna la
/// confianza de la fuente (media, ADR-06). Postcondición de fallo: no se registra nada.
/// </summary>
public sealed class ServicioIngesta(IRepositorioIngesta repositorio)
{
    /// <summary>Ingresa una intervención. <paramref name="huella"/> es el sha256 del cuerpo recibido.</summary>
    public async Task<ResultadoIngesta> IngerirAsync(
        string clave, string fuenteCodigo, EntradaIngesta entrada, string huella, DateTimeOffset ahora, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clave);
        ArgumentException.ThrowIfNullOrWhiteSpace(fuenteCodigo);
        ArgumentNullException.ThrowIfNull(entrada);

        // 1. Idempotencia por clave (RN-09): misma clave + misma huella = reintento; huella distinta = conflicto.
        var existente = await repositorio.BuscarPorClaveAsync(clave, ct);
        if (existente is not null)
        {
            if (string.Equals(existente.HuellaCuerpo, huella, StringComparison.OrdinalIgnoreCase))
            {
                return new ResultadoIngesta(CodigoIngesta.Reintento, Id: existente.Codigo,
                    TiempoValido: existente.TiempoValido, TiempoRegistrado: existente.TiempoRegistrado, Confianza: "media");
            }

            return new ResultadoIngesta(CodigoIngesta.ConflictoIdempotencia,
                HuellaOriginal: existente.HuellaCuerpo, HuellaRecibida: huella,
                AccionSugerida: "El cuerpo difiere del ya registrado con esta clave. Corregir por el endpoint de rectificación (pendiente, ADR-21) o reenviar con una clave de idempotencia nueva.");
        }

        // 2. La fuente de datos del encabezado debe estar registrada (aporta la confianza base).
        var fuente = await repositorio.FuenteAsync(fuenteCodigo, ct);
        if (fuente is null)
        {
            return new ResultadoIngesta(CodigoIngesta.FuenteNoRegistrada, Campo: "X-Fuente-Datos",
                Detalle: $"La fuente de datos '{fuenteCodigo}' no está registrada.");
        }

        // 3. Obligatorios mínimos del cuerpo.
        if (string.IsNullOrWhiteSpace(entrada.TipoIntervencionId) || string.IsNullOrWhiteSpace(entrada.DispositivoId)
            || entrada.TiempoValido is null || entrada.Costos is null)
        {
            return new ResultadoIngesta(CodigoIngesta.Validacion, Campo: "cuerpo", Invariante: "validacion",
                Detalle: "Faltan campos obligatorios: tipoIntervencionId, dispositivoId, tiempoValido y costos.");
        }

        // 4. Dinero completo (RN-07): cada importe con moneda y fecha, antes de construir el value object.
        var (costos, campoDinero) = ConstruirCostos(entrada.Costos);
        if (costos is null)
        {
            return new ResultadoIngesta(CodigoIngesta.Validacion, Campo: campoDinero, Invariante: "validacion",
                Detalle: "Todo importe debe declarar su moneda y su fecha (RN-07).");
        }

        // 5. Cuadre de costos (RN-08).
        if (!costos.Value.Cuadra())
        {
            return new ResultadoIngesta(CodigoIngesta.Validacion, Campo: "costos.total", Invariante: "validacion",
                Detalle: "El total no iguala la suma de repuestos y mano de obra (RN-08).");
        }

        // 6. Coherencia temporal (RN-12): ninguna unidad referenciada operó después de su baja.
        var unidades = new List<string> { entrada.DispositivoId };
        if (entrada.BateriaIds is not null)
        {
            unidades.AddRange(entrada.BateriaIds);
        }

        foreach (var codigoUnidad in unidades.Where(c => !string.IsNullOrWhiteSpace(c)))
        {
            var unidad = await repositorio.UnidadAsync(codigoUnidad, ct);
            if (unidad is not null && !unidad.AdmiteOperacionEn(entrada.TiempoValido.Value))
            {
                return new ResultadoIngesta(CodigoIngesta.CoherenciaTemporal, Campo: codigoUnidad, Invariante: "coherencia_temporal",
                    Detalle: $"La unidad '{codigoUnidad}' estaba dada de baja antes de la fecha de la intervención (RN-12).");
            }
        }

        // 7. Registrar (append-only). La confianza es la de la fuente (media por origen externo).
        var codigo = $"ing-{Guid.NewGuid():N}";
        var intervencion = new IntervencionIngerida(
            codigo, clave, huella, fuente.Codigo, fuente.ConfianzaBase,
            entrada.TipoIntervencionId, entrada.DispositivoId, entrada.BateriaIds ?? [], entrada.ProveedorId,
            costos.Value, entrada.Hallazgos, ConstruirDisposicion(entrada.DisposicionFinal), entrada.TiempoValido.Value, ahora);
        await repositorio.AgregarAsync(intervencion, ct);

        return new ResultadoIngesta(CodigoIngesta.Creado, Id: codigo,
            TiempoValido: entrada.TiempoValido, TiempoRegistrado: ahora, Confianza: "media");
    }

    // Construye los Costos sumando el arreglo de repuestos; devuelve el nombre del campo con dinero incompleto si lo hay.
    private static (Costos? costos, string? campo) ConstruirCostos(CostosEntrada entrada)
    {
        if (entrada.ManoDeObra is null || entrada.Total is null)
        {
            return (null, "costos");
        }

        if (!EsCompleto(entrada.ManoDeObra))
        {
            return (null, "costos.manoDeObra");
        }

        if (!EsCompleto(entrada.Total))
        {
            return (null, "costos.total");
        }

        var manoDeObra = ADinero(entrada.ManoDeObra);
        var total = ADinero(entrada.Total);

        Dinero repuestos;
        if (entrada.Repuestos is { Count: > 0 } lista)
        {
            if (lista.Any(d => !EsCompleto(d)))
            {
                return (null, "costos.repuestos");
            }

            // Se suman los importes de repuestos conservando su moneda y la fecha más reciente.
            var montos = lista.Sum(d => d.Monto);
            var moneda = lista[0].Moneda!;
            var fecha = lista.Max(d => d.Fecha!.Value);
            repuestos = new Dinero(montos, moneda, fecha);
        }
        else
        {
            // Sin repuestos declarados: cero en la moneda del total.
            repuestos = new Dinero(0m, total.Moneda, total.Fecha);
        }

        return (new Costos(repuestos, manoDeObra, total), null);
    }

    private static bool EsCompleto(DineroEntrada d) => !string.IsNullOrWhiteSpace(d.Moneda) && d.Fecha is not null;

    private static Dinero ADinero(DineroEntrada d) => new(d.Monto, d.Moneda!, d.Fecha!.Value);

    private static DisposicionFinal? ConstruirDisposicion(DisposicionEntrada? entrada) =>
        entrada is not null && !string.IsNullOrWhiteSpace(entrada.Destino) && !string.IsNullOrWhiteSpace(entrada.Receptor)
            ? new DisposicionFinal(entrada.Destino, entrada.Receptor)
            : null;
}
