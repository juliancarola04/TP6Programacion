using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TP6Programacion.Shared.DTO.PDFs.Response;

namespace TP6Programacion.Backend.GeneracionPDFs
{
    public class ReporteVenta : IDocument
    {
        private readonly ComprobanteVentaResponse _comprobante;
        private readonly CultureInfo _cultura = CultureInfo.GetCultureInfo("es-AR");

        public ReporteVenta(ComprobanteVentaResponse comprobante)
        {
            _comprobante = comprobante;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            var venta = _comprobante.Venta;

            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(column =>
                {
                    column.Item().Text($"Venta #{venta.Id}").FontSize(20).Bold();
                    column.Item().Text($"Fecha: {venta.Fecha:dd/MM/yyyy HH:mm} UTC");
                    column.Item().Text($"Cliente: {venta.ClienteNombre ?? "Consumidor final"}");
                    column.Item().Text($"Estado: {venta.Estado}");
                });

                page.Content().PaddingVertical(20).Column(column =>
                {
                    column.Item().Text("Detalle").FontSize(14).Bold();

                    column.Item().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn();
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Padding(5).Text("Producto").Bold();
                            header.Cell().Padding(5).AlignRight().Text("Cantidad").Bold();
                            header.Cell().Padding(5).AlignRight().Text("Precio").Bold();
                            header.Cell().Padding(5).AlignRight().Text("Subtotal").Bold();
                        });

                        foreach (var detalle in venta.Detalles)
                        {
                            table.Cell().Padding(5).Text(detalle.ProductoNombre);
                            table.Cell().Padding(5).AlignRight().Text(detalle.Cantidad.ToString());
                            table.Cell().Padding(5).AlignRight().Text(detalle.PrecioUnitario.ToString("C2", _cultura));
                            table.Cell().Padding(5).AlignRight().Text(detalle.Subtotal.ToString("C2", _cultura));
                        }
                    });

                    column.Item().PaddingTop(15).AlignRight()
                        .Text($"Total: {venta.Total.ToString("C2", _cultura)}")
                        .FontSize(16).Bold();

                    column.Item().PaddingTop(20).Text("Pagos").FontSize(14).Bold();

                    if (_comprobante.Pagos.Count == 0)
                    {
                        column.Item().Text("No hay pagos registrados.");
                    }

                    foreach (var pago in _comprobante.Pagos)
                    {
                        column.Item().PaddingTop(10).Column(datos =>
                        {
                            datos.Item().Text($"Monto: {pago.Monto.ToString("C2", _cultura)}");
                            datos.Item().Text($"Medio de pago: {pago.Metodo}");
                            datos.Item().Text($"Referencia: {pago.ReferenciaPago ?? "Sin referencia"}");
                            datos.Item().Text($"Estado: {pago.Estado}");
                        });
                    }
                });

                page.Footer().AlignRight().Text(text =>
                {
                    text.Span("Página ");
                    text.CurrentPageNumber();
                    text.Span(" de ");
                    text.TotalPages();
                });
            });
        }
    }
}
