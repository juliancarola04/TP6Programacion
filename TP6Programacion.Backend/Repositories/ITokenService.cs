using TP6Programacion.Backend.Models;

namespace TP6Programacion.Backend.Repositories
{
    public interface ITokenService
    {
        public (string token, DateTime expiracion) CrearToken(Usuario usuario);
        public string CrearRefreshToken();
        public string CalcularHash(string token);
    }
}
