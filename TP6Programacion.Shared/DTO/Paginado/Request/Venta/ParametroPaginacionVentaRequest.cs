using TP6Programacion.Shared.Enums;

namespace TP6Programacion.Shared.DTO.Paginado.Request.Venta;

public record ParametroPaginacionVentaRequest : ParametroPaginacionRequest
{
    public int? ClienteId { get; init; }
    public EstadoVenta? Estado { get; init; }
    public string? Buscar { get; init; } // Por nombre del cliente
    public string? OrdenarPor { get; init; } // "fecha", "total"
    public string? Direccion { get; init; } // "asc", "desc"
}
