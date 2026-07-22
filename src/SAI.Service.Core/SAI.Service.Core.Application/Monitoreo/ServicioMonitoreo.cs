using SAI.Service.Core.Application.Abstractions;
using SAI.Service.Core.Domain.Inventario;
using SAI.Service.Core.Domain.Monitoreo;
using SAI.Service.Core.Domain.Vinculos;

namespace SAI.Service.Core.Application.Monitoreo;

/// <summary>Resultado de una ronda de sondeo.</summary>
public enum ResultadoSondeo
{
    /// <summary>No hay dispositivo en servicio para sondear.</summary>
    SinDispositivo = 1,

    /// <summary>Se registró una muestra con datos (completa o parcial).</summary>
    Registrada = 2,

    /// <summary>El equipo no respondió: se registró una muestra perdida.</summary>
    Perdida = 3,
}

/// <summary>
/// Orquesta una ronda de sondeo (US-08, CU-04): resuelve el dispositivo en servicio y su sesión
/// activa (la abre si falta), lee el estado por el adaptador y persiste una <see cref="Muestra"/>
/// con su calidad. La procedencia por variable la aporta el mapa de la sesión (US-10). No emite
/// eventos ni evalúa reglas: eso es del Incremento B (US-09).
/// </summary>
public sealed class ServicioMonitoreo(IRepositorioMonitoreo repositorio, IAdaptadorConexion adaptador)
{
    /// <summary>Código de la fuente de datos NUT (herramienta de acceso).</summary>
    public const string CodigoFuenteNut = "fuente-nut";

    private static readonly string[] VariablesEsperadas = [.. Variables.ProcedenciaCanonica.Keys];

    /// <summary>Ejecuta una ronda de sondeo con el intervalo (segundos) de la sesión.</summary>
    public async Task<ResultadoSondeo> SondearAsync(int intervaloSeg, CancellationToken ct)
    {
        var dispositivo = await repositorio.DispositivoEnServicioAsync(ct);
        if (dispositivo is null)
        {
            return ResultadoSondeo.SinDispositivo;
        }

        var sesion = await repositorio.SesionActivaDeAsync(dispositivo.Codigo, ct)
                     ?? await AbrirSesionAsync(dispositivo, intervaloSeg, ct);

        var estado = await adaptador.LeerEstadoAsync(ct);
        var lecturas = new Dictionary<string, double?>(StringComparer.Ordinal)
        {
            [Variables.TensionEntrada] = estado.TensionEntradaVoltios,
            [Variables.TensionSalida] = estado.TensionSalidaVoltios,
            [Variables.CargaSalida] = estado.CargaSalidaPorcentaje,
            [Variables.CargaBateria] = estado.CargaBateriaPorcentaje,
            [Variables.EstadoUps] = estado.EstadoUps switch
            {
                Abstractions.EstadoUps.EnBateria => Variables.CodigoEnBateria,
                Abstractions.EstadoUps.EnLinea => Variables.CodigoEnLinea,
                _ => null,
            },
            [Variables.TensionBateria] = estado.TensionBateriaVoltios,
        };

        var muestra = Muestra.Registrar(
            $"mue-{Guid.NewGuid():N}",
            dispositivo.Codigo,
            sesion.Codigo,
            estado.MarcaTiempoUtc,
            estado.Alcanzable,
            lecturas,
            VariablesEsperadas);

        await repositorio.GuardarMuestraAsync(muestra, ct);

        // Derivación de eventos (US-09, BT-19) sobre la ventana reciente, con las reglas vigentes.
        var recientes = await repositorio.MuestrasRecientesAsync(dispositivo.Codigo, VentanaMuestras, ct);
        var reglas = await repositorio.ReglasVigentesAsync(muestra.Instante, ct);
        if (reglas.Count > 0)
        {
            var eventos = DerivadorEventos.Derivar(dispositivo.Codigo, recientes, reglas, () => $"evt-{Guid.NewGuid():N}");
            if (eventos.Count > 0)
            {
                await repositorio.GuardarEventosAsync(eventos, ct);
            }
        }

        return muestra.Calidad == CalidadMuestra.Perdida ? ResultadoSondeo.Perdida : ResultadoSondeo.Registrada;
    }

    // Ventana de muestras que evalúan las reglas (cubre el sostenido de tensión y el disparo BT-20).
    private const int VentanaMuestras = 200;

    private async Task<SesionSondeo> AbrirSesionAsync(Dispositivo dispositivo, int intervaloSeg, CancellationToken ct)
    {
        var existeFuente = await repositorio.ExisteFuenteAsync(CodigoFuenteNut, ct);
        var nuevaFuente = existeFuente
            ? null
            : new FuenteDatos(CodigoFuenteNut, ConfianzaFuente.Media, "Herramienta de acceso (NUT)");

        var sesion = new SesionSondeo(
            $"ses-{Guid.NewGuid():N}",
            dispositivo.Codigo,
            CodigoFuenteNut,
            driver: "nut",
            intervaloSeg: intervaloSeg,
            vigencia: new Vigencia(DateTimeOffset.UtcNow),
            mapaVariableOrigen: Variables.ProcedenciaCanonica);

        await repositorio.GuardarSesionAsync(nuevaFuente, sesion, ct);
        return sesion;
    }
}
