using Microsoft.AspNetCore.Components.Authorization;
using System.Net;
using System.Security.Claims;

namespace TP6Programacion.Web.Services;

public class EstadoAutenticacionProvider : AuthenticationStateProvider
{
    private readonly AutenticacionService _autenticacionService;
    private ClaimsPrincipal _usuario = new(new ClaimsIdentity());

    public EstadoAutenticacionProvider(AutenticacionService autenticacionService)
    {
        _autenticacionService = autenticacionService;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(new AuthenticationState(_usuario));
    }

    public async Task Actualizar()
    {
        ClaimsPrincipal nuevoUsuario;

        try
        {
            var usuario = await _autenticacionService.ObtenerUsuarioSesion();

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Username),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Rol.ToString())
            };

            var identidad = new ClaimsIdentity(claims, "Api");
            nuevoUsuario = new ClaimsPrincipal(identidad);
        }
        catch (HttpRequestException e) when (e.StatusCode == HttpStatusCode.Unauthorized)
        {
            nuevoUsuario = new ClaimsPrincipal(new ClaimsIdentity());
        }

        _usuario = nuevoUsuario;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
