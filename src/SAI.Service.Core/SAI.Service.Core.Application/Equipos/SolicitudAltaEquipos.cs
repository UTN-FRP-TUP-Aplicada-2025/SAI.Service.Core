using SAI.Service.Core.Domain.Valores;

namespace SAI.Service.Core.Application.Equipos;

/// <summary>
/// Datos declarados por el administrador para dar de alta un equipo (CU-02, US-04). Reúne el
/// catálogo (fabricante y modelos), el inventario (host, dispositivo, batería) y la posición del
/// montaje. El descubrimiento y la prueba de conexión (US-03) ocurren antes, en el panel; acá llega
/// lo ya declarado. La potencia nominal puede venir desconocida (procedencia <c>Imputado</c>, no
/// bloquea el alta, FA-1).
/// </summary>
/// <param name="Instante">Momento del alta: inicio de las vigencias y sello de la siembra.</param>
public sealed record SolicitudAltaEquipos(
    DateTimeOffset Instante,
    DatosFabricante Fabricante,
    DatosModeloDispositivo ModeloDispositivo,
    DatosModeloBateria ModeloBateria,
    DatosHost Host,
    DatosDispositivo Dispositivo,
    DatosBateria Bateria,
    string Posicion);

/// <summary>Fabricante declarado (catálogo).</summary>
public sealed record DatosFabricante(string Codigo, string Nombre, bool Identificado = true);

/// <summary>Modelo de dispositivo declarado. <paramref name="PotenciaVaNominal"/> nulo = desconocida.</summary>
public sealed record DatosModeloDispositivo(
    string Codigo,
    string Nombre,
    string? LineaTopologia = null,
    double? TensionNominalV = null,
    Valor<double>? PotenciaVaNominal = null);

/// <summary>Modelo de batería declarado. Vida de flotación exige temperatura de referencia (I-21).</summary>
public sealed record DatosModeloBateria(
    string Codigo,
    string Nombre,
    string? Tecnologia = null,
    double? CapacidadAh = null,
    double? TensionNominalV = null,
    double? VidaFlotacionAniosMin = null,
    double? VidaFlotacionAniosMax = null,
    double? TemperaturaReferenciaC = null);

/// <summary>Host protegido declarado (inventario).</summary>
public sealed record DatosHost(string Codigo, string? Criticidad = null);

/// <summary>Dispositivo (SAI) declarado (inventario). El número de serie puede faltar (CA-01).</summary>
public sealed record DatosDispositivo(string Codigo, string? NumeroSerie = null);

/// <summary>Batería declarada (inventario). La edad se cuenta desde la fabricación (FA-2).</summary>
public sealed record DatosBateria(string Codigo, DateOnly? FechaFabricacion = null, DateOnly? FechaCompra = null);
