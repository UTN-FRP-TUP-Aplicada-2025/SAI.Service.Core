namespace SAI.Service.Core.Domain.Intervenciones;

/// <summary>
/// Disposición final de la batería retirada (CU-08 §4.6, trazabilidad ambiental): a dónde fue y
/// quién la recibió. Ambos datos son obligatorios: una batería retirada siempre declara su destino.
/// </summary>
public readonly record struct DisposicionFinal
{
    /// <summary>Destino de la batería retirada (p. ej. "reciclado gestor habilitado").</summary>
    public string Destino { get; }

    /// <summary>Receptor de la batería retirada (persona u organización).</summary>
    public string Receptor { get; }

    /// <summary>Construye la disposición final exigiendo destino y receptor.</summary>
    public DisposicionFinal(string destino, string receptor)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destino);
        ArgumentException.ThrowIfNullOrWhiteSpace(receptor);
        Destino = destino;
        Receptor = receptor;
    }
}
