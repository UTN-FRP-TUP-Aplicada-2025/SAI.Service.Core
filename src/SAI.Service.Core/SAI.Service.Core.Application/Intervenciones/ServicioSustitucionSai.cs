using SAI.Service.Core.Domain.Intervenciones;
using SAI.Service.Core.Domain.Inventario;
using SAI.Service.Core.Domain.Valores;
using SAI.Service.Core.Domain.Verificaciones;
using SAI.Service.Core.Domain.Vinculos;

namespace SAI.Service.Core.Application.Intervenciones;

/// <summary>Datos de una sustitución/reparación del SAI a registrar (CU-09, US-20).</summary>
public sealed record SolicitudSustitucion(
    DateTimeOffset InstanteOcurrido,
    DateTimeOffset InstanteRegistrado,
    string HostCodigo,
    string DispositivoSalienteCodigo,
    TipoIntervencionSai Tipo,
    string? DispositivoEntranteCodigo,
    string? ModeloDispositivoEntrante,
    string? NumeroSerieEntrante,
    DateTimeOffset? InstanteInicioCobertura,
    string Proveedor,
    string Ejecutor,
    string Hallazgos,
    ImporteEntrada? Costo,
    string? DestinoDisposicion,
    string? ReceptorDisposicion);

/// <summary>Código de resultado de una sustitución del SAI.</summary>
public enum CodigoSustitucion
{
    /// <summary>La sustitución se registró (un solo acto): cierre/apertura de cobertura, estados e intervención.</summary>
    Registrado = 1,

    /// <summary>No hay una cobertura vigente del host por el equipo saliente.</summary>
    SinCoberturaVigente = 2,

    /// <summary>La cobertura del suplente solaparía con otra vigente del host (RC-02/I-4).</summary>
    CoberturaSolapada = 3,

    /// <summary>La fecha de la intervención es incoherente con la historia (RN-12, RC-08).</summary>
    CoherenciaTemporal = 4,

    /// <summary>El costo no declara moneda o fecha (RN-07).</summary>
    DineroSinMonedaOFecha = 5,

    /// <summary>Datos inválidos (equipo/modelo inexistente, código en uso, estado incompatible, etc.).</summary>
    DatosInvalidos = 6,
}

/// <summary>Resultado de una sustitución; con los días sin protección del hueco cuando corresponde.</summary>
public sealed record ResultadoSustitucion(
    CodigoSustitucion Codigo,
    string Mensaje,
    string? SustitucionCodigo = null,
    int? DiasSinProteccion = null,
    bool FirmwareReiniciado = false);

