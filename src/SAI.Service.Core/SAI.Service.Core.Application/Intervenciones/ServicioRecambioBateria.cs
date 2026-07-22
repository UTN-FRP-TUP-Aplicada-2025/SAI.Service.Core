using SAI.Service.Core.Domain.Intervenciones;
using SAI.Service.Core.Domain.Inventario;
using SAI.Service.Core.Domain.Valores;
using SAI.Service.Core.Domain.Vinculos;

namespace SAI.Service.Core.Application.Intervenciones;

/// <summary>Un importe tal como llega del formulario: puede faltar la moneda o la fecha (RN-07).</summary>
public sealed record ImporteEntrada(decimal Monto, string? Moneda, DateOnly? Fecha);

/// <summary>Datos de un recambio de batería a registrar (CU-08, US-18).</summary>
public sealed record SolicitudRecambio(
    DateTimeOffset InstanteOcurrido,
    DateTimeOffset InstanteRegistrado,
    string DispositivoCodigo,
    string Posicion,
    string BateriaEntranteCodigo,
    string ModeloBateriaEntranteCodigo,
    DateOnly? FechaFabricacionEntrante,
    DateOnly? FechaCompraEntrante,
    string Proveedor,
    string Ejecutor,
    ImporteEntrada Repuestos,
    ImporteEntrada ManoDeObra,
    ImporteEntrada Total,
    string Hallazgos,
    string DestinoDisposicion,
    string ReceptorDisposicion,
    decimal TasaAUsd,
    string FuenteCotizacion);

/// <summary>Código de resultado de un recambio.</summary>
public enum CodigoRecambio
{
    /// <summary>El recambio se registró (un solo acto): cierre/apertura, baja/servicio, intervención y ficha.</summary>
    Registrado = 1,

    /// <summary>Algún importe no declara moneda o fecha (RN-07).</summary>
    DineroSinMonedaOFecha = 2,

    /// <summary>El total no cuadra con repuestos + mano de obra (RN-08).</summary>
    CostosNoCuadran = 3,

    /// <summary>La fecha del recambio es incoherente con la historia (RN-12, RC-08).</summary>
    CoherenciaTemporal = 4,

    /// <summary>No hay montaje vigente para el dispositivo/posición.</summary>
    SinMontajeVigente = 5,

    /// <summary>Datos inválidos (modelo inexistente, código de batería ya usado, etc.).</summary>
    DatosInvalidos = 6,
}

/// <summary>Resultado de un recambio; con la ficha proyectada cuando se registró.</summary>
public sealed record ResultadoRecambio(CodigoRecambio Codigo, string Mensaje, string? IntervencionCodigo = null, FichaVidaUtil? Ficha = null);

