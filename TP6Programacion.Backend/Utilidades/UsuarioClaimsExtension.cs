using System.Security.Claims;
namespace TP6Programacion.Backend.Utilidades
{
    public static class UsuarioClaimsExtension
    {
        public static int? ObtenerUsuarioId(this ClaimsPrincipal user)
        {
            string? valor = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
            return int.TryParse(valor, out int id) ? id : null;
        }
    }
}
