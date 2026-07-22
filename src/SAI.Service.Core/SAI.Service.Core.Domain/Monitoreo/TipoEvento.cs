namespace SAI.Service.Core.Domain.Monitoreo;

/// <summary>
/// Tipo de evento derivado del sondeo (US-09, CU-04). Cada evento se deriva por una
/// <see cref="ReglaDerivacion"/> versionada (RC-09) sobre las muestras.
/// <para>Los valores arrancan en 1: el <c>default</c> (0) no es un tipo válido.</para>
/// </summary>
public enum TipoEvento
{
    /// <summary>Corte de suministro: el equipo pasó a batería (ups.status OL → OB).</summary>
    CorteSuministro = 1,

    /// <summary>Retorno de la red: el equipo volvió a línea (ups.status OB → OL).</summary>
    RetornoRed = 2,

    /// <summary>Microcorte: un corte breve (duración por debajo del umbral), con incertidumbre estructural.</summary>
    Microcorte = 3,

    /// <summary>Desconexión de la herramienta de acceso: sondeos perdidos consecutivos (pérdida de comunicación).</summary>
    DesconexionUsb = 4,

    /// <summary>Tensión de entrada fuera de rango sostenida.</summary>
    TensionFueraDeRango = 5,

    /// <summary>Se cumplió la condición de disparo del apagado (BT-20). En solo aviso no ejecuta apagado.</summary>
    DisparoApagado = 6,
}
