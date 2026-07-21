namespace SAI.Service.Core.Domain.Valores;

/// <summary>
/// Procedencia de un valor de dominio (ADR-06, RN-05). Todo <see cref="Valor{T}"/>
/// declara de dónde proviene su contenido; es el sustrato del invariante I-7
/// ("procedencia obligatoria de todo valor").
/// <para>
/// Los valores del enum arrancan en 1 a propósito: el 0 (valor por defecto de un
/// entero) NO corresponde a ninguna procedencia válida, de modo que un
/// <see cref="Valor{T}"/> con <c>Origen = default</c> es rechazable como inválido.
/// </para>
/// </summary>
public enum Origen
{
    /// <summary>Medición directa del equipo o de un sensor (dato observado).</summary>
    Medido = 1,

    /// <summary>Derivado por cálculo a partir de otros valores medidos o declarados.</summary>
    Derivado = 2,

    /// <summary>Declarado por el administrador (dato ingresado a mano).</summary>
    Declarado = 3,

    /// <summary>Imputado ante ausencia de dato (relleno con criterio explícito).</summary>
    Imputado = 4,

    /// <summary>Estimado por un driver/adaptador cuando el equipo no lo expone directamente.</summary>
    EstimadoPorDriver = 5,

    /// <summary>
    /// No calculable: el valor queda nulo pero con su procedencia declarada (ADR-06,
    /// "nunca un número inventado"). Se incluye para cubrir el conjunto canónico de
    /// ADR-06 / Modelo-Datos-Lógico, que define seis procedencias.
    /// </summary>
    NoCalculable = 6,
}
