namespace SAI.Service.Core.Application.Abstractions;

/// <summary>
/// Descubrimiento de dispositivos SAI en el bus (US-03, CU-02). Es una capacidad separada del
/// puerto de operación <see cref="IAdaptadorConexion"/> (cuyo contrato de cuatro operaciones cerró
/// ADR-27): descubrir enumera los candidatos que la herramienta de acceso conoce, antes de que el
/// dispositivo esté dado de alta. Lo implementan el adaptador NUT (real) y el simulado.
/// </summary>
public interface IDescubridorSai
{
    /// <summary>
    /// Enumera los dispositivos candidatos con sus descriptores. Lista vacía si no se descubrió
    /// ninguno (el flujo de alta lo informa como <c>DISPOSITIVO_NO_DESCUBIERTO</c> y no inventa un
    /// dispositivo, US-03).
    /// </summary>
    /// <param name="ct">Token de cancelación.</param>
    Task<IReadOnlyList<DispositivoDescubierto>> DescubrirAsync(CancellationToken ct);
}
