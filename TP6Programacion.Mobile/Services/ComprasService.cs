using System.Net;
using System.Net.Http.Json;
using TP6Programacion.Shared.DTO.Venta.Request;
using TP6Programacion.Shared.DTO.Venta.Response;
using TP6Programacion.Shared.Enums;

namespace TP6Programacion.Mobile.Services;

public class ComprasService
{
    private readonly AutenticacionService _autenticacionService;

    public ComprasService(AutenticacionService autenticacionService)
    {
        _autenticacionService = autenticacionService;
    }

    public async Task<VentaResponse> CrearCompraOnline(List<CrearVentaItemRequest> items)
    {
        CrearCompraOnlineRequest request = new CrearCompraOnlineRequest(items);

        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(() =>
            new HttpRequestMessage(HttpMethod.Post, "api/ventas/online")
            {
                Content = JsonContent.Create(request)
            });

        return await LeerVenta(respuesta, "No se pudo iniciar la compra.");
    }

    public async Task<VentaResponse> Pagar(int ventaId, MetodoPago metodo)
    {
        PagarVentaRequest request = new PagarVentaRequest(metodo);

        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(() =>
            new HttpRequestMessage(HttpMethod.Post, $"api/ventas/{ventaId}/pago")
            {
                Content = JsonContent.Create(request)
            });

        return await LeerVenta(respuesta, "No se pudo procesar el pago.");
    }

    public async Task<VentaResponse> ObtenerMiCompra(int ventaId)
    {
        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(() =>
            new HttpRequestMessage(HttpMethod.Get, $"api/ventas/mis-compras/{ventaId}"));

        return await LeerVenta(respuesta, "No se pudo obtener la compra.");
    }

    private static async Task<VentaResponse> LeerVenta(HttpResponseMessage respuesta, string mensajePorDefecto)
    {
        if (!respuesta.IsSuccessStatusCode)
        {
            if (respuesta.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new InvalidOperationException("Solo las cuentas de cliente pueden cfrealizar compras.");
            }

            string mensaje = await respuesta.Content.ReadAsStringAsync();

            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(mensaje) ? mensajePorDefecto : mensaje);
        }

        VentaResponse? venta = await respuesta.Content.ReadFromJsonAsync<VentaResponse>();

        if (venta is null)
        {
            throw new InvalidOperationException("El servidor no devolvió los datos de la compra.");
        }

        return venta;
    }
}
