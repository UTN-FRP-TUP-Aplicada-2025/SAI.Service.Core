using SAI.Service.Core.Domain.Verificaciones;

namespace SAI.Service.Core.Application.Equipos;

/// <summary>
/// Estado de puesta en marcha para el aviso permanente del panel (US-05): la modalidad efectiva y el
/// conteo «n de 4 supuestos verificados». Mientras no estén los cuatro verificados, la modalidad
/// efectiva es <see cref="Modalidad.SoloAlerta"/> y el panel muestra el aviso de degradación con el
/// enlace a la ventana de mantenimiento (no enterrado en configuración).
/// </summary>
/// <param name="ModalidadEfectiva">Modalidad efectiva derivada de las verificaciones (RN-02).</param>
/// <param name="SupuestosVerificados">Cantidad de supuestos verificados.</param>
/// <param name="SupuestosTotales">Cantidad total de supuestos (4).</param>
/// <param name="HayEquipos">Verdadero si ya hay al menos un equipo dado de alta.</param>
public sealed record EstadoPuestaEnMarcha(
    Modalidad ModalidadEfectiva,
    int SupuestosVerificados,
    int SupuestosTotales,
    bool HayEquipos)
{
    /// <summary>Verdadero si el servicio está degradado a solo aviso por supuestos sin verificar.</summary>
    public bool Degradado => ModalidadEfectiva == Modalidad.SoloAlerta;
}
