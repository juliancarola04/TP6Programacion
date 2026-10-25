using System.Security.Claims;
using System.Text.Json;
using TP6Programacion.Shared.Enums;

namespace TP6Programacion.Shared.Auth;

public static class LectorToken
{
    public static (UsuarioSesion Usuario, DateTimeOffset Expira) Leer(string? token)
    {
        try
        {
            var partes = token?.Split('.');
            if (partes is null || partes.Length != 3)
                throw new FormatException();

            string payload = partes[1].Replace('-', '+').Replace('_', '/');
            payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
            using var documento = JsonDocument.Parse(Convert.FromBase64String(payload));
            var datos = documento.RootElement;

            string? id = datos.GetProperty("sub").GetString();
            string? username = datos.GetProperty("username").GetString();
            string? email = datos.GetProperty("email").GetString();
            string? rol = datos.TryGetProperty(ClaimTypes.Role, out var claimRol)
                ? claimRol.GetString()
                : datos.GetProperty("role").GetString();
            var expira = DateTimeOffset.FromUnixTimeSeconds(datos.GetProperty("exp").GetInt64());

            if (!int.TryParse(id, out var usuarioId) || usuarioId <= 0 || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || !Enum.TryParse<Rol>(rol, out var rolUsuario) || !Enum.IsDefined(rolUsuario))
                throw new FormatException();

            return (new UsuarioSesion(usuarioId, username, email, rolUsuario), expira);
        }
        catch (Exception e) when (e is JsonException or FormatException or KeyNotFoundException or InvalidOperationException or ArgumentException or OverflowException)
        {
            throw new FormatException("La sesión guardada no contiene un JWT válido.", e);
        }
    }
}