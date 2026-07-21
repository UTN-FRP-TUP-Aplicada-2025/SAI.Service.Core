using System.Text;

namespace SAI.Service.Core.Infrastructure.Adaptadores.Nut;

/// <summary>
/// Parseo puro del protocolo de red de NUT (Network UPS Tools), separado del transporte TCP para
/// poder probarlo sin socket. El protocolo es textual y line-based (RFC 9271):
/// <list type="bullet">
///   <item><c>GET VAR &lt;ups&gt; &lt;var&gt;</c> → <c>VAR &lt;ups&gt; &lt;var&gt; "&lt;valor&gt;"</c> o <c>ERR &lt;codigo&gt;</c>.</item>
///   <item><c>LIST VAR &lt;ups&gt;</c> → líneas <c>VAR &lt;ups&gt; &lt;var&gt; "&lt;valor&gt;"</c> entre BEGIN/END.</item>
///   <item><c>LIST UPS</c> → líneas <c>UPS &lt;nombre&gt; "&lt;descripcion&gt;"</c> entre BEGIN/END.</item>
/// </list>
/// Los valores van entre comillas dobles; dentro se escapan <c>"</c> como <c>\"</c> y <c>\</c> como <c>\\</c>.
/// </summary>
public static class ProtocoloNut
{
    /// <summary>Verdadero si la línea es una respuesta de error del servidor (<c>ERR ...</c>).</summary>
    public static bool EsError(string linea) => linea.StartsWith("ERR ", StringComparison.Ordinal) || linea == "ERR";

    /// <summary>
    /// Extrae el valor de una línea <c>VAR &lt;ups&gt; &lt;variable&gt; "&lt;valor&gt;"</c>. Devuelve
    /// <c>null</c> si la línea no corresponde a esa variable (o es un error / no soportada).
    /// </summary>
    public static string? LeerValorDeVar(string linea, string ups, string variable)
    {
        var prefijo = $"VAR {ups} {variable} ";
        if (!linea.StartsWith(prefijo, StringComparison.Ordinal))
        {
            return null;
        }

        return DesescaparEntreComillas(linea.AsSpan(prefijo.Length));
    }

    /// <summary>
    /// Parsea una línea <c>VAR &lt;ups&gt; &lt;variable&gt; "&lt;valor&gt;"</c> de un <c>LIST VAR</c>,
    /// devolviendo <c>(variable, valor)</c>, o <c>null</c> si no es una línea VAR.
    /// </summary>
    public static (string Variable, string Valor)? ParsearLineaVar(string linea, string ups)
    {
        var prefijo = $"VAR {ups} ";
        if (!linea.StartsWith(prefijo, StringComparison.Ordinal))
        {
            return null;
        }

        var resto = linea.AsSpan(prefijo.Length);
        var espacio = resto.IndexOf(' ');
        if (espacio < 0)
        {
            return null;
        }

        var variable = resto[..espacio].ToString();
        var valor = DesescaparEntreComillas(resto[(espacio + 1)..]);
        return valor is null ? null : (variable, valor);
    }

    /// <summary>
    /// Parsea una línea <c>UPS &lt;nombre&gt; "&lt;descripcion&gt;"</c> de un <c>LIST UPS</c>,
    /// devolviendo <c>(nombre, descripcion)</c>, o <c>null</c> si no es una línea UPS.
    /// </summary>
    public static (string Nombre, string Descripcion)? ParsearLineaUps(string linea)
    {
        const string prefijo = "UPS ";
        if (!linea.StartsWith(prefijo, StringComparison.Ordinal))
        {
            return null;
        }

        var resto = linea.AsSpan(prefijo.Length);
        var espacio = resto.IndexOf(' ');
        if (espacio < 0)
        {
            return null;
        }

        var nombre = resto[..espacio].ToString();
        var descripcion = DesescaparEntreComillas(resto[(espacio + 1)..]) ?? string.Empty;
        return (nombre, descripcion);
    }

    /// <summary>
    /// Desescapa un valor NUT entre comillas dobles (el primer par de comillas del tramo). Devuelve
    /// <c>null</c> si el tramo no empieza con comillas.
    /// </summary>
    private static string? DesescaparEntreComillas(ReadOnlySpan<char> tramo)
    {
        if (tramo.Length == 0 || tramo[0] != '"')
        {
            return null;
        }

        var sb = new StringBuilder(tramo.Length);
        for (var i = 1; i < tramo.Length; i++)
        {
            var c = tramo[i];
            if (c == '\\' && i + 1 < tramo.Length)
            {
                sb.Append(tramo[++i]);
            }
            else if (c == '"')
            {
                return sb.ToString();
            }
            else
            {
                sb.Append(c);
            }
        }

        // Comilla de cierre ausente: devolvemos lo acumulado (tolerante).
        return sb.ToString();
    }
}
