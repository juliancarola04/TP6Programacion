using TP6Programacion.Backend.Excepciones;
using TP6Programacion.Backend.Models;
using TP6Programacion.Backend.Models.ModeloAuxiliar;
using TP6Programacion.Backend.Models.ModeloAuxiliar.Query.Usuario;
using TP6Programacion.Backend.Repositories;
using TP6Programacion.Backend.Utilidades;
using TP6Programacion.Shared.DTO.Auth.Response;
using TP6Programacion.Shared.DTO.Paginado.Request.Usuario;
using TP6Programacion.Shared.DTO.Usuario.Request;
using TP6Programacion.Shared.DTO.Usuario.Response;
using TP6Programacion.Shared.Enums;

namespace TP6Programacion.Backend.Services
{
    public class UsuarioService
    {
        public readonly IUsuarioRepository _repo;
        public readonly ITokenService _tokenService;

        public UsuarioService(IUsuarioRepository repo, ITokenService tokenService)
        {
            _repo = repo;
            _tokenService = tokenService;
        }

        public async Task<UsuarioActualResponse> ObtenerActual(int id)
        {
            var usuario = await _repo.BuscarPorId(id);

            if (usuario is null || usuario.Eliminado)
                throw new SesionInvalidaException();

            return new UsuarioActualResponse(usuario.Id, usuario.Username, usuario.Email, usuario.Rol);
        }

        public async Task CambiarRol(int id, Rol rol)
        {
            if (!Enum.IsDefined(rol))
                throw new DatosLlegaronErradosException("El rol indicado no es válido.");

            Usuario? usuario = await _repo.BuscarPorId(id);

            if (usuario is null)
                throw new RecursoNoExisteException("No existe ningún usuario con ese ID.");

            usuario.Rol = rol;
            await _repo.Actualizar(usuario);
        }
        public Task ConvertirEnAdministrador(int id) => CambiarRol(id, Rol.Administrador);
        public Task QuitarAdministrador(int id) => CambiarRol(id, Rol.Cajero);

        /* public async Task ConvertirEnAdministrador(int id)
         {
             if (Validaciones.EstanDatosBien(id) == false)
             {
                 throw new DatosLlegaronErradosException("El ID llegó errado.");
             }

                 Usuario? usuario = await _repo.BuscarPorId(id);

                 if (usuario == null)
                 {
                     throw new RecursoNoExisteException("No existe ningún usuario con ese ID.");
                 }

                 usuario.EsAdministrador = true;

                 await _repo.Actualizar(usuario);


         }

         public async Task QuitarAdministrador(int id)
         {
             if (Validaciones.EstanDatosBien(id) == false)
             {
                 throw new DatosLlegaronErradosException("El ID llegó errado.");
             }

                 Usuario? usuario = await _repo.BuscarPorId(id);

                 if (usuario == null)
                 {
                     throw new RecursoNoExisteException("No existe ningún usuario con ese ID.");
                 }

                 usuario.EsAdministrador = false;

                 await _repo.Actualizar(usuario);

         }*/

        public async Task<ActualizarUsuarioResponse> Actualizar(int id, ActualizarUsuarioRequest actualizarUsuarioRequest)
        {
            if (Validaciones.EstanDatosBien(id) == false)
            {
                throw new DatosLlegaronErradosException("El ID llegó errado.");
            }

            Usuario? usuario = await _repo.BuscarPorId(id);
            bool cambieAlgo = false;

            if (usuario == null)
            {
                throw new RecursoNoExisteException("No existe ningún proveedor con ese ID.");
            }

            if (actualizarUsuarioRequest.Username != usuario.Username && Validaciones.EstanDatosBien(actualizarUsuarioRequest.Username))
            {
                if (await _repo.ExistePorUsername(actualizarUsuarioRequest.Username!))
                {
                    throw new RecursoExistenteException("Ya existe alguien con ese usuario.");
                }
                else
                {
                    cambieAlgo = true;
                    usuario.Username = actualizarUsuarioRequest.Username!;
                }

            }

            if (actualizarUsuarioRequest.Email != usuario.Email && Validaciones.EstanDatosBien(actualizarUsuarioRequest.Email))
            {
                if (!Validaciones.EsUnEmailValido(actualizarUsuarioRequest.Email!))
                {
                    throw new DatosLlegaronErradosException("El formato del E-Mail es inválido.");
                }

                if (await _repo.ExistePorEmail(actualizarUsuarioRequest.Email!))
                {
                    throw new RecursoExistenteException("Ya existe un usuario con ese E-Mail.");
                }

                cambieAlgo = true;
                usuario.Email = actualizarUsuarioRequest.Email!;
            }

            if (Validaciones.EstanDatosBien(actualizarUsuarioRequest.Password) && !BCrypt.Net.BCrypt.EnhancedVerify(actualizarUsuarioRequest.Password, usuario.Password))
            {
                cambieAlgo = true;
                usuario.Password = BCrypt.Net.BCrypt.EnhancedHashPassword(actualizarUsuarioRequest.Password);
            }

            if (cambieAlgo == true)
            {
                await _repo.Actualizar(usuario);

                (string token, DateTime expiracion) = _tokenService.CrearToken(usuario);

                ActualizarUsuarioResponse actualizarUsuarioResponse = new ActualizarUsuarioResponse(token);

                return actualizarUsuarioResponse;
            }
            else
            {
                throw new DatosLlegaronErradosException("Los datos que mandó fueron inválidos");
            }

        }

        public async Task DarDeBaja(int id)
        {
            if (Validaciones.EstanDatosBien(id) == false)
            {
                throw new DatosLlegaronErradosException("El ID llegó errado.");
            }

            Usuario? usuario = await _repo.BuscarPorId(id);

            if (usuario == null)
            {
                throw new RecursoNoExisteException("No existe ningún usuario con ese ID.");
            }

            usuario.Eliminado = true;

            await _repo.DarDeBaja(usuario);


        }

        public async Task<PaginadoResponse<ObtenerUsuarioResponse>> ObtenerlosATodos(ParametroPaginacionUsuarioRequest parametros)
        {

            int numeroPagina = parametros.NumeroPagina is null || parametros.NumeroPagina < 1
                ? 1
                : parametros.NumeroPagina.Value;

            int tamanoPagina = parametros.TamanoPagina is null || parametros.TamanoPagina < 1
                ? 20
                : parametros.TamanoPagina > 50 ? 50 : parametros.TamanoPagina.Value;

            bool? eliminado = parametros.Eliminado;


            UsuarioQueryParametros usuarioQueryParametros = new UsuarioQueryParametros
            {
                NumeroPagina = numeroPagina,
                TamanoPagina = tamanoPagina,
                Eliminado = eliminado,
                Rol = parametros.Rol
            };

            PaginadoResponse<Usuario> resultado = await _repo.ObtenerTodos(usuarioQueryParametros);

            List<ObtenerUsuarioResponse> obtenerUsuarioResponse = resultado.Datos.Select(
                    u => new ObtenerUsuarioResponse(u.Id, u.Username, u.Email, u.Rol)).ToList();
           
                    
                
                return new PaginadoResponse<ObtenerUsuarioResponse>(
                    obtenerUsuarioResponse,
                    resultado.NumeroPagina,
                    resultado.TamanoPagina,
                    resultado.TotalRegistros
                    );

        }
    }
}
