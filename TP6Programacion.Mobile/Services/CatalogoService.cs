
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using TP6Programacion.Shared.DTO.Paginado.Request.Producto;
using TP6Programacion.Shared.DTO.Paginado.Response;
using TP6Programacion.Shared.DTO.Producto.Response;

namespace TP6Programacion.Mobile.Services;

public class CatalogoService
{
    private readonly HttpClient _httpClient;

    public CatalogoService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PaginadoResponseDto<ProductoCatalogoResponse>>
        ObtenerCatalogo(ParametroPaginacionProductoRequest parametros)
    {
        var filtros = new Dictionary<string, string>
        {
            ["NumeroPagina"] = (parametros.NumeroPagina ?? 1)
                .ToString(CultureInfo.InvariantCulture),

            ["TamanoPagina"] = (parametros.TamanoPagina ?? 20)
                .ToString(CultureInfo.InvariantCulture)
        };

        if (parametros.CategoriaId.HasValue)
        {
            filtros["CategoriaId"] = parametros.CategoriaId.Value
                .ToString(CultureInfo.InvariantCulture);
        }

        if (!string.IsNullOrWhiteSpace(parametros.Buscar))
        {
            filtros["Buscar"] = parametros.Buscar;
        }

        if (!string.IsNullOrWhiteSpace(parametros.OrdenarPor))
        {
            filtros["OrdenarPor"] = parametros.OrdenarPor;
        }

        if (!string.IsNullOrWhiteSpace(parametros.Direccion))
        {
            filtros["Direccion"] = parametros.Direccion;
        }

        string query = string.Join("&",
            filtros.Select(f =>
                $"{WebUtility.UrlEncode(f.Key)}={WebUtility.UrlEncode(f.Value)}"));

        string ruta = $"api/productos/catalogo?{query}";

        using HttpResponseMessage respuesta = await _httpClient.GetAsync(ruta);

        respuesta.EnsureSuccessStatusCode();

        var catalogo =
            await respuesta.Content.ReadFromJsonAsync<
                PaginadoResponseDto<ProductoCatalogoResponse>>();

        if (catalogo is null)
        {
            throw new InvalidOperationException(
                "El servidor no devolvió los datos del catálogo.");
        }

        return catalogo;
    }
}
