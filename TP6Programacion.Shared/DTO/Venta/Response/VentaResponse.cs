using TP6Programacion.Shared.Enums;

namespace TP6Programacion.Shared.DTO.Venta.Response;

public record VentaDetalleResponse(
    int ProductoId,
    string ProductoNombre,
    int Cantidad,
    decimal PrecioUnitario,
    decimal Subtotal);

public record VentaResponse(
    int Id,
    DateTime Fecha,
    decimal Total,
    int? ClienteId,
    string? ClienteNombre,
    int? UsuarioId,
    string? UsuarioUsername,
    EstadoVenta Estado,
    List<VentaDetalleResponse> Detalles);

public record VentaListadoResponse(
    int Id,
    DateTime Fecha,
    decimal Total,
    int? ClienteId,
    string? ClienteNombre,
    EstadoVenta Estado);