using TP6Programacion.Backend.Excepciones;
using TP6Programacion.Backend.Models;
using TP6Programacion.Backend.Repositories;
using TP6Programacion.Backend.Utilidades;
using TP6Programacion.Shared.DTO.Auth.Request;
using TP6Programacion.Shared.DTO.Auth.Response;
using TP6Programacion.Shared.Enums;

namespace TP6Programacion.Backend.Services;

public class RegisterService(
    IRegisterRepository repo,
    IUsuarioRepository usuarioRepository,
    ITokenService tokenService,
    ISesionUsuarioRepository sesionRepository)
{
    public Task<RegisterResponse> Registrarse(RegisterRequest request) => CrearCuenta(request, Rol.Cliente);

    public Task<RegisterResponse> RegistrarseCajero(RegisterRequest request) => CrearCuenta(request, Rol.Cajero);

    private async Task<RegisterResponse> CrearCuenta(RegisterRequest request, Rol rol)
    {
        if (!Validaciones.EstanDatosBien(request.Username, request.Password, request.Email))
            throw new DatosLlegaronErradosException("Alguno de los datos llegó vacío.");

        string username = request.Username.Trim();
        string password = request.Password.Trim();
        string email = request.Email.Trim();

        if (username.Length > 30 || email.Length > 320)
            throw new DatosLlegaronErradosException("El usuario admite hasta 30 caracteres y el e-mail hasta 320.");

        if (!Validaciones.EsUnEmailValido(email))
            throw new DatosLlegaronErradosException("El formato del E-Mail es inválido.");

        if (await usuarioRepository.ExistePorUsername(username))
            throw new RecursoExistenteException("Ya existe alguien con ese usuario.");

        if (await usuarioRepository.ExistePorEmail(email))
            throw new RecursoExistenteException("Ya existe alguien con ese email.");

        var usuario = new Usuario
        {
            Username = username,
            Password = BCrypt.Net.BCrypt.EnhancedHashPassword(password),
            Email = email,
            Rol = rol
        };
        await repo.Registrarse(usuario);

        var (token, _) = tokenService.CrearToken(usuario);
        var refreshToken = tokenService.CrearRefreshToken();
        var sesion = new SesionUsuario
        {
            UsuarioId = usuario.Id,
            RefreshToken = tokenService.CalcularHash(refreshToken),
            Expira = DateTime.UtcNow.AddDays(30)
        };
        await sesionRepository.Crear(sesion);
        return new RegisterResponse(token, refreshToken, sesion.Expira);
    }
}