/// <summary>
/// Registra el recambio de batería en un solo acto transaccional (CU-08, US-18): valida el cuadre de
/// costos (RN-08) y que todo importe lleve moneda y fecha (RN-07) <b>antes</b> de aplicar efectos
/// (postcondición de fallo: nada se aplica), cierra el montaje vigente y abre el de la batería nueva
/// sin hueco (RC-03), da de baja la batería retirada (I-5) y pone en servicio la nueva, y proyecta la
/// ficha de vida útil con el costo por año normalizado a USD (US-19).
/// </summary>
public sealed class ServicioRecambioBateria(IRepositorioIntervenciones repositorio)
{
    /// <summary>Registra un recambio de batería.</summary>
    public async Task<ResultadoRecambio> RegistrarAsync(SolicitudRecambio solicitud, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(solicitud);

        // 1) Todo importe con moneda y fecha (RN-07), antes de tocar nada.
        if (AImporte(solicitud.Repuestos) is not { } repuestos
            || AImporte(solicitud.ManoDeObra) is not { } manoDeObra
            || AImporte(solicitud.Total) is not { } total)
        {
            return new ResultadoRecambio(CodigoRecambio.DineroSinMonedaOFecha, "DINERO_SIN_MONEDA_O_FECHA: todo importe debe declarar su moneda y su fecha (RN-07).");
        }

        // 2) Cuadre de costos (RN-08).
        var costos = new Costos(repuestos, manoDeObra, total);
        if (!costos.Cuadra())
        {
            return new ResultadoRecambio(CodigoRecambio.CostosNoCuadran, "COSTOS_NO_CUADRAN: el total debe igualar repuestos + mano de obra, en la misma moneda (RN-08).");
        }

        // 3) Montaje vigente.
        var montaje = await repositorio.MontajeVigenteAsync(solicitud.DispositivoCodigo, solicitud.Posicion, ct);
        if (montaje is null)
        {
            return new ResultadoRecambio(CodigoRecambio.SinMontajeVigente, "No hay un montaje de batería vigente para ese dispositivo y posición.");
        }

        var bateriaVieja = await repositorio.BateriaAsync(montaje.BateriaCodigo, ct);
        if (bateriaVieja is null)
        {
            return new ResultadoRecambio(CodigoRecambio.DatosInvalidos, "No se encontró la batería del montaje vigente.");
        }

        // 4) Coherencia temporal: el recambio no puede ser anterior al inicio del montaje ni posterior
        //    a una baja ya registrada (RN-12, RC-08).
        if (solicitud.InstanteOcurrido < montaje.Vigencia.Desde || !bateriaVieja.AdmiteOperacionEn(solicitud.InstanteOcurrido))
        {
            return new ResultadoRecambio(CodigoRecambio.CoherenciaTemporal, "COHERENCIA_TEMPORAL: la fecha del recambio es anterior al montaje o posterior a una baja (RN-12).");
        }

        // 5) Datos de la batería entrante: el modelo debe existir y el código no puede estar en uso.
        if (await repositorio.ModeloBateriaAsync(solicitud.ModeloBateriaEntranteCodigo, ct) is null)
        {
            return new ResultadoRecambio(CodigoRecambio.DatosInvalidos, "El modelo de la batería entrante no existe en el catálogo.");
        }

        if (await repositorio.ExisteUnidadAsync(solicitud.BateriaEntranteCodigo, ct))
        {
            return new ResultadoRecambio(CodigoRecambio.DatosInvalidos, "Ya existe una unidad física con el código de la batería entrante.");
        }

        var modeloViejo = await repositorio.ModeloBateriaAsync(bateriaVieja.ModeloBateriaCodigo, ct);
        var vidaEsperadaDias = (int)Math.Round((modeloViejo?.VidaFlotacionAniosMin ?? 0) * 365.25);

        // 6) Aplicar el recambio (un solo acto). El cierre valida la coherencia de la vigencia.
        var montajes = await repositorio.MontajesDeDispositivoAsync(solicitud.DispositivoCodigo, solicitud.Posicion, ct);
        var bateriaNueva = new Bateria(solicitud.BateriaEntranteCodigo, solicitud.ModeloBateriaEntranteCodigo, solicitud.FechaFabricacionEntrante, solicitud.FechaCompraEntrante);
        var montajeNuevo = new MontajeBateria($"mon-{Guid.NewGuid():N}", bateriaNueva.Codigo, solicitud.DispositivoCodigo, solicitud.Posicion, new Vigencia(solicitud.InstanteOcurrido));

        try
        {
            montaje.Cerrar(solicitud.InstanteOcurrido);
        }
        catch (ArgumentException)
        {
            return new ResultadoRecambio(CodigoRecambio.CoherenciaTemporal, "COHERENCIA_TEMPORAL: no se pudo cerrar la vigencia en esa fecha (RN-12).");
        }

        // El nuevo montaje arranca donde cerró el anterior: la sucesión no debe solapar (I-1/I-2).
        if (!Vigencias.AdmiteNuevo(montajeNuevo, montajes))
        {
            return new ResultadoRecambio(CodigoRecambio.CoherenciaTemporal, "COHERENCIA_TEMPORAL: el montaje nuevo solapa con la historia de la posición (I-1/I-2).");
        }

        bateriaNueva.PonerEnServicio();
        bateriaVieja.DarDeBaja(solicitud.InstanteOcurrido, $"Recambio de batería: {solicitud.DestinoDisposicion}");

        var intervencion = new Intervencion(
            $"int-{Guid.NewGuid():N}", solicitud.DispositivoCodigo, solicitud.Posicion,
            bateriaVieja.Codigo, bateriaNueva.Codigo, solicitud.InstanteOcurrido, solicitud.InstanteRegistrado,
            solicitud.Proveedor, solicitud.Ejecutor, costos, solicitud.Hallazgos,
            new DisposicionFinal(solicitud.DestinoDisposicion, solicitud.ReceptorDisposicion));

        var ficha = FichaVidaUtil.Proyectar(
            $"fic-{Guid.NewGuid():N}", intervencion.Codigo, solicitud.DispositivoCodigo, bateriaVieja.Codigo,
            montaje.Vigencia.Desde, solicitud.InstanteOcurrido, vidaEsperadaDias, total, solicitud.TasaAUsd, solicitud.FuenteCotizacion);

        await repositorio.GuardarRecambioAsync(montaje, bateriaVieja, bateriaNueva, montajeNuevo, intervencion, ficha, ct);
        return new ResultadoRecambio(CodigoRecambio.Registrado, "Recambio registrado: montaje cerrado y reabierto, batería retirada dada de baja y ficha proyectada.", intervencion.Codigo, ficha);
    }

    // Convierte un importe de entrada en Dinero, o null si le falta la moneda o la fecha (RN-07).
    private static Dinero? AImporte(ImporteEntrada entrada)
    {
        if (entrada is null || string.IsNullOrWhiteSpace(entrada.Moneda) || entrada.Fecha is not { } fecha)
        {
            return null;
        }

        return new Dinero(entrada.Monto, entrada.Moneda, fecha);
    }
}
