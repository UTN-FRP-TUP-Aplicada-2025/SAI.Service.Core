namespace SAI.Service.Core.Domain.Monitoreo;

/// <summary>
/// Confianza base de una <see cref="FuenteDatos"/> (de dónde provienen las lecturas). Modula la
/// confianza de las conclusiones que se apoyan en ella (p. ej. la salud de la batería).
/// <para>Los valores arrancan en 1: el <c>default</c> (0) no es una confianza válida.</para>
/// </summary>
public enum ConfianzaFuente
{
    /// <summary>Fuente de alta confianza.</summary>
    Alta = 1,

    /// <summary>Fuente de confianza media.</summary>
    Media = 2,

    /// <summary>Fuente de baja confianza.</summary>
    Baja = 3,
}
