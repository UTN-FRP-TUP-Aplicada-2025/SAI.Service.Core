namespace SAI.Service.Core.Domain.Vinculos;

/// <summary>
/// Vigencia como <b>intervalo semiabierto</b> <c>[Desde, Hasta)</c> (ADR-05, RC-02). Es un
/// objeto de valor: la vigencia no es un atributo de la unidad sino una entidad con intervalo,
/// y acá se materializa su semántica temporal, reutilizable por <see cref="MontajeBateria"/> y
/// <see cref="CoberturaHost"/>.
/// <para>
/// <see cref="Hasta"/> nulo significa <b>VIGENTE</b> (fin abierto), nunca "desconocido"
/// (RC-02 §4). La contención es semiabierta: el instante de cierre pertenece al vínculo
/// <i>siguiente</i>, no al que cierra (RC-03 / RC-07), de modo que dos vínculos que se tocan
/// en el borde no solapan (sucesión sin hueco).
/// </para>
/// </summary>
public readonly record struct Vigencia
{
    /// <summary>Instante de inicio del intervalo (inclusive).</summary>
    public DateTimeOffset Desde { get; }

    /// <summary>Instante de fin (exclusivo). Nulo = vigente (fin abierto).</summary>
    public DateTimeOffset? Hasta { get; }

    /// <summary>
    /// Construye una vigencia. Rechaza un intervalo mal formado (<see cref="Hasta"/> anterior a
    /// <see cref="Desde"/>), coherente con el CHECK <c>CK_Vigencia_Intervalo</c> del modelo lógico.
    /// </summary>
    /// <param name="desde">Inicio del intervalo.</param>
    /// <param name="hasta">Fin exclusivo, o nulo para una vigencia abierta.</param>
    /// <exception cref="ArgumentException">Si <paramref name="hasta"/> es anterior a <paramref name="desde"/>.</exception>
    public Vigencia(DateTimeOffset desde, DateTimeOffset? hasta = null)
    {
        if (hasta is not null && hasta.Value < desde)
        {
            throw new ArgumentException(
                "Una vigencia mal formada: el fin no puede ser anterior al inicio.",
                nameof(hasta));
        }

        Desde = desde;
        Hasta = hasta;
    }

    /// <summary>Verdadero si la vigencia está abierta (sin fin declarado).</summary>
    public bool EsVigente => Hasta is null;

    /// <summary>
    /// Contención semiabierta <c>Desde &lt;= instante &lt; Hasta</c> (RC-07). El instante de
    /// cierre no queda contenido: pertenece al vínculo que abre a continuación.
    /// </summary>
    public bool Contiene(DateTimeOffset instante) =>
        Desde <= instante && (Hasta is null || instante < Hasta.Value);

    /// <summary>
    /// Solape de dos intervalos semiabiertos (I-1, I-2, I-4). Tocarse en el borde
    /// (<c>this.Hasta == otra.Desde</c>) <b>no</b> es solape: eso es exactamente la sucesión
    /// sin hueco de RC-03.
    /// </summary>
    public bool Solapa(Vigencia otra)
    {
        var finEste = Hasta ?? DateTimeOffset.MaxValue;
        var finOtra = otra.Hasta ?? DateTimeOffset.MaxValue;
        return Desde < finOtra && otra.Desde < finEste;
    }

    /// <summary>
    /// Cierra una vigencia abierta en el instante indicado, devolviendo una vigencia nueva
    /// (inmutable). El instante de cierre es el <see cref="Desde"/> del vínculo siguiente en una
    /// sucesión sin hueco (RC-03).
    /// </summary>
    /// <exception cref="InvalidOperationException">Si la vigencia ya estaba cerrada.</exception>
    /// <exception cref="ArgumentException">Si <paramref name="hasta"/> es anterior a <see cref="Desde"/>.</exception>
    public Vigencia CerrarEn(DateTimeOffset hasta)
    {
        if (!EsVigente)
        {
            throw new InvalidOperationException("La vigencia ya está cerrada; no se puede cerrar dos veces.");
        }

        return new Vigencia(Desde, hasta);
    }
}
