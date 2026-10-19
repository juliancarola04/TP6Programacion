using TP6Programacion.Backend.Excepciones;
using TP6Programacion.Backend.Models;
using TP6Programacion.Backend.Repositories;
using TP6Programacion.Backend.Utilidades;
using TP6Programacion.Shared.DTO.Auth.Request;
using TP6Programacion.Shared.DTO.Auth.Response;

namespace TP6Programacion.Backend.Services
{
    public class LoginService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ITokenService _tokenService;
        private readonly ISesionUsuarioRepository _sesionRepository;
        
        public LoginService(ITokenService tokenService, IUsuarioRepository usuarioRepository, ISesionUsuarioRepository sesionRepository)
        {
            _tokenService = tokenService;
            _usuarioRepository = usuarioRepository;
            _sesionRepository = sesionRepository;
        }


        public async Task<LoginResponse?> Login(LoginRequest loginRequest)
        {
            string username = loginRequest.Username.Trim();
            string password = loginRequest.Password.Trim();

            if (Validaciones.EstanDatosBien(username, password) == false)
            {
                throw new DatosLlegaronErradosException("Ya sea el usuario o la contraseña llegaron vacíos.");
            }
                Usuario? usuario = await _usuarioRepository.BuscarPorUsername(username);
                
                if (usuario is null)
                {
                    throw new RecursoNoExisteException("No existe ningún usuario con ese usuario.");
                }

                bool sonIguales = BCrypt.Net.BCrypt.EnhancedVerify(password, usuario.Password);

                if (sonIguales)
                {
                    var (token, _) = _tokenService.CrearToken(usuario);
                    var refreshToken = _tokenService.CrearRefreshToken();

                    var sesion = new SesionUsuario
                    {
                        UsuarioId = usuario.Id,
                        RefreshToken = _tokenService.CalcularHash(refreshToken),
                        Expira = DateTime.UtcNow.AddDays(30)
                    };

                    await _sesionRepository.Crear(sesion);

                    return new LoginResponse(token, refreshToken, sesion.Expira);
                }
                else
                {
                    throw new DatosLlegaronErradosException("La contraseña ingresada no coincide.");
                }
            
        }

    }
}
