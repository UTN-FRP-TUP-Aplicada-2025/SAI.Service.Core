namespace SAI.Service.Core.Application.Monitoreo;

/// <summary>
/// Parámetros de la prueba de batería (US-12, US-13). La serie se recoge a cadencia densa durante
/// una ventana corta; en producción a 1 Hz, en pruebas con intervalo cero para no demorar.
/// </summary>
public sealed class OpcionesPrueba
{
    /// <summary>Nombre de la sección de configuración.</summary>
    public const string Seccion = "Sai:Prueba";

    /// <summary>Cantidad de muestras densas que recoge la prueba.</summary>
    public int NumeroMuestras { get; set; } = 15;

    /// <summary>Intervalo entre muestras densas, en milisegundos (1000 = 1 Hz).</summary>
    public int IntervaloMuestraMs { get; set; } = 1000;

    /// <summary>Tiempo mínimo en flotación tras el último corte para poder probar, en segundos.</summary>
    public int FlotacionMinimaSeg { get; set; } = 300;

    /// <summary>Tolerancia de carga concurrente para considerar dos pruebas comparables, en puntos porcentuales.</summary>
    public int ToleranciaCargaPct { get; set; } = 5;
}
