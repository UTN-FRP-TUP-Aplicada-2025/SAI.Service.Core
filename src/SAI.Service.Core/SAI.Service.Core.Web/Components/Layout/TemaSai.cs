using MudBlazor;

namespace SAI.Service.Core.Web.Components.Layout;

/// <summary>
/// Tema visual del panel, con la paleta y la tipografía de la maqueta aprobada
/// (tokens del catálogo de diseño materializados en SDD/Maquetas/.../Estilos-Maqueta.css:
/// primario verde #0F6E56, chrome de marca oscuro #04342C, barra superior blanca,
/// estados semánticos ámbar/rojo/azul, fuente Inter). Reemplaza el tema violeta
/// por defecto de MudBlazor. Lo consumen MainLayout y LayoutAcceso.
/// </summary>
public static class TemaSai
{
    public static readonly MudTheme Tema = new()
    {
        PaletteLight = new PaletteLight
        {
            // Marca / acento (rampa verde de la maqueta)
            Primary = "#0F6E56",
            PrimaryDarken = "#04342C",
            PrimaryLighten = "#E1F5EE",
            Secondary = "#185FA5",
            Tertiary = "#854F0B",

            // Estados semánticos (texto de la maqueta; el bg lo aporta MudBlazor)
            Success = "#0F6E56",
            Warning = "#854F0B",
            Error = "#A03030",
            Info = "#185FA5",

            // Superficies y texto
            Background = "#F1F1EF",
            Surface = "#FFFFFF",
            TextPrimary = "#1A1A18",
            TextSecondary = "#5C5C57",
            TextDisabled = "#8A8A82",

            // Barra superior: blanca con texto oscuro (mq-topbar)
            AppbarBackground = "#FFFFFF",
            AppbarText = "#1A1A18",

            // Barra lateral: chrome oscuro de marca con texto atenuado (mq-chrome)
            DrawerBackground = "#04342C",
            DrawerText = "rgba(255, 255, 255, 0.72)",
            DrawerIcon = "rgba(255, 255, 255, 0.72)",

            // Líneas y divisores
            LinesDefault = "#E6E6E1",
            LinesInputs = "#D9D9D4",
            TableLines = "#E6E6E1",
            Divider = "#E6E6E1",
            ActionDefault = "#5C5C57",
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "10px",
            DrawerWidthLeft = "260px",
        },
        Typography = new Typography
        {
            Default = new DefaultTypography
            {
                FontFamily = ["Inter", "system-ui", "-apple-system", "Segoe UI", "Roboto", "sans-serif"],
            },
        },
    };
}
