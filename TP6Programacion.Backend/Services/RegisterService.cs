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
    // todo el que se registra es cliente hasta que lo de de alta un admin.

    //separo la creacion de usuario de la creacion de sesion (un empleado dado de alto por un admin no usa tokens)
    private async Task<Usuario> CrearUsuario(string username, string password, string email, Rol rol)
    {
        if (!Validaciones.EstanDatosBien(username, password, email))
            throw new DatosLlegaronErradosException(
                "Alguno de los datos llegó vacío.");

        username = username.Trim();
        password = password.Trim();
        email = email.Trim();

        if (username.Length > 30 || email.Length > 320)
            throw new DatosLlegaronErradosException(
                "El usuario admite hasta 30 caracteres y el e-mail hasta 320.");

        if (!Validaciones.EsUnEmailValido(email))
            throw new DatosLlegaronErradosException(
                "El formato del E-Mail es inválido.");

        if (await usuarioRepository.ExistePorUsername(username))
            throw new RecursoExistenteException(
                "Ya existe alguien con ese usuario.");

        if (await usuarioRepository.ExistePorEmail(email))
            throw new RecursoExistenteException(
                "Ya existe alguien con ese email.");

        var usuario = new Usuario
        {
            Username = username,
            Password = BCrypt.Net.BCrypt.EnhancedHashPassword(password),
            Email = email,
            Rol = rol
        };

        await repo.Registrarse(usuario);

        return usuario;
    }
    private async Task<RegisterResponse> CrearCuenta(RegisterRequest request, Rol rol)
    {
        Usuario usuario = await CrearUsuario(request.Username, request.Password, request.Email, rol);
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
    public async Task CrearEmpleado(CrearEmpleadoRequest request)
    {
        if (request.Rol is not (Rol.Cajero or Rol.Administrador))
            throw new DatosLlegaronErradosException(
                "El rol debe ser Cajero o Administrador.");

        await CrearUsuario(
            request.Username,
            request.Password,
            request.Email,
            request.Rol);
    }
}
