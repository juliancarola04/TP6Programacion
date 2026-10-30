
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using TP6Programacion.Shared.DTO.Ingreso.Request;
using TP6Programacion.Shared.DTO.Ingreso.Response;
using TP6Programacion.Shared.DTO.Paginado.Request.Ingreso;
using TP6Programacion.Shared.DTO.Paginado.Response;

namespace TP6Programacion.Web.Services;

public class IngresoService
{
    private readonly AutenticacionService _autenticacionService;

    public IngresoService(AutenticacionService autenticacionService)
    {
        _autenticacionService = autenticacionService;
    }

    public async Task<PaginadoResponseDto<IngresoListadoResponse>> ObtenerIngresos(
        ParametroPaginacionIngresoRequest parametros)
    {
        var filtros = new Dictionary<string, string>
        {
            ["NumeroPagina"] = (parametros.NumeroPagina ?? 1)
                .ToString(CultureInfo.InvariantCulture),

            ["TamanoPagina"] = (parametros.TamanoPagina ?? 10)
                .ToString(CultureInfo.InvariantCulture)
        };

        if (parametros.ProveedorId.HasValue)
            filtros["ProveedorId"] = parametros.ProveedorId.Value
                .ToString(CultureInfo.InvariantCulture);

        if (parametros.Anulado.HasValue)
            filtros["Anulado"] = parametros.Anulado.Value.ToString().ToLowerInvariant();

        if (!string.IsNullOrWhiteSpace(parametros.Buscar))
            filtros["Buscar"] = parametros.Buscar;

        if (!string.IsNullOrWhiteSpace(parametros.OrdenarPor))
            filtros["OrdenarPor"] = parametros.OrdenarPor;

        if (!string.IsNullOrWhiteSpace(parametros.Direccion))
            filtros["Direccion"] = parametros.Direccion;

        string query = string.Join("&", filtros.Select(f =>
            $"{WebUtility.UrlEncode(f.Key)}={WebUtility.UrlEncode(f.Value)}"));

        string ruta = $"api/ingresos?{query}";

        using HttpResponseMessage respuesta =
            await _autenticacionService.EnviarAutenticado(
                () => new HttpRequestMessage(HttpMethod.Get, ruta));

        respuesta.EnsureSuccessStatusCode();

        return await respuesta.Content
            .ReadFromJsonAsync<PaginadoResponseDto<IngresoListadoResponse>>()
            ?? throw new InvalidOperationException(
                "El servidor no devolvió el listado de ingresos.");
    }

    public async Task<IngresoResponse> ObtenerIngreso(int id)
    {
        using HttpResponseMessage respuesta =
            await _autenticacionService.EnviarAutenticado(
                () => new HttpRequestMessage(
                    HttpMethod.Get, $"api/ingresos/{id}"));

        respuesta.EnsureSuccessStatusCode();

        return await respuesta.Content.ReadFromJsonAsync<IngresoResponse>()
            ?? throw new InvalidOperationException(
                "El servidor no devolvió los datos del ingreso.");
    }

    public async Task<IngresoResponse> CrearIngreso(CrearIngresoRequest request)
    {
        using HttpResponseMessage respuesta =
            await _autenticacionService.EnviarAutenticado(
                () => new HttpRequestMessage(HttpMethod.Post, "api/ingresos")
                {
                    Content = JsonContent.Create(request)
                });

        respuesta.EnsureSuccessStatusCode();

        return await respuesta.Content.ReadFromJsonAsync<IngresoResponse>()
            ?? throw new InvalidOperationException(
                "El servidor no devolvió los datos del ingreso creado.");
    }

    public async Task AnularIngreso(int id)
    {
        using HttpResponseMessage respuesta =
            await _autenticacionService.EnviarAutenticado(
                () => new HttpRequestMessage(
                    HttpMethod.Delete, $"api/ingresos/{id}"));

        respuesta.EnsureSuccessStatusCode();
    }
}

