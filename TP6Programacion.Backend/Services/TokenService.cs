using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TP6Programacion.Backend.Models;
using TP6Programacion.Backend.Options;
using TP6Programacion.Backend.Repositories;

namespace TP6Programacion.Backend.Services;

// De acá lo saqué.
// https://codewithmukesh.com/blog/jwt-authentication-in-aspnet-core/
public class TokenService(IOptions<JwtSettings> jwtSettings) : ITokenService
{
    // Hay que dejar las settings del Jwt en algún lado. Yo te recomiendo en un user secret.
    private readonly JwtSettings _settings = jwtSettings.Value;

    public (string token, DateTime expiracion) CrearToken(Usuario usuario)
    {
        var expiracion = DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Email),
            new("username", usuario.Username),
            new(ClaimTypes.Role, usuario.Rol.ToString())
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiracion,
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            SigningCredentials = credentials
        };

        var handler = new JsonWebTokenHandler();
        var token = handler.CreateToken(descriptor);

        return (token, expiracion);
    }

    public string CrearRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    public string CalcularHash(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        return Convert.ToHexString(SHA256.HashData(bytes));
    }
}