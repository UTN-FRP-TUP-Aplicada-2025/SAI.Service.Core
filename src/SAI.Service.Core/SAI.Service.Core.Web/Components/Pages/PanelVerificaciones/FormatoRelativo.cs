using System.Globalization;

namespace SAI.Service.Core.Web.Components.Pages.PanelVerificaciones;

/// <summary>
/// Formato de fechas y tiempos del panel (SPEC 5.3, H-6): fecha absoluta localizada + tiempo relativo
/// entre paréntesis. Nunca ISO crudo en la UI (el ISO queda para el <c>title</c>). Relativa granular:
/// días si falta poco, meses o años si falta más.
/// </summary>
public static class FormatoRelativo
{
    private static readonly CultureInfo EsAr = CultureInfo.GetCultureInfo("es-AR");

    /// <summary>Fecha absoluta localizada, p. ej. «18 ene 2027».</summary>
    public static string FechaCorta(DateTimeOffset fecha) => fecha.ToString("d MMM yyyy", EsAr);

    /// <summary>Fecha ISO (UTC) para tooltips/copia; nunca visible en el cuerpo.</summary>
    public static string Iso(DateTimeOffset fecha) => fecha.UtcDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    /// <summary>Tiempo relativo a partir de los días restantes, p. ej. «faltan 6 meses», «venció hace 3 días».</summary>
    public static string Relativo(int diasRestantes)
    {
        if (diasRestantes < 0)
        {
            return $"venció {HaceFalta(-diasRestantes, "hace")}";
        }

        return diasRestantes == 0 ? "vence hoy" : $"{HaceFalta(diasRestantes, "falta")}";
    }

    /// <summary>Vencimiento completo: «vigente hasta el 18 ene 2027 (faltan 6 meses)» o «sin caducidad».</summary>
    public static string Vencimiento(DateTimeOffset? fecha, int? diasRestantes)
    {
        if (fecha is not { } f)
        {
            return "sin caducidad";
        }

        return diasRestantes is { } d
            ? $"vigente hasta el {FechaCorta(f)} ({Relativo(d)})"
            : $"vigente hasta el {FechaCorta(f)}";
    }

    // Construye «faltan N días/meses/1 año» o «hace N días/meses…», eligiendo la granularidad.
    private static string HaceFalta(int dias, string verbo)
    {
        if (dias < 60)
        {
            return dias == 1 ? $"{Conjuga(verbo, singular: true)} 1 día" : $"{Conjuga(verbo, singular: false)} {dias} días";
        }

        if (dias < 365)
        {
            var meses = (int)Math.Round(dias / 30.0);
            return meses == 1 ? $"{Conjuga(verbo, singular: true)} 1 mes" : $"{Conjuga(verbo, singular: false)} {meses} meses";
        }

        var anios = (int)Math.Round(dias / 365.0);
        return anios == 1 ? $"{Conjuga(verbo, singular: true)} 1 año" : $"{Conjuga(verbo, singular: false)} {anios} años";
    }

    // "falta"/"faltan" según número; "hace" es invariable.
    private static string Conjuga(string verbo, bool singular) =>
        verbo == "falta" ? (singular ? "falta" : "faltan") : verbo;
}
