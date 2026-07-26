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

        return propuesta.ModalidadSolicitada switch
        {
            Modalidad.SoloAlerta =>
                "El sistema solo avisará ante un corte de energía; no apagará el host.",
            Modalidad.ApagarHostConRetorno =>
                $"Cuando el corte supere {umbral} s en batería, el sistema apagará el host de forma ordenada "
                + $"(reservándole {reservado} s) y lo repondrá al volver la energía.",
            Modalidad.ApagarHostLuegoUpsConRetorno =>
                $"Cuando el corte supere {umbral} s en batería, el sistema apagará el host "
                + $"(reservándole {reservado} s) y luego cortará la salida del SAI, reponiendo ambos al volver la energía.",
            Modalidad.CicloForzado =>
                $"Cuando el corte supere {umbral} s en batería, el sistema apagará el host "
                + $"(reservándole {reservado} s) y cortará la salida del SAI; el corte no se cancela aunque vuelva la red.",
            _ => "Política sin descripción.",
        };
    }
}
