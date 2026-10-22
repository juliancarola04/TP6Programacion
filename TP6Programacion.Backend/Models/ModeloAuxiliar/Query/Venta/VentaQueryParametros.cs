using TP6Programacion.Shared.Enums;

namespace TP6Programacion.Backend.Models.ModeloAuxiliar.Query.Venta;

public record VentaQueryParametros : QueryParametros
{
    public int? ClienteId { get; init; }
    public EstadoVenta? Estado { get; init; }
    public string? Buscar { get; init; } // Por nombre del cliente
    public string? OrdenarPor { get; init; } = "fecha"; // "fecha", "total"
    public string? Direccion { get; init; } // "asc", "desc"
}
