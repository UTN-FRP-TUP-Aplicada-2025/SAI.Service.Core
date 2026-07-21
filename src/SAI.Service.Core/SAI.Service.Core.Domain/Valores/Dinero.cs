namespace SAI.Service.Core.Domain.Valores;

/// <summary>
/// Importe monetario que <b>siempre</b> viaja con su moneda y su fecha (RN-07,
/// invariante I-18): "todo importe monetario declara su moneda y su fecha". El
/// constructor rechaza una moneda vacía o una fecha sin declarar; no hay forma de
/// construir un <see cref="Dinero"/> a medias.
/// <para>
/// Es un <c>readonly record struct</c> (semántica de valor, inmutable) y framework-free.
/// El equivalente normalizado a otra moneda —cuando exista— es un valor aparte marcado
/// como <see cref="Origen.Derivado"/> con su fuente de cotización; no se modela acá en la
/// Etapa 1 (solo la fundación monto+moneda+fecha).
/// </para>
/// </summary>
public readonly record struct Dinero
{
    /// <summary>Monto del importe.</summary>
    public decimal Monto { get; }

    /// <summary>Código de moneda (ISO-4217, p. ej. <c>ARS</c>, <c>USD</c>). Obligatorio.</summary>
    public string Moneda { get; }

    /// <summary>Fecha del importe (ISO-8601). Obligatoria.</summary>
    public DateOnly Fecha { get; }

    /// <summary>
    /// Construye un importe exigiendo moneda y fecha (RN-07).
    /// </summary>
    /// <param name="monto">Monto del importe.</param>
    /// <param name="moneda">Código de moneda; no puede ser nulo ni vacío.</param>
    /// <param name="fecha">Fecha del importe.</param>
    /// <exception cref="ArgumentException">Si <paramref name="moneda"/> es nula o en blanco.</exception>
    public Dinero(decimal monto, string moneda, DateOnly fecha)
    {
        if (string.IsNullOrWhiteSpace(moneda))
        {
            throw new ArgumentException(
                "Todo importe monetario debe declarar su moneda (RN-07 / invariante I-18).",
                nameof(moneda));
        }

        Monto = monto;
        Moneda = moneda;
        Fecha = fecha;
    }
}
