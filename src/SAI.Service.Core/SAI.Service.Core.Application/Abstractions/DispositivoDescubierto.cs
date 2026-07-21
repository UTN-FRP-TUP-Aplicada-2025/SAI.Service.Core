namespace SAI.Service.Core.Application.Abstractions;

/// <summary>
/// Descriptor de un dispositivo (SAI) descubierto en el bus (US-03, CU-02 paso 2). Es un resultado
/// de transporte para que el panel liste los candidatos con lo que el equipo expone; lo que el
/// equipo no expone (marca, modelo, número de serie) queda en <c>null</c> y el administrador lo
/// declara a mano con procedencia <c>Declarado</c> (el modelo no siempre es legible del dispositivo).
/// </summary>
/// <param name="NombreNut">Nombre del UPS tal como lo conoce el servidor NUT (p. ej. "sai").</param>
/// <param name="Descriptor">Descriptor legible para el panel (p. ej. "0665:5161 · UPS INNO TECH · serie: vacío").</param>
/// <param name="VendorId">Identificador de fabricante USB (p. ej. "0665"), si está disponible.</param>
/// <param name="ProductId">Identificador de producto USB (p. ej. "5161"), si está disponible.</param>
/// <param name="Driver">Driver de la herramienta de acceso (p. ej. "nutdrv_qx"), si está disponible.</param>
/// <param name="NumeroSerie">Número de serie (iSerial), si el equipo lo expone; nulo/vacío si no.</param>
public sealed record DispositivoDescubierto(
    string NombreNut,
    string Descriptor,
    string? VendorId,
    string? ProductId,
    string? Driver,
    string? NumeroSerie);
