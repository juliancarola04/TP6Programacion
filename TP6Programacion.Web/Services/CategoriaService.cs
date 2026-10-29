using System.Net.Http.Json;
using TP6Programacion.Shared.DTO.Categoria.Response;
using TP6Programacion.Shared.DTO.Paginado.Response;

namespace TP6Programacion.Web.Services;

public class CategoriaService
{
    private const int TamanoPagina = 50;
    private const int MaximoPaginas = 50;

    private readonly AutenticacionService _autenticacionService;

    public CategoriaService(AutenticacionService autenticacionService)
    {
        _autenticacionService = autenticacionService;
    }

    public async Task<List<CategoriaResponse>> ObtenerCategorias()
    {
        List<CategoriaResponse> categorias = new();
        int pagina = 1;

        while (pagina <= MaximoPaginas)
        {
            PaginadoResponseDto<CategoriaResponse> resultado = await ObtenerPagina(pagina);

            // Si el servidor no respetó la página pedida, se corta para no duplicar datos ni quedar en un bucle.
            if (resultado.NumeroPagina != pagina)
            {
                break;
            }

            categorias.AddRange(resultado.Datos);

            if (!resultado.TienePaginaPosterior)
            {
                break;
            }

            pagina++;
        }

        return categorias;
    }

    private async Task<PaginadoResponseDto<CategoriaResponse>> ObtenerPagina(int pagina)
    {
        string ruta = $"api/categorias?NumeroPagina={pagina}&TamanoPagina={TamanoPagina}";

        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(
            () => new HttpRequestMessage(HttpMethod.Get, ruta));

        respuesta.EnsureSuccessStatusCode();

        return await respuesta.Content.ReadFromJsonAsync<PaginadoResponseDto<CategoriaResponse>>()
               ?? throw new InvalidOperationException("No se pudieron obtener las categorías.");
    }
}