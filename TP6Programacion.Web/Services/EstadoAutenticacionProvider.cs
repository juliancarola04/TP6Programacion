using Microsoft.AspNetCore.Components.Authorization;
using System.Net;
using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace TP6Programacion.Web.Services;

// Sol, te voy a comentar este código para que quede un poquito más claro lo que estamos haciendo.
// AuthenticationStateProvider es la clase que se utiliza para el tema de AuthorizeView y AuthorizeRouteView.
public class EstadoAutenticacionProvider : AuthenticationStateProvider
{
    private readonly AutenticacionService _autenticacionService;
    
    // Este va a ser el objeto que va a representar el usuario.
    // ClaimsIdentity va a representar una identity en si, con sus claims y el tipo de autenticación (tipo de autenticación = forma de identificar a ese ClaimsIdentity. Podría provenir de
    // nuestra API Backend, pero también podría provenir de otro lado como otro sistema de la empresa o lo que sea)
    // mientras que ClaimsPrincipal es como un contenedor de todo lo que representa ClaimsIdentity.
    // Apenas empieza esto está vacío.
    // Son dos cosas distintas porque un ClaimsPrinciapl podría tener varios ClaimsIdentity. Cada uno se identificaría por su AuthenticationType.
    
    // Como este provider lo tenemos en el contenedor de dependencias inyectado como Scoped, va a vivir siempre y cuando no cerremos o actualicemos la página.
    // Pero ese _usuario vive en memoria.
    private ClaimsPrincipal _usuario = new ClaimsPrincipal(new ClaimsIdentity());
    public EstadoAutenticacionProvider(AutenticacionService autenticacionService)
    {
        _autenticacionService = autenticacionService;
    }

    // Esta función que sobreescribimos va a ser la que se va a utilizar cuando Blazor necesite saber el usuario conectado (en los AuthorizeRouteView y AuthroizeView).
    // Como podés ver, le enviamos este _usuario que teníamos en memoria.
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(new AuthenticationState(_usuario));
    }

    // Cuando el usuario vaya navegando a través de las distintas páginas va a ir llamando a esta función. Esta función de lo que se va a encargar
    // es de a medida que el chico vaya moviéndose a través de las distintas páginas verifique que puede ver eso. Nunca vamos a contactar de nuevo
    // al backend para esto, siempre es con lo que tenemos.
    // Cuando sí vemos en el backend es cuando se le vence el JWT. Ahí llamamos a refresh y toda la bola.
    // Esto está en App.razor si lo querés ver.
    public async Task Actualizar()
    {
        ClaimsPrincipal nuevoUsuario;

        try
        {
            string? token = await _autenticacionService.ObtenerTokenVigente();

            nuevoUsuario = new ClaimsPrincipal(new ClaimsIdentity());

            if (!string.IsNullOrWhiteSpace(token))
            {
                var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
                
                // Acá en este constructor el username va a ser lo que nos va a retornar cuando pongamos "usuario.Identity.Name". Tenemos que aclararlo porque en TokenService establecimos que Name se almacene así, como username.
                // Y ClaimTypes.Role va a ser el nombre del Claim que contiene los roles (el de por defecto en este caso). Va a ser lo que se va a buscar a la hora de chequear los perms.
                var identidad = new ClaimsIdentity(jwt.Claims, "Bearer", "username", ClaimTypes.Role);

                nuevoUsuario = new ClaimsPrincipal(identidad);
            }
        }
        catch (HttpRequestException e) when (e.StatusCode == HttpStatusCode.Unauthorized)
        {
            nuevoUsuario = new ClaimsPrincipal(new ClaimsIdentity());
        }

        // Lo almacenamos en memoria.
        _usuario = nuevoUsuario;
        
        // Y acá le informo a los componentes que el estado de la autenticación cambió.
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
