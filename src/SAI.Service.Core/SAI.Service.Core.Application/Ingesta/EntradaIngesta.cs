namespace SAI.Service.Core.Application.Ingesta;

/// <summary>Un importe tal como llega en el cuerpo: moneda y fecha son <b>nullable</b> para poder detectar
/// el "dinero incompleto" (RN-07) antes de construir el value object <c>Dinero</c>.</summary>
public sealed record DineroEntrada(decimal Monto, string? Moneda, DateOnly? Fecha);

/// <summary>Costos del cuerpo: repuestos como arreglo (contrato), mano de obra y total.</summary>
public sealed record CostosEntrada(IReadOnlyList<DineroEntrada>? Repuestos, DineroEntrada? ManoDeObra, DineroEntrada? Total);

/// <summary>Disposición final del cuerpo (destino y receptor de una batería retirada).</summary>
public sealed record DisposicionEntrada(string? Destino, string? Receptor);

/// <summary>
/// Cuerpo de una intervención ingresada por la API (CU-11, DTO <c>IntervencionEntrada</c> del contrato).
/// Todos los campos son nullable: la validación de obligatorios y de invariantes la hace
/// <see cref="ServicioIngesta"/>, para devolver un 422 tipado en vez de fallar al deserializar.
/// </summary>
public sealed record EntradaIngesta(
    string? TipoIntervencionId,
    string? DispositivoId,
    IReadOnlyList<string>? BateriaIds,
    string? ProveedorId,
    DateTimeOffset? TiempoValido,
    CostosEntrada? Costos,
    string? Hallazgos,
    DisposicionEntrada? DisposicionFinal);
