using SAI.Service.Core.Domain.Historia;

namespace SAI.Service.Core.Domain.Monitoreo;

/// <summary>
/// Agregado de una variable en una ventana de tiempo (BT-18). Es historia append-only y un tipo
/// <b>distinto</b> de <see cref="Muestra"/> —no hereda de ella ni comparten flag— para que el
/// compilador impida confundirlos (ADR-08, RC-04): servir un agregado como muestra borraría los
/// microcortes que el promedio horario oculta. Por eso conserva <b>mínimo y máximo</b> además del
/// promedio, y viaja siempre con su <see cref="Cobertura"/> y una posible <see cref="Advertencia"/>
/// (I-20, RN-10). El conteo de microcortes sale de los eventos, nunca de esta serie agregada.
/// </summary>
public sealed class Agregado : IEntidadHistoria
{
    /// <summary>Código de negocio del agregado (identidad estable).</summary>
    public string Codigo { get; private set; }

    /// <summary>Código del dispositivo (SAI).</summary>
    public string DispositivoCodigo { get; private set; }

    /// <summary>Variable agregada (p. ej. "input.voltage").</summary>
    public string Variable { get; private set; }

    /// <summary>Inicio de la ventana de agregación.</summary>
    public DateTimeOffset VentanaInicio { get; private set; }

    /// <summary>Duración de la ventana en ISO-8601 (p. ej. "PT1H").</summary>
    public string VentanaDuracion { get; private set; }

    /// <summary>Cantidad de muestras con valor que contribuyeron al agregado.</summary>
    public int NMuestras { get; private set; }

    /// <summary>Cobertura de la ventana en 0..1 (fracción de muestras con dato).</summary>
    public double Cobertura { get; private set; }

    /// <summary>Advertencia sobre la calidad del agregado (p. ej. cobertura baja), o nula.</summary>
    public string? Advertencia { get; private set; }

    /// <summary>Promedio de los valores, o nulo si no hubo datos.</summary>
    public double? Promedio { get; private set; }

    /// <summary>Mínimo de los valores, o nulo si no hubo datos.</summary>
    public double? Minimo { get; private set; }

    /// <summary>Máximo de los valores, o nulo si no hubo datos.</summary>
    public double? Maximo { get; private set; }

    // Constructor de materialización (EF Core).
    private Agregado()
    {
        Codigo = null!;
        DispositivoCodigo = null!;
        Variable = null!;
        VentanaDuracion = null!;
    }

    /// <summary>Construye un agregado ya calculado (lo produce <see cref="CalculadorAgregado"/>).</summary>
    public Agregado(
        string codigo,
        string dispositivoCodigo,
        string variable,
        DateTimeOffset ventanaInicio,
        string ventanaDuracion,
        int nMuestras,
        double cobertura,
        double? promedio,
        double? minimo,
        double? maximo,
        string? advertencia)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(dispositivoCodigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(variable);
        ArgumentException.ThrowIfNullOrWhiteSpace(ventanaDuracion);
        if (cobertura is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(cobertura), "La cobertura debe estar en 0..1.");
        }

        Codigo = codigo;
        DispositivoCodigo = dispositivoCodigo;
        Variable = variable;
        VentanaInicio = ventanaInicio;
        VentanaDuracion = ventanaDuracion;
        NMuestras = nMuestras;
        Cobertura = cobertura;
        Promedio = promedio;
        Minimo = minimo;
        Maximo = maximo;
        Advertencia = advertencia;
    }
}
