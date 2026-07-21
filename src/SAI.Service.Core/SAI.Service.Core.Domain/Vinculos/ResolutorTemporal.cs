namespace SAI.Service.Core.Domain.Vinculos;

/// <summary>
/// Resuelve qué vínculo estaba vigente en un instante dado (RC-07). La historia densa guarda el
/// dispositivo y el instante, nunca la batería directamente; la batería (o el dispositivo que
/// cubre un host) se resuelve consultando el vínculo cuyo intervalo contiene ese instante.
/// <para>
/// Es una función <b>pura</b> de resolución: no muta ni copia atribución. Por eso corregir la
/// fecha de un recambio reatribuye el histórico afectado sin migrar datos (ADR-05 §5.2), y la
/// misma función se reutiliza cuando una prueba de batería congela el montaje resuelto (I-15,
/// incremento posterior).
/// </para>
/// </summary>
public static class ResolutorTemporal
{
    /// <summary>
    /// Devuelve el montaje que aloja la batería en <paramref name="dispositivoCodigo"/> /
    /// <paramref name="posicion"/> en el <paramref name="instante"/> dado, o <c>null</c> si no
    /// había batería montada (hueco legítimo). La contención es semiabierta (RC-07): el instante
    /// de cierre pertenece al montaje siguiente.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Si más de un montaje contiene el instante para la misma clave: eso es un solape que la
    /// exclusividad temporal (I-1) debió impedir al escribir.
    /// </exception>
    public static MontajeBateria? ResolverMontaje(
        IEnumerable<MontajeBateria> montajes,
        string dispositivoCodigo,
        string posicion,
        DateTimeOffset instante)
    {
        ArgumentNullException.ThrowIfNull(montajes);

        return montajes.SingleOrDefault(m =>
            m.DispositivoCodigo == dispositivoCodigo
            && m.Posicion == posicion
            && m.Vigencia.Contiene(instante));
    }

    /// <summary>
    /// Devuelve la cobertura vigente del <paramref name="hostCodigo"/> en el
    /// <paramref name="instante"/> dado, o <c>null</c> si el host no estaba protegido (hueco
    /// legítimo: días sin protección).
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Si más de una cobertura contiene el instante para el mismo host: solape que I-4 debió impedir.
    /// </exception>
    public static CoberturaHost? ResolverCobertura(
        IEnumerable<CoberturaHost> coberturas,
        string hostCodigo,
        DateTimeOffset instante)
    {
        ArgumentNullException.ThrowIfNull(coberturas);

        return coberturas.SingleOrDefault(c =>
            c.HostCodigo == hostCodigo
            && c.Vigencia.Contiene(instante));
    }
}
