using System.Net.Http.Json;
using TP6Programacion.Shared.DTO.Paginado.Response;
using TP6Programacion.Shared.DTO.Proveedor.Request;
using TP6Programacion.Shared.DTO.Proveedor.Response;

namespace TP6Programacion.Web.Services;

public class ProveedorService
{
    private readonly AutenticacionService _autenticacionService;

    public ProveedorService(AutenticacionService autenticacionService)
    {
        _autenticacionService = autenticacionService;
    }

    public async Task<PaginadoResponseDto<ObtenerProveedorResponse>> ObtenerProveedores(int pagina)
    {
        string ruta = $"api/proveedores/admin?NumeroPagina={pagina}&TamanoPagina=10&Eliminado=false&OrdenarPor=razonsocial&Direccion=asc";

        using var respuesta = await _autenticacionService.EnviarAutenticado(() => new HttpRequestMessage(HttpMethod.Get, ruta));

        if (!respuesta.IsSuccessStatusCode)
        {
            string mensaje = await respuesta.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(mensaje))
            {
                mensaje = "No se pudieron obtener los proveedores.";
            }

            throw new InvalidOperationException(mensaje);
        }

        var resultado = await respuesta.Content.ReadFromJsonAsync<PaginadoResponseDto<ObtenerProveedorResponse>>();

        if (resultado is null)
        {
            throw new InvalidOperationException("El servidor no devolvió los proveedores.");
        }

        return resultado;
    }

    public async Task Crear(CrearProveedorRequest request)
    {
        using var respuesta = await _autenticacionService.EnviarAutenticado(() => new HttpRequestMessage(HttpMethod.Post, "api/proveedores/admin/crear")
        {
            Content = JsonContent.Create(request)
        });

        if (!respuesta.IsSuccessStatusCode)
        {
            string mensaje = await respuesta.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(mensaje))
            {
                mensaje = "No se pudo crear el proveedor.";
            }

            throw new InvalidOperationException(mensaje);
        }
    }

    public async Task Actualizar(int id, ActualizarProveedorRequest request)
    {
        using var respuesta = await _autenticacionService.EnviarAutenticado(() => new HttpRequestMessage(HttpMethod.Put, $"api/proveedores/admin/actualizar/{id}")
        {
            Content = JsonContent.Create(request)
        });

        if (!respuesta.IsSuccessStatusCode)
        {
            string mensaje = await respuesta.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(mensaje))
            {
                mensaje = "No se pudo actualizar el proveedor.";
            }

            throw new InvalidOperationException(mensaje);
        }
    }

    public async Task DarDeBaja(int id)
    {
        using var respuesta = await _autenticacionService.EnviarAutenticado(() => new HttpRequestMessage(HttpMethod.Delete, $"api/proveedores/admin/dardebaja/{id}"));

        if (!respuesta.IsSuccessStatusCode)
        {
            string mensaje = await respuesta.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(mensaje))
            {
                mensaje = "No se pudo dar de baja al proveedor.";
            }

            throw new InvalidOperationException(mensaje);
        }
    }
}