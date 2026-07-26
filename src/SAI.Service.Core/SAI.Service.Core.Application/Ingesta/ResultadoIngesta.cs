namespace SAI.Service.Core.Application.Ingesta;

/// <summary>
/// Resultado de negocio de una ingesta (CU-11), que el endpoint mapea a los cuatro caminos HTTP del
/// contrato (ADR-17): <see cref="Creado"/>→201, <see cref="Reintento"/>→200,
/// <see cref="ConflictoIdempotencia"/>→409, y <see cref="Validacion"/>/<see cref="CoherenciaTemporal"/>/
/// <see cref="FuenteNoRegistrada"/>→422.
/// </summary>
public enum CodigoIngesta
{
    /// <summary>Clave nueva y cuerpo válido: se creó el registro (201).</summary>
    Creado = 1,

    /// <summary>Misma clave con el mismo cuerpo: se devuelve el registro existente sin duplicar (200).</summary>
    Reintento = 2,

    /// <summary>Misma clave con cuerpo distinto: conflicto, nunca se aplica (409).</summary>
    ConflictoIdempotencia = 3,

    /// <summary>Invariante de validación roto: cuadre de costos o dinero sin moneda/fecha (422).</summary>
    Validacion = 4,

    /// <summary>Intervención fechada después de la baja de una unidad (422, RN-12).</summary>
    CoherenciaTemporal = 5,

    /// <summary>La fuente de datos del encabezado no está registrada (422).</summary>
    FuenteNoRegistrada = 6,
}

/// <summary>Resultado de <see cref="ServicioIngesta.IngerirAsync"/> con lo que el endpoint necesita para
/// armar la respuesta (id creado, huellas del conflicto, campo/invariante del 422).</summary>
public sealed record ResultadoIngesta(
    CodigoIngesta Codigo,
    string? Id = null,
    DateTimeOffset? TiempoValido = null,
    DateTimeOffset? TiempoRegistrado = null,
    string? Confianza = null,
    string? HuellaOriginal = null,
    string? HuellaRecibida = null,
    string? AccionSugerida = null,
    string? Campo = null,
    string? Invariante = null,
    string? Detalle = null);
