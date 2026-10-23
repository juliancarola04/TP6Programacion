using Microsoft.AspNetCore.Components.WebAssembly.Http;
using System.Net.Http.Json;
using TP6Programacion.Shared.DTO.Auth.Request;
using TP6Programacion.Shared.DTO.Auth.Response;

namespace TP6Programacion.Web.Services;

public class AutenticacionService
{
    private readonly HttpClient _httpClient;

    public AutenticacionService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task IniciarSesion(LoginRequest request)
    {
        await EnviarConCsrf(HttpMethod.Post, "api/auth/web/login", JsonContent.Create(request), "No se pudo iniciar sesión.");
    }

    public async Task Registrarse(RegisterRequest request)
    {
        await EnviarConCsrf(HttpMethod.Post, "api/auth/web/register", JsonContent.Create(request), "No se pudo crear la cuenta.");
    }

    public async Task<UsuarioActualResponse> ObtenerUsuarioActual()
    {
        try
        {
            return await ConsultarUsuarioActual();
        }
        catch (HttpRequestException e) when (e.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            await RenovarSesion();

            return await ConsultarUsuarioActual();
        }
    }

    private async Task<UsuarioActualResponse> ConsultarUsuarioActual()
    {
        using HttpRequestMessage solicitud = new HttpRequestMessage(HttpMethod.Get, "api/auth/usuario-actual");
        solicitud.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

        using HttpResponseMessage respuesta = await _httpClient.SendAsync(solicitud);
        respuesta.EnsureSuccessStatusCode();

        UsuarioActualResponse? usuario = await respuesta.Content.ReadFromJsonAsync<UsuarioActualResponse>();

        if (usuario is null)
        {
            throw new InvalidOperationException("No se pudieron obtener los datos del usuario.");
        }

        return usuario;
    }

    public async Task CerrarSesion()
    {
        await EnviarConCsrf(HttpMethod.Post, "api/auth/web/logout", null, "No se pudo cerrar la sesión.");
    }

    private async Task RenovarSesion()
    {
        await EnviarConCsrf(HttpMethod.Post, "api/auth/web/refresh", null, "No se pudo renovar la sesión.", true);
    }

    private async Task EnviarConCsrf(
        HttpMethod metodo, string ruta, HttpContent? contenido, string mensajeError, bool requiereSesion = false)
    {
        using HttpRequestMessage solicitud = new HttpRequestMessage(metodo, ruta);
        solicitud.Content = contenido;

        string tokenCsrf = await ObtenerTokenCsrf();

        solicitud.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        solicitud.Headers.Add("X-CSRF-TOKEN", tokenCsrf);

        using HttpResponseMessage respuesta = await _httpClient.SendAsync(solicitud);

        if (requiereSesion && respuesta.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            respuesta.EnsureSuccessStatusCode();
        }

        if (!respuesta.IsSuccessStatusCode)
        {
            string mensaje = await respuesta.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(mensaje))
            {
                mensaje = mensajeError;
            }

            throw new InvalidOperationException(mensaje);
        }
    }

    private async Task<string> ObtenerTokenCsrf()
    {
        using HttpRequestMessage solicitudCsrf = new HttpRequestMessage(HttpMethod.Get, "api/auth/web/csrf");
        solicitudCsrf.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

        // Acá sería donde volvería el JSON que desp lo vamos a usar
        using HttpResponseMessage respuestaCsrf = await _httpClient.SendAsync(solicitudCsrf);
        respuestaCsrf.EnsureSuccessStatusCode();

        CsrfResponse? csrf = await respuestaCsrf.Content.ReadFromJsonAsync<CsrfResponse>();

        if (csrf is null || string.IsNullOrWhiteSpace(csrf.Token))
        {
            throw new InvalidOperationException("Falló el generar el token CSRF.");
        }

        return csrf.Token;
    }

    private record CsrfResponse(string Token);
}
