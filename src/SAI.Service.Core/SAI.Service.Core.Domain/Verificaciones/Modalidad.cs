namespace SAI.Service.Core.Domain.Verificaciones;

/// <summary>
/// Modalidad de operación del servicio ante un corte (ADR-10). <see cref="SoloAlerta"/> es el
/// estado base seguro impuesto por diseño (RN-01): solo avisa, no apaga nada. Las modalidades con
/// acción solo se habilitan cuando los cuatro supuestos están verificados (RN-02).
/// <para>Los valores arrancan en 1: el <c>default</c> (0) no es una modalidad válida.</para>
/// </summary>
public enum Modalidad
{
    /// <summary>Solo aviso: no ejecuta ningún apagado. Estado base y degradado seguro (RN-01, RN-02).</summary>
    SoloAlerta = 1,

    /// <summary>Apaga el host de forma ordenada con retorno al volver la energía.</summary>
    ApagarHostConRetorno = 2,

    /// <summary>Apaga el host y luego el propio SAI, ambos con retorno.</summary>
    ApagarHostLuegoUpsConRetorno = 3,
}
