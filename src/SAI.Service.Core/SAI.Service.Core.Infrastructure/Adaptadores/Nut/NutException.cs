namespace SAI.Service.Core.Infrastructure.Adaptadores.Nut;

/// <summary>
/// Falla de <b>transporte</b> hablando con el servidor NUT (no se pudo conectar, timeout, respuesta
/// de error del protocolo). Representa que no se pudo observar el equipo, no un veredicto sobre él:
/// el adaptador la traduce a un resultado "no alcanzable / no conectado" (validación por efecto
/// observado, ADR-11). El sufijo "Exception" cumple la convención de .NET (CA1710).
/// </summary>
public sealed class NutException : Exception
{
    /// <summary>Crea la excepción con un mensaje.</summary>
    public NutException(string mensaje) : base(mensaje)
    {
    }

    /// <summary>Crea la excepción con un mensaje y la causa subyacente.</summary>
    public NutException(string mensaje, Exception interna) : base(mensaje, interna)
    {
    }
}
