namespace SAI.Service.Core.Domain.Valores;

/// <summary>
/// Objeto de valor que ata un contenido a su <see cref="Origen"/> (procedencia).
/// Materializa el invariante <b>I-7</b> (ADR-06, RN-05): <b>no se puede construir un
/// valor de dominio sin declarar su procedencia</b>.
/// <para>
/// El constructor exige el <see cref="Origen"/> (no hay sobrecarga que lo omita: pedirlo
/// de menos no compila) y rechaza un origen no definido — por ejemplo el <c>default</c>
/// del <see cref="Origen"/>, que es 0 y no corresponde a ninguna procedencia.
/// </para>
/// <para>
/// Es un <c>readonly record struct</c> para tener semántica de valor (igualdad estructural,
/// inmutabilidad). Se conserva el marcador de framework-free: sin EF ni ASP.NET acá.
/// </para>
/// </summary>
/// <typeparam name="T">Tipo del contenido transportado (temperatura, porcentaje, importe, etc.).</typeparam>
public readonly record struct Valor<T>
{
    /// <summary>Contenido del valor (el dato en sí).</summary>
    public T Contenido { get; }

    /// <summary>Procedencia declarada del <see cref="Contenido"/> (invariante I-7).</summary>
    public Origen Origen { get; }

    /// <summary>
    /// Construye un valor con su procedencia obligatoria.
    /// </summary>
    /// <param name="contenido">El dato transportado.</param>
    /// <param name="origen">La procedencia declarada; debe ser un valor definido del enum.</param>
    /// <exception cref="ArgumentException">Si <paramref name="origen"/> no es una procedencia definida.</exception>
    public Valor(T contenido, Origen origen)
    {
        if (!Enum.IsDefined(origen))
        {
            throw new ArgumentException(
                "Todo valor de dominio debe declarar una procedencia válida (invariante I-7).",
                nameof(origen));
        }

        Contenido = contenido;
        Origen = origen;
    }
}
