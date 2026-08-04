using System.Globalization;
using SAI.Service.Core.Domain.Verificaciones;

namespace SAI.Service.Core.Application.Politicas;

/// <summary>
/// Redacta "en palabras" lo que hará una propuesta de política (CU-03, wireframe §"En palabras"): prosa
/// generada a partir de la modalidad y los valores, que se regenera al cambiar un parámetro. No usa
/// culturas nombradas (la solución corre en modo globalization-invariant).
/// </summary>
public static class ExplicacionPolitica
{
    /// <summary>Describe en prosa qué hará la política propuesta.</summary>
    public static string Redactar(PropuestaPolitica propuesta)
    {
        ArgumentNullException.ThrowIfNull(propuesta);
        var umbral = propuesta.UmbralDisparoSegundos.ToString(CultureInfo.InvariantCulture);
        var reservado = propuesta.TiempoReservadoApagadoSeg.ToString(CultureInfo.InvariantCulture);
        var retorno = propuesta.TiempoRetornoSeg.ToString(CultureInfo.InvariantCulture);

        if (propuesta.ModalidadSolicitada == Modalidad.SoloAlerta)
        {
            return "El sistema solo avisará ante un corte de energía; no apagará el host.";
        }

        // Disparo + ventana del host, común a las modalidades de acción.
        var disparo =
            $"Superado el tiempo en batería de {umbral} s, el servicio envía al sistema operativo la orden de "
            + $"apagado ordenado. El host tendrá {reservado} s para completar el apagado antes de que el SAI "
            + "corte la energía a su salida.";

        // Reposición tras el retorno de la red (ups.delay.start) + dependencia de la BIOS.
        var reposicion =
            $" Cuando vuelva la energía de red, el SAI esperará {retorno} s antes de restaurar la alimentación "
            + "al host, forzando una transición ausencia→presencia limpia; el host reenciende por sí solo "
            + "siempre que tenga el autoencendido (power-on tras corte de energía) activado en la BIOS.";

        return propuesta.ModalidadSolicitada switch
        {
            Modalidad.ApagarHostConRetorno => disparo + reposicion,
            Modalidad.ApagarHostLuegoUpsConRetorno =>
                disparo + " Luego el SAI corta su propia salida." + reposicion,
            Modalidad.CicloForzado =>
                disparo + " El corte no se cancela aunque vuelva la red antes de completarse (ciclo forzado)." + reposicion,
            _ => "Política sin descripción.",
        };
    }
}
