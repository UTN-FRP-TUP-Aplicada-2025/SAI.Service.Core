namespace SAI.Service.Core.Domain.Vinculos;

/// <summary>
/// Cobertura de un host por un dispositivo (SAI) durante una <see cref="Vigencia"/> (ADR-05,
/// RC-02). Es la capa de <b>vínculos temporales</b> del lado de los hosts protegidos: qué SAI
/// protege a qué host y desde cuándo; el dispositivo vigente se resuelve con
/// <see cref="ResolutorTemporal"/>.
/// <para>
/// Es excluyente por <c>host</c>: a lo sumo una cobertura vigente por host y sin solapes (I-4).
/// Un hueco intencional (host sin protección) es legítimo y se mide como días sin protección; lo
/// que se prohíbe es el solape.
/// </para>
/// </summary>
public sealed class CoberturaHost : IVinculoTemporal
{
    /// <summary>Código de negocio de la cobertura (identidad estable).</summary>
    public string Codigo { get; }

    /// <summary>Código de la unidad física dispositivo (SAI) que protege.</summary>
    public string DispositivoCodigo { get; }

    /// <summary>Código de la unidad física host protegido.</summary>
    public string HostCodigo { get; }

    /// <inheritdoc/>
    public Vigencia Vigencia { get; private set; }

    // Constructor de materialización (EF Core): construye la instancia y luego asigna las
    // propiedades por campo/setter. No introduce dependencia de framework en el dominio.
    private CoberturaHost()
    {
        Codigo = null!;
        DispositivoCodigo = null!;
        HostCodigo = null!;
    }

    /// <summary>Construye una cobertura con su vigencia.</summary>
    public CoberturaHost(
        string codigo,
        string dispositivoCodigo,
        string hostCodigo,
        Vigencia vigencia)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(dispositivoCodigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(hostCodigo);

        Codigo = codigo;
        DispositivoCodigo = dispositivoCodigo;
        HostCodigo = hostCodigo;
        Vigencia = vigencia;
    }

    /// <inheritdoc/>
    public string ClaveExclusividad => HostCodigo;

    /// <summary>
    /// Cierra la cobertura en el instante dado (el host deja de estar protegido por este SAI). El
    /// mismo instante abre la cobertura siguiente en una sucesión sin hueco (RC-03).
    /// </summary>
    public void Cerrar(DateTimeOffset hasta) => Vigencia = Vigencia.CerrarEn(hasta);
}
