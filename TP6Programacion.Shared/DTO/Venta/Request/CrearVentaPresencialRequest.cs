using TP6Programacion.Shared.Enums;

namespace TP6Programacion.Shared.DTO.Venta.Request;

public record CrearVentaPresencialRequest(
    int? ClienteId,
    List<CrearVentaItemRequest> Items,
    MetodoPago Metodo,
    string? ReferenciaPago
);