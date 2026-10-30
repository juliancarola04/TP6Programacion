using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Headers;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using TP6Programacion.Shared.DTO.Paginado.Request.Producto;
using TP6Programacion.Shared.DTO.Paginado.Response;
using TP6Programacion.Shared.DTO.Producto.Request;
using TP6Programacion.Shared.DTO.Producto.Response;

namespace TP6Programacion.Web.Services;

public class ProductoService
{
    private readonly AutenticacionService _autenticacionService;

    public ProductoService(AutenticacionService autenticacionService)
    {
        _autenticacionService = autenticacionService;
    }

    public async Task<PaginadoResponseDto<ProductoListadoResponse>> ObtenerProductos(
        ParametroPaginacionProductoRequest parametros)
    {
        var filtros = new Dictionary<string, string>
        {
            ["NumeroPagina"] = (parametros.NumeroPagina ?? 1)
                .ToString(CultureInfo.InvariantCulture),

            ["TamanoPagina"] = (parametros.TamanoPagina ?? 10)
                .ToString(CultureInfo.InvariantCulture)
        };

        if (!string.IsNullOrWhiteSpace(parametros.Buscar))
            filtros["Buscar"] = parametros.Buscar;

        if (parametros.CategoriaId.HasValue)
            filtros["CategoriaId"] = parametros.CategoriaId.Value
                .ToString(CultureInfo.InvariantCulture);

        if (!string.IsNullOrWhiteSpace(parametros.OrdenarPor))
            filtros["OrdenarPor"] = parametros.OrdenarPor;

        if (!string.IsNullOrWhiteSpace(parametros.Direccion))
            filtros["Direccion"] = parametros.Direccion;

        string query = string.Join("&", filtros.Select(f =>
            $"{WebUtility.UrlEncode(f.Key)}={WebUtility.UrlEncode(f.Value)}"));

        string ruta = $"api/productos?{query}";

        using HttpResponseMessage respuesta =
            await _autenticacionService.EnviarAutenticado(
                () => new HttpRequestMessage(HttpMethod.Get, ruta));

        respuesta.EnsureSuccessStatusCode();

        return await respuesta.Content
            .ReadFromJsonAsync<PaginadoResponseDto<ProductoListadoResponse>>()
            ?? throw new InvalidOperationException(
                "El servidor no devolvió el listado de productos.");
    }

    public async Task<ProductoResponse> ObtenerProducto(int id)
    {
        using HttpResponseMessage respuesta =
            await _autenticacionService.EnviarAutenticado(
                () => new HttpRequestMessage(
                    HttpMethod.Get, $"api/productos/{id}"));

        respuesta.EnsureSuccessStatusCode();

        return await respuesta.Content.ReadFromJsonAsync<ProductoResponse>()
            ?? throw new InvalidOperationException(
                "El servidor no devolvió los datos del producto.");
    }

    public async Task<ProductoListadoResponse> CrearProducto(
        CrearProductoRequest request)
    {
        using HttpResponseMessage respuesta =
            await _autenticacionService.EnviarAutenticado(
                () => new HttpRequestMessage(HttpMethod.Post, "api/productos")
                {
                    Content = JsonContent.Create(request)
                });

        respuesta.EnsureSuccessStatusCode();

        return await respuesta.Content
            .ReadFromJsonAsync<ProductoListadoResponse>()
            ?? throw new InvalidOperationException(
                "El servidor no devolvió el producto creado.");
    }

    public async Task ActualizarProducto(
        int id, ActualizarProductoRequest request)
    {
        using HttpResponseMessage respuesta =
            await _autenticacionService.EnviarAutenticado(
                () => new HttpRequestMessage(
                    HttpMethod.Put, $"api/productos/{id}")
                {
                    Content = JsonContent.Create(request)
                });

        respuesta.EnsureSuccessStatusCode();
    }

    public async Task EliminarProducto(int id)
    {
        using HttpResponseMessage respuesta =
            await _autenticacionService.EnviarAutenticado(
                () => new HttpRequestMessage(
                    HttpMethod.Delete, $"api/productos/{id}"));

        respuesta.EnsureSuccessStatusCode();
    }
    public async Task<ProductoImagenResponse> SubirImagen(
    int productoId,
    IBrowserFile archivo)
    {
        const long maxFileSize = 5 * 1024 * 1024;

        using var contenido = new MultipartFormDataContent();

        await using var stream = archivo.OpenReadStream(maxFileSize);

        using var contenidoArchivo = new StreamContent(stream);

        contenidoArchivo.Headers.ContentType =
            new MediaTypeHeaderValue(archivo.ContentType);

        contenido.Add(contenidoArchivo, "archivo", archivo.Name);

        using HttpResponseMessage respuesta =
            await _autenticacionService.EnviarAutenticado(
                () => new HttpRequestMessage(
                    HttpMethod.Post,
                    $"api/productos/{productoId}/imagen")
                {
                    Content = contenido
                });

        respuesta.EnsureSuccessStatusCode();

        return await respuesta.Content.ReadFromJsonAsync<ProductoImagenResponse>()
            ?? throw new InvalidOperationException(
                "El servidor no devolvió los datos de la imagen.");
    }
}

   /* public async Task<ProductoImagenResponse> SubirImagen(
        int id, IBrowserFile archivo)
    {
        using var contenido = new MultipartFormDataContent();
        using var flujo = archivo.OpenReadStream(5 * 1024 * 1024);

        using var archivoContenido = new StreamContent(flujo);
        archivoContenido.Headers.ContentType =
            new MediaTypeHeaderValue(archivo.ContentType);

        contenido.Add(archivoContenido, "archivo", archivo.Name);

        using HttpResponseMessage respuesta =
            await _autenticacionService.EnviarAutenticado(
                () => new HttpRequestMessage(
                    HttpMethod.Post, $"api/productos/{id}/imagen")
                {
                    Content = contenido
                });

        respuesta.EnsureSuccessStatusCode();

        return await respuesta.Content.ReadFromJsonAsync<ProductoImagenResponse>()
               ?? throw new InvalidOperationException(
                   "El servidor no devolvió los datos de la imagen.");
    }*/

