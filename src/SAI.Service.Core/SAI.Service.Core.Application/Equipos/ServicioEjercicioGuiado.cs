using SAI.Service.Core.Domain.Verificaciones;

namespace SAI.Service.Core.Application.Equipos;

/// <summary>
/// Estado de un ejercicio guiado: la sesión (si hay una abierta) y el <b>paso derivado</b> en el que va.
/// El paso no se persiste: se calcula a partir de las verificaciones, que son la única verdad.
/// </summary>
/// <param name="Sesion">Sesión en curso, o <c>null</c> si no hay ejercicio abierto.</param>
/// <param name="PasoActual">Primer supuesto pendiente, o <c>null</c> si los cuatro están vigentes.</param>
/// <param name="NumeroPaso">Número (1–4) del paso actual, o 0 si no hay pendientes.</param>
/// <param name="Total">Cantidad de pasos del ejercicio.</param>
public sealed record EstadoEjercicio(SesionEjercicio? Sesion, Supuesto? PasoActual, int NumeroPaso, int Total)
{
    /// <summary>Verdadero si hay un ejercicio guiado abierto.</summary>
    public bool HayEjercicio => Sesion is not null;
}

/// <summary>
/// Ejercicio guiado de la ventana de mantenimiento (P-7). Es una capa de <b>acompañamiento</b>: no
/// verifica nada por su cuenta ni cambia el proceso: cada paso se confirma con los métodos de
/// <see cref="ServicioVerificacion"/> que ya existen. Acá solo se registra que hay un ejercicio en curso
/// (intención y momento) y se deriva en qué paso va, para que la interfaz pueda guiar de a uno.
/// <para>
/// La sesión se persiste —no vive en el circuito Blazor— para que el ejercicio sobreviva al reinicio del
/// host, que es parte normal del ejercicio, y se pueda consultar desde cualquier navegador.
/// </para>
/// </summary>
public sealed class ServicioEjercicioGuiado(IRepositorioEquipos repositorio)
{
    /// <summary>
    /// Estado actual del ejercicio. Si la sesión abierta ya cumplió los cuatro supuestos, la cierra como
    /// completada (cierre perezoso: no hace falta un proceso aparte).
    /// </summary>
    public async Task<EstadoEjercicio> EstadoAsync(CancellationToken ct)
    {
        var sesion = await repositorio.SesionEjercicioEnCursoAsync(ct);
        var verificaciones = await repositorio.ListarVerificacionesAsync(ct);
        var ahora = DateTimeOffset.UtcNow;

        var pendiente = SecuenciaFisica.PrimeroPendiente(verificaciones, ahora);
        if (sesion is not null && pendiente is null)
        {
            sesion.Completar(ahora);
            await repositorio.ActualizarSesionEjercicioAsync(sesion, ct);
            sesion = null;
        }

        return new EstadoEjercicio(
            sesion,
            pendiente,
            pendiente is { } supuesto ? SecuenciaFisica.Numero(supuesto) : 0,
            SecuenciaFisica.Orden.Count);
    }

    /// <summary>
    /// Inicia un ejercicio guiado. Si ya hay uno en curso lo devuelve tal cual (idempotente): no se
    /// abren dos sesiones en paralelo.
    /// </summary>
    public async Task<EstadoEjercicio> IniciarAsync(CancellationToken ct)
    {
        var estado = await EstadoAsync(ct);
        if (estado.HayEjercicio)
        {
            return estado;
        }

        var ahora = DateTimeOffset.UtcNow;
        var sesion = SesionEjercicio.Iniciar($"ejercicio-{ahora.UtcDateTime:yyyyMMddHHmmss}", ahora);
        await repositorio.AgregarSesionEjercicioAsync(sesion, ct);

        return estado with { Sesion = sesion };
    }

    /// <summary>
    /// Cierra el ejercicio en curso porque el operador salió. Lo ya verificado <b>no se pierde</b>: vive
    /// en las verificaciones, no en la sesión. Idempotente si no hay ninguno abierto.
    /// </summary>
    public async Task AbandonarAsync(CancellationToken ct)
    {
        var sesion = await repositorio.SesionEjercicioEnCursoAsync(ct);
        if (sesion is null)
        {
            return;
        }

        sesion.Abandonar(DateTimeOffset.UtcNow);
        await repositorio.ActualizarSesionEjercicioAsync(sesion, ct);
    }
}
