using SAI.Service.Core.Domain.Acciones;
using SAI.Service.Core.Domain.Verificaciones;

namespace SAI.Service.Core.Application.Politicas;

/// <summary>Tipo de un parámetro de configuración, para que la UI sepa cómo renderizarlo.</summary>
public enum TipoParametro
{
    /// <summary>Valor numérico entero (con unidad y límites).</summary>
    Numerico = 1,

    /// <summary>Selección de un conjunto cerrado de opciones (enum).</summary>
    Seleccion = 2,
}

/// <summary>Una opción de un parámetro de selección: el valor y su etiqueta legible.</summary>
public sealed record OpcionParametro(string Valor, string Etiqueta);

/// <summary>
/// Descriptor de un parámetro de política: la fuente única de su etiqueta, ayuda, tipo, unidad, default y
/// límites. La pantalla <b>lee</b> el descriptor, no lo hardcodea (config dirigida por descriptores).
/// </summary>
public sealed record DescriptorParametro(
    string Clave,
    string Etiqueta,
    string Ayuda,
    TipoParametro Tipo,
    string Unidad,
    int? Minimo = null,
    int? Maximo = null,
    IReadOnlyList<OpcionParametro>? Opciones = null);

/// <summary>Un preset nombrado: compone una propuesta de política coherente (wireframe §4).</summary>
public sealed record Preset(string Nombre, string Descripcion, PropuestaPolitica Propuesta);

/// <summary>
/// Metadatos de la configuración de políticas (CU-03): los descriptores de cada parámetro y los presets.
/// De acá salen los campos del formulario, sus límites (incluido el techo duro de
/// <see cref="Accion.TechoDuroApagadoSeg"/> como máximo del tiempo reservado) y los presets.
/// </summary>
public static class DescriptorPoliticas
{
    private const int UmbralDefault = 300;
    private const int TiempoReservadoDefault = 120;
    private const int TiempoReservadoMinimo = 12;

    /// <summary>Descriptores de los parámetros configurables.</summary>
    public static IReadOnlyList<DescriptorParametro> Parametros { get; } =
    [
        new DescriptorParametro(
            "modalidad", "Modalidad de apagado",
            "Qué hace el sistema ante un corte prolongado.",
            TipoParametro.Seleccion, "",
            Opciones:
            [
                new OpcionParametro(nameof(Modalidad.SoloAlerta), "Solo aviso"),
                new OpcionParametro(nameof(Modalidad.ApagarHostConRetorno), "Apagar el host con retorno"),
                new OpcionParametro(nameof(Modalidad.ApagarHostLuegoUpsConRetorno), "Apagar el host y luego el SAI, con retorno"),
                new OpcionParametro(nameof(Modalidad.CicloForzado), "Ciclo forzado (no se cancela al volver la red)"),
            ]),
        new DescriptorParametro(
            "umbralDisparoSegundos", "Umbral de disparo",
            "Cuántos segundos en batería deben pasar antes de disparar el apagado.",
            TipoParametro.Numerico, "s", Minimo: 1),
        new DescriptorParametro(
            "tiempoReservadoApagadoSeg", "Tiempo reservado para el apagado",
            $"Cuánto tiempo se le reserva al host para apagarse. Entre {TiempoReservadoMinimo} y {Accion.TechoDuroApagadoSeg} s (techo duro del equipo).",
            TipoParametro.Numerico, "s", Minimo: TiempoReservadoMinimo, Maximo: Accion.TechoDuroApagadoSeg),
    ];

    /// <summary>Propuesta por defecto (arranque seguro en solo aviso, RN-01).</summary>
    public static PropuestaPolitica Defecto { get; } =
        new(Modalidad.SoloAlerta, UmbralDefault, TiempoReservadoDefault);

    /// <summary>Los tres presets nombrados por su modalidad (wireframe §2).</summary>
    public static IReadOnlyList<Preset> Presets { get; } =
    [
        new Preset("Solo aviso", "El sistema solo avisa; no apaga el host.",
            new PropuestaPolitica(Modalidad.SoloAlerta, UmbralDefault, TiempoReservadoDefault)),
        new Preset("Apagado con retorno", "Apaga el host ordenadamente y lo repone al volver la energía.",
            new PropuestaPolitica(Modalidad.ApagarHostConRetorno, UmbralDefault, TiempoReservadoDefault)),
        new Preset("Ciclo forzado", "Apaga host y SAI; el corte no se cancela aunque vuelva la red.",
            new PropuestaPolitica(Modalidad.CicloForzado, UmbralDefault, TiempoReservadoDefault)),
    ];
}
