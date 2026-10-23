using System.Net.Http.Json;
using TP6Programacion.Shared.DTO.Paginado.Request.Producto;
using TP6Programacion.Shared.DTO.Paginado.Response;
using TP6Programacion.Shared.DTO.Producto.Response;

namespace TP6Programacion.Mobile.Services;

public class CatalogoService
{
    private readonly AutenticacionService _autenticacionService;

    public CatalogoService(AutenticacionService autenticacionService)
    {
        _autenticacionService = autenticacionService;
    }

    public async Task<PaginadoResponseDto<ProductoCatalogoResponse>> ObtenerCatalogo(ParametroPaginacionProductoRequest parametros)
    {
        var filtros = new List<string>
        {
            $"NumeroPagina={parametros.NumeroPagina ?? 1}",
            $"TamanoPagina={parametros.TamanoPagina ?? 20}"
        };

        if (parametros.CategoriaId.HasValue)
        {
            filtros.Add($"CategoriaId={parametros.CategoriaId.Value}");
        }

        if (!string.IsNullOrWhiteSpace(parametros.Buscar))
        {
            filtros.Add($"Buscar={parametros.Buscar}");
        }

        if (!string.IsNullOrWhiteSpace(parametros.OrdenarPor))
        {
            filtros.Add($"OrdenarPor={parametros.OrdenarPor}");
        }

        if (!string.IsNullOrWhiteSpace(parametros.Direccion))
        {
            filtros.Add($"Direccion={parametros.Direccion}");
        }

        string ruta = $"api/productos/catalogo?{string.Join("&", filtros)}";

        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(() => new HttpRequestMessage(HttpMethod.Get, ruta));

        respuesta.EnsureSuccessStatusCode();

        var catalogo = await respuesta.Content.ReadFromJsonAsync<PaginadoResponseDto<ProductoCatalogoResponse>>();

        if (catalogo is null)
        {
            throw new InvalidOperationException("El servidor no devolvió los datos del catálogo.");
        }

        return catalogo;
    }
}
