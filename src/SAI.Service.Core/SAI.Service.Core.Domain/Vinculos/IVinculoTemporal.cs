namespace SAI.Service.Core.Domain.Vinculos;

/// <summary>
/// Vínculo temporal entre unidades físicas (ADR-05, RC-02): un montaje de batería o una
/// cobertura de host, que valen durante una <see cref="Vigencia"/> y son excluyentes por una
/// clave (I-1, I-2, I-4). La abstracción permite validar el no solapamiento con una sola regla
/// (<see cref="Vigencias.AdmiteNuevo{T}"/>) para ambos tipos de vínculo.
/// </summary>
public interface IVinculoTemporal
{
    /// <summary>Intervalo durante el cual el vínculo vale.</summary>
    Vigencia Vigencia { get; }

    /// <summary>
    /// Clave por la que el vínculo es excluyente en el tiempo: dos vínculos con la misma clave no
    /// pueden solapar (montaje: dispositivo + posición; cobertura: host).
    /// </summary>
    string ClaveExclusividad { get; }
}
