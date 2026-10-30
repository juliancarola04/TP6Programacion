using System.Net.Http.Json;
using TP6Programacion.Shared.DTO.Categoria.Request;
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

    public async Task Crear(CrearCategoriaRequest request)
    {
        using var respuesta = await _autenticacionService.EnviarAutenticado(() => new HttpRequestMessage(HttpMethod.Post, "api/categorias")
        {
            Content = JsonContent.Create(request)
        });

        if (respuesta.IsSuccessStatusCode)
        {
            return;
        }

        string mensaje = await respuesta.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(mensaje))
        {
            mensaje = "No se pudo completar la operación.";
        }

        throw new InvalidOperationException(mensaje);
    }

    public async Task Actualizar(int id, ActualizarCategoriaRequest request)
    {
        using var respuesta = await _autenticacionService.EnviarAutenticado(() => new HttpRequestMessage(HttpMethod.Put, $"api/categorias/{id}")
        {
            Content = JsonContent.Create(request)
        });

        if (respuesta.IsSuccessStatusCode)
        {
            return;
        }

        string mensaje = await respuesta.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(mensaje))
        {
            mensaje = "No se pudo completar la operación.";
        }

        throw new InvalidOperationException(mensaje);
    }

    public async Task Eliminar(int id)
    {
        using var respuesta = await _autenticacionService.EnviarAutenticado(() => new HttpRequestMessage(HttpMethod.Delete, $"api/categorias/{id}"));

        if (respuesta.IsSuccessStatusCode)
        {
            return;
        }

        string mensaje = await respuesta.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(mensaje))
        {
            mensaje = "No se pudo completar la operación.";
        }

        throw new InvalidOperationException(mensaje);
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