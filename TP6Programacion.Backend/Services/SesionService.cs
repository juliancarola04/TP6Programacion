using Microsoft.EntityFrameworkCore;
using TP6Programacion.Backend.Excepciones;
using TP6Programacion.Backend.Repositories;
using TP6Programacion.Shared.DTO.Auth.Response;

namespace TP6Programacion.Backend.Services;

public class SesionService
{
    private readonly ISesionUsuarioRepository _sesionRepository;
    private readonly ITokenService _tokenService;

    public SesionService(ISesionUsuarioRepository sesionRepository, ITokenService tokenService)
    {
        _sesionRepository = sesionRepository;
        _tokenService = tokenService;
    }

    public async Task<LoginResponse> Renovar(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new DatosLlegaronErradosException("El refresh token está vacío.");

        var hash = _tokenService.CalcularHash(refreshToken);
        var sesion = await _sesionRepository.ObtenerPorRefreshToken(hash);
        var ahora = DateTime.UtcNow;

        if (sesion is null || sesion.Revocada is not null || sesion.Expira <= ahora || sesion.Usuario is null || sesion.Usuario.Eliminado)
        {
            throw new DatosLlegaronErradosException("La sesión no es válida.");
        }

        var (token, _) = _tokenService.CrearToken(sesion.Usuario);
        var nuevoRefreshToken = _tokenService.CrearRefreshToken();

        sesion.RefreshToken = _tokenService.CalcularHash(nuevoRefreshToken);
        sesion.Expira = ahora.AddDays(30);

        try
        {
            await _sesionRepository.GuardarCambios();
        }
        catch (DbUpdateException)
        {
            throw new BaseDeDatosException("No se pudo actualizar la sesión.");
        }

        return new LoginResponse(token, nuevoRefreshToken, sesion.Expira);
    }
}