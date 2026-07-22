namespace SAI.Service.Core.Application.Abstractions;

/// <summary>
/// Instantanea minima del estado del SAI leida por el adaptador de conexion.
/// <para>
/// Placeholder de Sprint 0: los campos son razonables pero el detalle real
/// (procedencia por variable, dialecto, sesion de sondeo) se completa en etapas
/// posteriores (§17.P.4, ADR-06). No es una entidad de dominio: es el resultado
/// de transporte del puerto <see cref="IAdaptadorConexion"/>.
/// </para>
/// </summary>
/// <param name="Alcanzable">Si el equipo respondio la lectura (validado por efecto observado, ADR-11).</param>
/// <param name="TensionEntradaVoltios">Tension de entrada (input.voltage) en voltios, si esta disponible.</param>
/// <param name="TensionSalidaVoltios">Tension de salida (output.voltage) en voltios, si esta disponible.</param>
/// <param name="CargaSalidaPorcentaje">Carga de salida (ups.load) en porcentaje, si esta disponible.</param>
/// <param name="CargaBateriaPorcentaje">Carga de bateria (battery.charge) en porcentaje; siempre derivada (RN-05).</param>
/// <param name="EstadoUps">Estado de alimentacion (ups.status: en linea / en bateria), base de los eventos de corte.</param>
/// <param name="TensionBateriaVoltios">Tension de bateria (battery.voltage) en voltios; medida. Base del disparo BT-20 (ADR-12).</param>
/// <param name="MarcaTiempoUtc">Instante de la lectura en UTC.</param>
public sealed record EstadoSai(
    bool Alcanzable,
    double? TensionEntradaVoltios,
    double? TensionSalidaVoltios,
    double? CargaSalidaPorcentaje,
    double? CargaBateriaPorcentaje,
    EstadoUps? EstadoUps,
    double? TensionBateriaVoltios,
    DateTimeOffset MarcaTiempoUtc);
