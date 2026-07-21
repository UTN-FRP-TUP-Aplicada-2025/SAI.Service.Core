using SAI.Service.Core.Domain.Catalogo;
using SAI.Service.Core.Domain.Inventario;
using SAI.Service.Core.Domain.Verificaciones;
using SAI.Service.Core.Domain.Vinculos;

namespace SAI.Service.Core.Application.Equipos;

/// <summary>
/// Caso de uso de alta y puesta en marcha de equipos (CU-02, US-04, US-05). Orquesta el dominio:
/// crea catálogo e inventario, abre los vínculos temporales con fin abierto (validando el no
/// solapamiento), siembra las cuatro verificaciones en <see cref="EstadoVerificacion.NuncaVerificado"/>
/// y deja la modalidad efectiva forzada a <see cref="Modalidad.SoloAlerta"/>. El guardado es
/// transaccional: si algo falla, no quedan entidades a medias.
/// </summary>
public sealed class ServicioAltaEquipos(IRepositorioEquipos repositorio)
{
    /// <summary>Registra un alta a partir de los datos declarados por el administrador.</summary>
    public async Task<ResultadoAltaEquipos> RegistrarAsync(SolicitudAltaEquipos solicitud, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(solicitud);

        // Catálogo. El modelo de batería valida el invariante I-21 (vida de flotación exige
        // temperatura de referencia) en su constructor; se traduce al código de dominio.
        ModeloBateria modeloBateria;
        try
        {
            modeloBateria = new ModeloBateria(
                solicitud.ModeloBateria.Codigo,
                solicitud.Fabricante.Codigo,
                solicitud.ModeloBateria.Nombre,
                solicitud.ModeloBateria.Tecnologia,
                solicitud.ModeloBateria.CapacidadAh,
                solicitud.ModeloBateria.TensionNominalV,
                solicitud.ModeloBateria.VidaFlotacionAniosMin,
                solicitud.ModeloBateria.VidaFlotacionAniosMax,
                solicitud.ModeloBateria.TemperaturaReferenciaC);
        }
        catch (ArgumentException)
        {
            return ResultadoAltaEquipos.Error("VIDA_FLOTACION_SIN_TEMPERATURA");
        }

        var fabricante = new Fabricante(solicitud.Fabricante.Codigo, solicitud.Fabricante.Nombre, solicitud.Fabricante.Identificado);
        var modeloDispositivo = new ModeloDispositivo(
            solicitud.ModeloDispositivo.Codigo,
            solicitud.Fabricante.Codigo,
            solicitud.ModeloDispositivo.Nombre,
            solicitud.ModeloDispositivo.LineaTopologia,
            solicitud.ModeloDispositivo.TensionNominalV,
            solicitud.ModeloDispositivo.PotenciaVaNominal);

        // Inventario, puesto en servicio.
        var host = new Host(solicitud.Host.Codigo, solicitud.Host.Criticidad, solicitud.Instante);
        host.PonerEnServicio();
        var dispositivo = new Dispositivo(solicitud.Dispositivo.Codigo, solicitud.ModeloDispositivo.Codigo, solicitud.Dispositivo.NumeroSerie);
        dispositivo.PonerEnServicio();
        var bateria = new Bateria(solicitud.Bateria.Codigo, solicitud.ModeloBateria.Codigo, solicitud.Bateria.FechaFabricacion, solicitud.Bateria.FechaCompra);
        bateria.PonerEnServicio();

        // Vínculos temporales abiertos (fin abierto = vigente).
        var vigencia = new Vigencia(solicitud.Instante);
        var montaje = new MontajeBateria($"mnt-{dispositivo.Codigo}-{solicitud.Posicion}", bateria.Codigo, dispositivo.Codigo, solicitud.Posicion, vigencia);
        var cobertura = new CoberturaHost($"cob-{host.Codigo}", dispositivo.Codigo, host.Codigo, vigencia);

        // No solapamiento contra lo ya existente (I-1/I-2/I-4).
        var montajes = await repositorio.MontajesDeDispositivoAsync(dispositivo.Codigo, solicitud.Posicion, ct);
        if (!Vigencias.AdmiteNuevo(montaje, montajes))
        {
            return ResultadoAltaEquipos.Error("MONTAJE_SOLAPADO");
        }

        var coberturas = await repositorio.CoberturasDeHostAsync(host.Codigo, ct);
        if (!Vigencias.AdmiteNuevo(cobertura, coberturas))
        {
            return ResultadoAltaEquipos.Error("COBERTURA_SOLAPADA");
        }

        // Siembra de las cuatro verificaciones en NuncaVerificado (US-05).
        var verificaciones = Enum.GetValues<Supuesto>()
            .Select(supuesto => Verificacion.Sembrar($"ver-{dispositivo.Codigo}-{supuesto}", supuesto, solicitud.Instante))
            .ToList();

        var conjunto = new ConjuntoAlta(fabricante, modeloDispositivo, modeloBateria, host, dispositivo, bateria, montaje, cobertura, verificaciones);
        await repositorio.GuardarAltaAsync(conjunto, ct);

        // Modalidad efectiva forzada a SoloAlerta: recién sembrado, 0 de 4 verificados (RN-02).
        var modalidad = EvaluadorModalidad.Efectiva(Modalidad.SoloAlerta, verificaciones);
        return ResultadoAltaEquipos.Ok(modalidad, EvaluadorModalidad.Verificados(verificaciones), EvaluadorModalidad.SupuestosRequeridos);
    }

    /// <summary>
    /// Estado de puesta en marcha para el aviso del panel (US-05): modalidad efectiva y «n de 4».
    /// Sin política configurada todavía (llega en un incremento posterior), la modalidad solicitada
    /// es <see cref="Modalidad.SoloAlerta"/>, por lo que la efectiva también lo es hasta verificar.
    /// </summary>
    public async Task<EstadoPuestaEnMarcha> ConsultarEstadoAsync(CancellationToken ct)
    {
        var verificaciones = await repositorio.ListarVerificacionesAsync(ct);
        var hayEquipos = await repositorio.HayEquiposAsync(ct);
        var modalidad = EvaluadorModalidad.Efectiva(Modalidad.SoloAlerta, verificaciones);
        return new EstadoPuestaEnMarcha(
            modalidad,
            EvaluadorModalidad.Verificados(verificaciones),
            EvaluadorModalidad.SupuestosRequeridos,
            hayEquipos);
    }
}
