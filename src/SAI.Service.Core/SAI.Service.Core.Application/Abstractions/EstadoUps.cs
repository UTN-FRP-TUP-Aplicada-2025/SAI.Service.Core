namespace SAI.Service.Core.Application.Abstractions;

/// <summary>
/// Estado de alimentación del SAI leído de <c>ups.status</c> (DM-05). Es la base de la derivación de
/// eventos de corte/retorno (US-09): la transición <see cref="EnLinea"/> → <see cref="EnBateria"/>
/// arranca un corte de suministro; la inversa, el retorno de la red.
/// <para>Los valores arrancan en 1: el <c>default</c> (0) no es un estado válido.</para>
/// </summary>
public enum EstadoUps
{
    /// <summary>En línea (OL): alimentado por la red.</summary>
    EnLinea = 1,

    /// <summary>En batería (OB): la red cayó y el equipo alimenta desde la batería.</summary>
    EnBateria = 2,
}
