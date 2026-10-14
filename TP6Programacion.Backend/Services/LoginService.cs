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
        
        public LoginService(ITokenService tokenService, IUsuarioRepository usuarioRepository)
        {
            _tokenService = tokenService;
            _usuarioRepository = usuarioRepository;
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
                    (string token, DateTime expiracion) = _tokenService.CrearToken(usuario);

                    LoginResponse loginResponse = new LoginResponse(token);

                    return loginResponse;
                }
                else
                {
                    throw new DatosLlegaronErradosException("La contraseña ingresada no coincide.");
                }
            
        }

    }
}
