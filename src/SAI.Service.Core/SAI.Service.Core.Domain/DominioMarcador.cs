namespace SAI.Service.Core.Domain;

/// <summary>
/// Marcador del assembly de dominio (nucleo de Clean Architecture, ADR-15).
/// <para>
/// El dominio es <b>framework-free</b>: no referencia EF Core, ASP.NET, Blazor ni NUT.
/// En etapas posteriores aloja entidades, objetos de valor, invariantes I-1 a I-21,
/// el <c>ResolutorTemporal</c> y los veredictos. En Sprint 0 esta intencionalmente vacio:
/// es andamiaje, sin logica de negocio.
/// </para>
/// <para>
/// Se conserva como tipo publico para poder referenciar el assembly (por ejemplo,
/// <c>typeof(DominioMarcador).Assembly</c>) desde pruebas de arquitectura y de dependencias.
/// </para>
/// </summary>
public static class DominioMarcador
{
    /// <summary>Nombre de codigo de la capa, util para diagnostico.</summary>
    public const string Capa = "Domain";
}