/// <summary>
/// Registra la sustitución o reparación del SAI en un solo acto transaccional (CU-09, US-20): valida
/// <b>antes</b> de aplicar (postcondición de fallo: nada se aplica), cierra la cobertura vigente del host
/// por el equipo saliente (RC-03), cambia su estado (en reparación o dado de baja, RN-12), y —si hay
/// suplente— lo pone en servicio y abre una cobertura nueva sin solapar (I-4). Si el suplente es de otro
/// modelo, reinicia las verificaciones de firmware para recaracterizar (FA-2). Un hueco entre coberturas
/// es legítimo y se mide como días sin protección.
/// </summary>
public sealed class ServicioSustitucionSai(IRepositorioSustituciones repositorio)
{
    /// <summary>Registra una sustitución del SAI.</summary>
    public async Task<ResultadoSustitucion> RegistrarAsync(SolicitudSustitucion solicitud, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(solicitud);

        // 1) Costo opcional: si viene un importe, debe declarar moneda y fecha (RN-07).
        Dinero? costo = null;
        if (solicitud.Costo is { } importe)
        {
            costo = AImporte(importe);
            if (costo is null)
            {
                return new ResultadoSustitucion(CodigoSustitucion.DineroSinMonedaOFecha,
                    "DINERO_SIN_MONEDA_O_FECHA: el costo debe declarar su moneda y su fecha (RN-07).");
            }
        }

        // 2) Cobertura vigente del host por el equipo saliente.
        var coberturas = await repositorio.CoberturasDeHostAsync(solicitud.HostCodigo, ct);
        var vigente = coberturas.FirstOrDefault(c => c.Vigencia.Hasta is null);
        if (vigente is null)
        {
            return new ResultadoSustitucion(CodigoSustitucion.SinCoberturaVigente,
                "No hay una cobertura vigente para ese host.");
        }

        if (vigente.DispositivoCodigo != solicitud.DispositivoSalienteCodigo)
        {
            return new ResultadoSustitucion(CodigoSustitucion.DatosInvalidos,
                "El equipo saliente no es el que cubre al host actualmente.");
        }

        var saliente = await repositorio.DispositivoAsync(solicitud.DispositivoSalienteCodigo, ct);
        if (saliente is null)
        {
            return new ResultadoSustitucion(CodigoSustitucion.DatosInvalidos, "No se encontró el equipo saliente.");
        }

        // 3) Coherencia temporal: la intervención no puede ser anterior al inicio de la cobertura ni
        //    posterior a una baja ya registrada (RN-12, RC-08).
        if (solicitud.InstanteOcurrido < vigente.Vigencia.Desde || !saliente.AdmiteOperacionEn(solicitud.InstanteOcurrido))
        {
            return new ResultadoSustitucion(CodigoSustitucion.CoherenciaTemporal,
                "COHERENCIA_TEMPORAL: la fecha es anterior a la cobertura o posterior a una baja (RN-12).");
        }

        // 4) Reemplazo (baja) exige disposición final.
        DisposicionFinal? disposicion = null;
        if (solicitud.Tipo == TipoIntervencionSai.Reemplazo)
        {
            if (string.IsNullOrWhiteSpace(solicitud.DestinoDisposicion) || string.IsNullOrWhiteSpace(solicitud.ReceptorDisposicion))
            {
                return new ResultadoSustitucion(CodigoSustitucion.DatosInvalidos,
                    "Un reemplazo (baja) debe declarar el destino y el receptor de la disposición final.");
            }

            disposicion = new DisposicionFinal(solicitud.DestinoDisposicion, solicitud.ReceptorDisposicion);
        }

        // 5) Suplente (opcional): existente en stock o nuevo a registrar.
        Dispositivo? entrante = null;
        var entranteEsNuevo = false;
        var inicioCobertura = solicitud.InstanteInicioCobertura ?? solicitud.InstanteOcurrido;
        var firmwareReiniciado = false;

        if (!string.IsNullOrWhiteSpace(solicitud.DispositivoEntranteCodigo))
        {
            if (solicitud.DispositivoEntranteCodigo == solicitud.DispositivoSalienteCodigo)
            {
                return new ResultadoSustitucion(CodigoSustitucion.DatosInvalidos,
                    "El suplente no puede ser el mismo equipo saliente.");
            }

            if (inicioCobertura < solicitud.InstanteOcurrido)
            {
                return new ResultadoSustitucion(CodigoSustitucion.CoherenciaTemporal,
                    "COHERENCIA_TEMPORAL: la cobertura del suplente no puede empezar antes de la intervención.");
            }

            entrante = await repositorio.DispositivoAsync(solicitud.DispositivoEntranteCodigo, ct);
            if (entrante is null)
            {
                // Suplente nuevo: modelo debe existir y el código no puede estar en uso.
                if (string.IsNullOrWhiteSpace(solicitud.ModeloDispositivoEntrante))
                {
                    return new ResultadoSustitucion(CodigoSustitucion.DatosInvalidos,
                        "El suplente nuevo debe declarar su modelo de catálogo.");
                }

                if (!await repositorio.ExisteModeloDispositivoAsync(solicitud.ModeloDispositivoEntrante, ct))
                {
                    return new ResultadoSustitucion(CodigoSustitucion.DatosInvalidos,
                        "El modelo del suplente no existe en el catálogo.");
                }

                if (await repositorio.ExisteUnidadAsync(solicitud.DispositivoEntranteCodigo, ct))
                {
                    return new ResultadoSustitucion(CodigoSustitucion.DatosInvalidos,
                        "Ya existe una unidad física con el código del suplente.");
                }

                entrante = new Dispositivo(solicitud.DispositivoEntranteCodigo, solicitud.ModeloDispositivoEntrante, solicitud.NumeroSerieEntrante);
                entranteEsNuevo = true;
            }

            var modeloEntrante = entranteEsNuevo ? solicitud.ModeloDispositivoEntrante! : entrante.ModeloDispositivoCodigo;
            firmwareReiniciado = !string.Equals(modeloEntrante, saliente.ModeloDispositivoCodigo, StringComparison.Ordinal);
        }

        // 6) Aplicar (un solo acto). El cierre valida la coherencia de la vigencia (RC-03).
        try
        {
            vigente.Cerrar(solicitud.InstanteOcurrido);
        }
        catch (ArgumentException)
        {
            return new ResultadoSustitucion(CodigoSustitucion.CoherenciaTemporal,
                "COHERENCIA_TEMPORAL: no se pudo cerrar la cobertura en esa fecha (RN-12).");
        }

        try
        {
            if (solicitud.Tipo == TipoIntervencionSai.Reparacion)
            {
                saliente.EnviarAReparacion();
            }
            else
            {
                saliente.DarDeBaja(solicitud.InstanteOcurrido, $"Sustitución del SAI: {disposicion!.Value.Destino}");
            }
        }
        catch (InvalidOperationException)
        {
            return new ResultadoSustitucion(CodigoSustitucion.DatosInvalidos,
                "El equipo saliente no admite ese cambio de estado en su ciclo de vida (I-6).");
        }

        CoberturaHost? coberturaNueva = null;
        List<Verificacion> reiniciadas = [];
        if (entrante is not null)
        {
            coberturaNueva = new CoberturaHost($"cob-{Guid.NewGuid():N}", entrante.Codigo, solicitud.HostCodigo, new Vigencia(inicioCobertura));

            // La cobertura nueva arranca donde cerró la anterior: la sucesión no debe solapar (I-4).
            if (!Vigencias.AdmiteNuevo(coberturaNueva, coberturas))
            {
                return new ResultadoSustitucion(CodigoSustitucion.CoberturaSolapada,
                    "COBERTURA_SOLAPADA: la cobertura del suplente solaparía con otra del host (I-4/RC-02).");
            }

            try
            {
                if (entrante.Estado != EstadoUnidad.EnServicio)
                {
                    entrante.PonerEnServicio();
                }
            }
            catch (InvalidOperationException)
            {
                return new ResultadoSustitucion(CodigoSustitucion.DatosInvalidos,
                    "El suplente no admite pasar a servicio desde su estado actual (I-6).");
            }

            // FA-2: si el suplente es de otro modelo, las verificaciones de firmware vuelven a «sin verificar».
            if (firmwareReiniciado)
            {
                foreach (var verificacion in await repositorio.ListarVerificacionesAsync(ct))
                {
                    verificacion.Reiniciar(solicitud.InstanteOcurrido);
                    reiniciadas.Add(verificacion);
                }
            }
        }

        var sustitucion = new SustitucionSai(
            $"sus-{Guid.NewGuid():N}", solicitud.HostCodigo, saliente.Codigo, entrante?.Codigo, solicitud.Tipo,
            solicitud.InstanteOcurrido, solicitud.InstanteRegistrado, solicitud.Proveedor, solicitud.Ejecutor,
            solicitud.Hallazgos, firmwareReiniciado, costo, disposicion);

        await repositorio.GuardarSustitucionAsync(vigente, coberturaNueva, saliente, entrante, entranteEsNuevo, sustitucion, reiniciadas, ct);

        // Días sin protección del hueco: solo cuando hay suplente que arranca después del cierre.
        int? diasSinProteccion = entrante is not null && inicioCobertura > solicitud.InstanteOcurrido
            ? (int)Math.Ceiling((inicioCobertura - solicitud.InstanteOcurrido).TotalDays)
            : null;

        return new ResultadoSustitucion(CodigoSustitucion.Registrado,
            entrante is null
                ? "Sustitución registrada: cobertura cerrada; el host queda sin protección hasta un suplente."
                : "Sustitución registrada: cobertura cerrada y reabierta con el suplente.",
            sustitucion.Codigo, diasSinProteccion, firmwareReiniciado);
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
