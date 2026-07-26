namespace SAI.Service.Core.Domain.Intervenciones;

/// <summary>
/// Tipo de intervención sobre el SAI (CU-09): reparación (el equipo vuelve tras arreglarse) o
/// sustitución/reemplazo (el equipo se retira definitivamente). Determina el estado en que queda el
/// equipo saliente: <b>en reparación</b> o <b>dado de baja</b>.
/// <para>Los valores arrancan en 1: el <c>default</c> (0) no es un tipo válido.</para>
/// </summary>
public enum TipoIntervencionSai
{
    /// <summary>El SAI se va a reparación: queda «en reparación» y puede volver luego.</summary>
    Reparacion = 1,

    /// <summary>El SAI se reemplaza: queda «dado de baja» con su disposición final.</summary>
    Reemplazo = 2,
}
