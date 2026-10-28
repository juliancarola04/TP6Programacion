using System.Net;
using TP6Programacion.Shared.Enums;
using TP6Programacion.Shared.Auth;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using TP6Programacion.Shared.DTO.Auth.Request;
using TP6Programacion.Shared.DTO.Auth.Response;

namespace TP6Programacion.Web.Services;

public class AutenticacionService
{
    private readonly HttpClient _httpClient;
    private readonly AlmacenamientoTokensService _almacenamientoTokensService;

    public AutenticacionService(HttpClient httpClient, AlmacenamientoTokensService almacenamientoTokensService)
    {
        _httpClient = httpClient;
        _almacenamientoTokensService = almacenamientoTokensService;
    }

    public async Task<UsuarioSesion> ObtenerUsuarioSesion()
    {
        string? token = await ObtenerTokenVigente();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new HttpRequestException("No hay una sesión válida.", null, HttpStatusCode.Unauthorized);
        }

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);

        if (!int.TryParse(jwt.Subject, out int usuarioId) ||
            !Enum.TryParse<Rol>(jwt.GetClaim(ClaimTypes.Role).Value, out var rol) || !Enum.IsDefined(rol))
        {
            throw new InvalidOperationException("Los datos del token no son válidos.");
        }

        return new UsuarioSesion(usuarioId, jwt.GetClaim("username").Value, jwt.GetClaim("email").Value, rol);
    }

    private async Task RenovarSesion()
    {
        LoginResponse? sesion = await _almacenamientoTokensService.Obtener();

        if (sesion is null || string.IsNullOrWhiteSpace(sesion.RefreshToken))
        {
            throw new HttpRequestException("No hay una sesión guardada.", null, System.Net.HttpStatusCode.Unauthorized);
        }

        RefreshRequest request = new RefreshRequest(sesion.RefreshToken);

        using HttpResponseMessage respuesta = await _httpClient.PostAsJsonAsync("api/auth/refresh", request);
        if (respuesta.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            await _almacenamientoTokensService.Limpiar();
        }

        respuesta.EnsureSuccessStatusCode();

        LoginResponse? nuevaSesion = await respuesta.Content.ReadFromJsonAsync<LoginResponse>();

        if (nuevaSesion is null || string.IsNullOrWhiteSpace(nuevaSesion.Token) || string.IsNullOrWhiteSpace(nuevaSesion.RefreshToken))
        {
            throw new InvalidOperationException("El servidor no devolvió una sesión válida.");
        }

        await _almacenamientoTokensService.Guardar(nuevaSesion);
    }

    private async Task<HttpResponseMessage> EnviarConToken(Func<HttpRequestMessage> crearSolicitud)
    {
        LoginResponse? sesion = await _almacenamientoTokensService.Obtener();

        if (sesion is null || string.IsNullOrWhiteSpace(sesion.Token))
        {
            throw new HttpRequestException("No hay una sesión guardada.", null, System.Net.HttpStatusCode.Unauthorized);
        }

        using HttpRequestMessage solicitud = crearSolicitud();
        solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", sesion.Token);

        return await _httpClient.SendAsync(solicitud);
    }

    public async Task<HttpResponseMessage> EnviarAutenticado(Func<HttpRequestMessage> crearSolicitud)
    {
        HttpResponseMessage respuesta = await EnviarConToken(crearSolicitud);

        if (respuesta.StatusCode != System.Net.HttpStatusCode.Unauthorized)
        {
            return respuesta;
        }

        respuesta.Dispose();

        await RenovarSesion();

        return await EnviarConToken(crearSolicitud);
    }

    public async Task CrearEmpleado(CrearEmpleadoRequest request)
    {
        using HttpResponseMessage respuesta = await EnviarAutenticado(() =>
            new HttpRequestMessage(HttpMethod.Post, "api/auth/empleados")
            {
                Content = JsonContent.Create(request)
            });

        if (!respuesta.IsSuccessStatusCode)
        {
            string mensaje = await respuesta.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(mensaje))
                mensaje = "No se pudo crear el empleado.";

            throw new InvalidOperationException(mensaje);
        }
    }

    public async Task CerrarSesion()
    {
        LoginResponse? sesion = await _almacenamientoTokensService.Obtener();

        if (!string.IsNullOrWhiteSpace(sesion?.RefreshToken))
        {
            LogoutRequest request = new LogoutRequest(sesion.RefreshToken);

            using HttpResponseMessage respuesta = await _httpClient.PostAsJsonAsync("api/auth/logout", request);

            if (!respuesta.IsSuccessStatusCode)
            {
                string mensaje = await respuesta.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(mensaje))
                {
                    mensaje = "No se pudo cerrar la sesión.";
                }

                throw new InvalidOperationException(mensaje);
            }
        }

        await _almacenamientoTokensService.Limpiar();
    }

    public async Task IniciarSesion(LoginRequest request)
    {
        using HttpResponseMessage respuesta = await _httpClient.PostAsJsonAsync("api/auth/login", request);

        if (!respuesta.IsSuccessStatusCode)
        {
            string mensaje = await respuesta.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(mensaje))
            {
                mensaje = "No se pudo iniciar sesión.";
            }

            throw new InvalidOperationException(mensaje);
        }

        LoginResponse? sesion = await respuesta.Content.ReadFromJsonAsync<LoginResponse>();

        if (sesion is null || string.IsNullOrWhiteSpace(sesion.Token) || string.IsNullOrWhiteSpace(sesion.RefreshToken))
        {
            throw new InvalidOperationException("El servidor no devolvió una sesión válida.");
        }

        await _almacenamientoTokensService.Guardar(sesion);

        var usuario = await ObtenerUsuarioSesion();

        if (usuario.Rol is not (Rol.Administrador or Rol.Cajero))
        {
            try
            {
                await CerrarSesion();
            }
            finally
            {
                await _almacenamientoTokensService.Limpiar();
            }

            throw new InvalidOperationException(
                "Esta cuenta no tiene acceso al panel web.");
        }
    }
    
    // ESTO es lo que vamos a usar!
    public async Task<string?> ObtenerTokenVigente()
    {
        LoginResponse? sesion = await _almacenamientoTokensService.Obtener();

        if (sesion is null || string.IsNullOrWhiteSpace(sesion.Token))
        {
            return null;
        }

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(sesion.Token);

        if (jwt.ValidTo > DateTime.UtcNow)
        {
            return sesion.Token;
        }

        if (string.IsNullOrWhiteSpace(sesion.RefreshToken))
        {
            return null;
        }

        try
        {
            await RenovarSesion();
        }
        catch (HttpRequestException e) when (e.StatusCode == HttpStatusCode.Unauthorized)
        {
            await _almacenamientoTokensService.Limpiar();
            return null;
        }

        sesion = await _almacenamientoTokensService.Obtener();

        return sesion?.Token;
    }
}
