namespace TP6Programacion.Shared.DTO.Venta.Request;

public record CrearVentaItemRequest(int ProductoId, int Cantidad);

public record CrearVentaRequest(
    int ClienteId,
    List<CrearVentaItemRequest> Items);