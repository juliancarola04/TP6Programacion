using TP6Programacion.Backend.Models;

namespace TP6Programacion.Backend.Repositories;

public interface ISesionUsuarioRepository
{
    Task Crear(SesionUsuario sesion);
    Task<SesionUsuario?> ObtenerPorRefreshToken(string refreshToken);
    Task GuardarCambios();
}