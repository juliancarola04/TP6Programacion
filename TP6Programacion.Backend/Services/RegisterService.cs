using TP6Programacion.Backend.Excepciones;
using TP6Programacion.Backend.Models;
using TP6Programacion.Backend.Repositories;
using TP6Programacion.Backend.Utilidades;
using TP6Programacion.Shared.DTO.Auth.Request;
using TP6Programacion.Shared.DTO.Auth.Response;

namespace TP6Programacion.Backend.Services
{
    public class RegisterService
    {
        private readonly IRegisterRepository _repo;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ITokenService _tokenService;


        public RegisterService(IRegisterRepository repo, IUsuarioRepository usuarioRepository, ITokenService tokenService)
        {
            _repo = repo;
            _usuarioRepository = usuarioRepository;
            _tokenService = tokenService;
        }

        public async Task<RegisterResponse?> Registrarse(RegisterRequest registerRequest)
        {
            string username = registerRequest.Username.Trim();
            string password = registerRequest.Password.Trim();
            string email = registerRequest.Email.Trim();

            if (Validaciones.EstanDatosBien(username, password, email) == false)
            {
                throw new DatosLlegaronErradosException("Alguno de los datos llegó vacío.");
            }
            
            if (!Validaciones.EsUnEmailValido(email))
            {
                throw new DatosLlegaronErradosException("El formato del E-Mail es inválido.");
            }


                if (await _usuarioRepository.ExistePorUsername(username))
                {
                    throw new RecursoExistenteException("Ya existe alguien con ese usuario.");
                }
                
                if (await _usuarioRepository.ExistePorEmail(email))
                {
                    throw new RecursoExistenteException("Ya existe alguien con ese email.");
                }

            
            Usuario usuario = new Usuario()
            {
                Username = username,
                Password = BCrypt.Net.BCrypt.EnhancedHashPassword(password),
                Email = email,
            };


                await _repo.Registrarse(usuario);

                (string token, DateTime expiracion) = _tokenService.CrearToken(usuario);

                RegisterResponse registerDtoOutput = new RegisterResponse(token);

                return registerDtoOutput;


        }
    }
}
