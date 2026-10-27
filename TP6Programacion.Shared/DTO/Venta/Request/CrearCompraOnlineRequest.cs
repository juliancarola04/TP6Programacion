namespace TP6Programacion.Shared.DTO.Venta.Request;

public record CrearCompraOnlineRequest(
    List<CrearVentaItemRequest> Items
);
