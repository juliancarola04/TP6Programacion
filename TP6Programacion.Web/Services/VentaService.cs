using System.Net;
using System.Net.Http.Json;
using TP6Programacion.Shared.DTO.Paginado.Response;
using TP6Programacion.Shared.DTO.Venta.Request;
using TP6Programacion.Shared.DTO.Venta.Response;

namespace TP6Programacion.Web.Services;

public class VentasService
{
    private readonly AutenticacionService _autenticacionService;

    public VentasService(AutenticacionService autenticacionService)
    {
        _autenticacionService = autenticacionService;
    }

    public async Task<VentaResponse> CrearVentaPresencial(CrearVentaPresencialRequest request)
    {
        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(() =>
            new HttpRequestMessage(HttpMethod.Post, "api/ventas/presencial")
            {
                Content = JsonContent.Create(request)
            });

        if (!respuesta.IsSuccessStatusCode)
        {
            if (respuesta.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new InvalidOperationException("Tu usuario no tiene permiso para registrar ventas.");
            }

            string mensaje = await respuesta.Content.ReadAsStringAsync();

            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(mensaje) ? "No se pudo guardar la venta." : mensaje);
        }

        return await respuesta.Content.ReadFromJsonAsync<VentaResponse>()
               ?? throw new InvalidOperationException("El servidor no devolvió los datos de la venta.");
    }

    public async Task<PaginadoResponseDto<VentaListadoResponse>> ObtenerVentas(int pagina)
    {
        string ruta = $"api/ventas?NumeroPagina={pagina}&TamanoPagina=10&OrdenarPor=fecha&Direccion=desc";

        using var respuesta = await _autenticacionService.EnviarAutenticado(() => new HttpRequestMessage(HttpMethod.Get, ruta));

        respuesta.EnsureSuccessStatusCode();

        var resultado = await respuesta.Content.ReadFromJsonAsync<PaginadoResponseDto<VentaListadoResponse>>();

        if (resultado is null)
        {
            throw new InvalidOperationException("No se pudieron obtener las ventas.");
        }

        return resultado;
    }

    public async Task<VentaResponse> ObtenerVenta(int id)
    {
        using var respuesta = await _autenticacionService.EnviarAutenticado(() => new HttpRequestMessage(HttpMethod.Get, $"api/ventas/{id}"));

        respuesta.EnsureSuccessStatusCode();

        var venta = await respuesta.Content.ReadFromJsonAsync<VentaResponse>();

        if (venta is null)
        {
            throw new InvalidOperationException("No se pudo obtener la venta.");
        }

        return venta;
    }

    public async Task<byte[]> ObtenerComprobante(int ventaId)
    {
        string ruta = $"api/ventas/{ventaId}/pdf";

        using var respuesta = await _autenticacionService.EnviarAutenticado(() => new HttpRequestMessage(HttpMethod.Get, ruta));

        if (!respuesta.IsSuccessStatusCode)
        {
            string mensaje = await respuesta.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(mensaje))
            {
                mensaje = "No se pudo obtener el comprobante.";
            }

            throw new InvalidOperationException(mensaje);
        }

        return await respuesta.Content.ReadAsByteArrayAsync();
    }
}