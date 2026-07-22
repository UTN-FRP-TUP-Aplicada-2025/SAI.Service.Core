using SAI.Service.Core.Domain.Historia;
using SAI.Service.Core.Domain.Valores;

namespace SAI.Service.Core.Domain.Intervenciones;

/// <summary>
/// Ficha de vida útil proyectada al recambiar una batería (CU-08 §4.7, US-19, BT-27). Es historia
/// append-only (ADR-04): registra cuánto duró de verdad la batería (días en servicio), si
/// <see cref="CumplioExpectativa"/> la vida de flotación esperada y su <see cref="DesvioDias"/>, y el
/// <b>costo por año de servicio</b> en la moneda original y su equivalente normalizado a USD, que
/// viaja marcado como <b>derivado</b> con su <see cref="FuenteCotizacion"/> y nunca reemplaza al
/// valor original (RN-07, ADR-06).
/// </summary>
public sealed class FichaVidaUtil : IEntidadHistoria
{
    private const double DiasPorAnio = 365.25;

    /// <summary>Código de negocio de la ficha.</summary>
    public string Codigo { get; private set; }

    /// <summary>Intervención que la originó.</summary>
    public string IntervencionCodigo { get; private set; }

    /// <summary>Dispositivo (SAI) al que pertenecía la batería.</summary>
    public string DispositivoCodigo { get; private set; }

    /// <summary>Batería retirada cuya vida describe la ficha.</summary>
    public string BateriaCodigo { get; private set; }

    /// <summary>Días que la batería estuvo en servicio (vigencia del montaje cerrado).</summary>
    public int DiasEnServicio { get; private set; }

    /// <summary>Vida de flotación esperada, en días (del modelo de batería).</summary>
    public int VidaEsperadaDias { get; private set; }

    /// <summary>Verdadero si la vida real alcanzó la esperada.</summary>
    public bool CumplioExpectativa { get; private set; }

    /// <summary>Desvío de la vida real contra la esperada, en días (positivo: duró más).</summary>
    public int DesvioDias { get; private set; }

    /// <summary>Costo por año de servicio en la moneda original (valor medido).</summary>
    public Dinero CostoPorAnioServicio { get; private set; }

    /// <summary>Costo por año de servicio normalizado a USD (valor derivado, ADR-06).</summary>
    public Dinero CostoPorAnioServicioUsd { get; private set; }

    /// <summary>Fuente de la cotización usada para normalizar a USD (obligatoria en el equivalente derivado).</summary>
    public string FuenteCotizacion { get; private set; }

    // Constructor de materialización (EF Core).
    private FichaVidaUtil()
    {
        Codigo = null!;
        IntervencionCodigo = null!;
        DispositivoCodigo = null!;
        BateriaCodigo = null!;
        FuenteCotizacion = null!;
    }

    private FichaVidaUtil(
        string codigo, string intervencionCodigo, string dispositivoCodigo, string bateriaCodigo,
        int diasEnServicio, int vidaEsperadaDias, Dinero costoPorAnio, Dinero costoPorAnioUsd, string fuenteCotizacion)
    {
        Codigo = codigo;
        IntervencionCodigo = intervencionCodigo;
        DispositivoCodigo = dispositivoCodigo;
        BateriaCodigo = bateriaCodigo;
        DiasEnServicio = diasEnServicio;
        VidaEsperadaDias = vidaEsperadaDias;
        CumplioExpectativa = diasEnServicio >= vidaEsperadaDias;
        DesvioDias = diasEnServicio - vidaEsperadaDias;
        CostoPorAnioServicio = costoPorAnio;
        CostoPorAnioServicioUsd = costoPorAnioUsd;
        FuenteCotizacion = fuenteCotizacion;
    }

    /// <summary>
    /// Proyecta la ficha a partir de la vigencia del montaje cerrado, la vida esperada del modelo, el
    /// costo total de la intervención y la cotización a USD (tasa: cuántos USD vale una unidad de la
    /// moneda original). El costo por año se anualiza sobre los días en servicio (mínimo un día para
    /// no dividir por cero); el equivalente en USD es derivado con su fuente.
    /// </summary>
    public static FichaVidaUtil Proyectar(
        string codigo, string intervencionCodigo, string dispositivoCodigo, string bateriaCodigo,
        DateTimeOffset desde, DateTimeOffset hasta, int vidaEsperadaDias, Dinero costoTotal,
        decimal tasaAUsd, string fuenteCotizacion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(intervencionCodigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(bateriaCodigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(fuenteCotizacion);
        if (tasaAUsd <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tasaAUsd), tasaAUsd, "La cotización a USD debe ser positiva.");
        }

        var dias = (int)Math.Round((hasta - desde).TotalDays);
        var anios = (decimal)(Math.Max(dias, 1) / DiasPorAnio);
        var montoPorAnio = decimal.Round(costoTotal.Monto / anios, 2);
        var costoPorAnio = new Dinero(montoPorAnio, costoTotal.Moneda, costoTotal.Fecha);
        var costoPorAnioUsd = new Dinero(decimal.Round(montoPorAnio * tasaAUsd, 2), "USD", costoTotal.Fecha);

        return new FichaVidaUtil(
            codigo, intervencionCodigo, dispositivoCodigo, bateriaCodigo,
            dias, vidaEsperadaDias, costoPorAnio, costoPorAnioUsd, fuenteCotizacion);
    }
}
