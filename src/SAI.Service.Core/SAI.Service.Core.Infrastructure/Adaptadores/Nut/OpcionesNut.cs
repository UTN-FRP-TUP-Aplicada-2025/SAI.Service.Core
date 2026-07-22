namespace SAI.Service.Core.Infrastructure.Adaptadores.Nut;

/// <summary>
/// Configuración del adaptador NUT (ADR-01, ADR-25). Se enlaza desde la sección <c>Sai:Nut</c> de
/// la configuración. En el despliegue el servidor NUT corre en el mismo contenedor y recibe el USB
/// por ruta física (ADR-03, ADR-19); en desarrollo apunta a un servidor NUT accesible.
/// </summary>
public sealed class OpcionesNut
{
    /// <summary>Nombre de la sección de configuración.</summary>
    public const string Seccion = "Sai:Nut";

    /// <summary>Host del servidor NUT (upsd). Por defecto localhost.</summary>
    public string Host { get; set; } = "127.0.0.1";

    /// <summary>Puerto TCP del servidor NUT (protocolo estándar 3493).</summary>
    public int Puerto { get; set; } = 3493;

    /// <summary>Nombre del UPS declarado en el servidor NUT (p. ej. "sai").</summary>
    public string Ups { get; set; } = "sai";

    /// <summary>Tiempo máximo de espera de cada operación de red, en segundos.</summary>
    public int TimeoutSegundos { get; set; } = 5;

    /// <summary>
    /// Usuario NUT con permiso de escritura (rol administrador/master en <c>upsd.users</c>), necesario
    /// para los comandos de apagado (US-14). Vacío = solo lectura anónima (no se puede ordenar apagado).
    /// </summary>
    public string Usuario { get; set; } = string.Empty;

    /// <summary>Clave del usuario NUT de escritura. Vacía = solo lectura anónima.</summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>Verdadero si hay credenciales de escritura configuradas (usuario y clave).</summary>
    public bool TieneCredencialesEscritura =>
        !string.IsNullOrWhiteSpace(Usuario) && !string.IsNullOrWhiteSpace(Password);
}
