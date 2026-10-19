using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

// Esto, Sol, va a ser para que se pueda distinguir a la hora de que impacten en un endpoint si es mobile o web. Es re útil
// por el tema del CSRF ya que esto solo se tiene que preveer cuando es a través de web.
namespace TP6Programacion.Backend.Filters;

public class CookieCsrfFilter : IAsyncAuthorizationFilter
{
    private readonly IAntiforgery _antiforgery;

    public CookieCsrfFilter(IAntiforgery antiforgery)
    {
        _antiforgery = antiforgery;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var request = context.HttpContext.Request;

        if (HttpMethods.IsGet(request.Method) || HttpMethods.IsHead(request.Method) || HttpMethods.IsOptions(request.Method))
            return;

        if (request.Headers.ContainsKey("Authorization"))
            return;

        if (!request.Cookies.ContainsKey("access_token"))
            return;

        try
        {
            await _antiforgery.ValidateRequestAsync(context.HttpContext);
        }
        catch (AntiforgeryValidationException)
        {
            context.Result = new BadRequestObjectResult("El token CSRF no es válido.");
        }
    }
}