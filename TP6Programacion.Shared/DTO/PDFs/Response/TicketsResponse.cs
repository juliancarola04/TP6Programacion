using TP6Programacion.Shared.DTO.Venta.Response;
using TP6Programacion.Shared.Enums;

namespace TP6Programacion.Shared.DTO.PDFs.Response;

public record PagoComprobanteResponse(decimal Monto, MetodoPago Metodo, string? ReferenciaPago, EstadoPago Estado);

public record ComprobanteVentaResponse(VentaResponse Venta, List<PagoComprobanteResponse> Pagos);