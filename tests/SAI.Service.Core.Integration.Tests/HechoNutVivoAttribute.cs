using Xunit;

namespace SAI.Service.Core.Integration.Tests;

/// <summary>
/// Marca una prueba que habla con un servidor NUT <b>real</b>. Se omite por defecto (no hay upsd en
/// CI) y solo corre si la variable de entorno <c>SAI_NUT_LIVE=1</c> está presente. Para ejecutarla
/// contra el SAI de desarrollo, el contenedor de pruebas necesita red al upsd del host
/// (p. ej. <c>--network host</c>).
/// </summary>
public sealed class HechoNutVivoAttribute : FactAttribute
{
    /// <summary>Crea el atributo, marcándolo como omitido salvo que <c>SAI_NUT_LIVE=1</c>.</summary>
    public HechoNutVivoAttribute()
    {
        if (Environment.GetEnvironmentVariable("SAI_NUT_LIVE") != "1")
        {
            Skip = "Prueba en vivo contra un servidor NUT real: definí SAI_NUT_LIVE=1 (con red al upsd) para ejecutarla.";
        }
    }
}
