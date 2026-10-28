using TP6Programacion.Shared.Auth;
using TP6Programacion.Backend.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TP6Programacion.Backend.Excepciones;
using TP6Programacion.Backend.Services;
using TP6Programacion.Shared.DTO.Auth.Request;
using TP6Programacion.Shared.DTO.Auth.Response;

namespace TP6Programacion.Backend.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AutorizacionController : ControllerBase
    {
        private readonly LoginService _loginService;
        private readonly RegisterService _registerService;
        private readonly SesionService _sesionService;
        private readonly UsuarioService _usuarioService;

        public AutorizacionController(LoginService loginService, RegisterService registerService, SesionService sesionService, UsuarioService usuarioService)
        {
            _loginService = loginService;
            _registerService = registerService;
            _sesionService = sesionService;
            _usuarioService = usuarioService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest loginRequest)
        {
            try
            {
                LoginResponse? loginDtoOutput = await _loginService.Login(loginRequest);
                return Ok(loginDtoOutput);
            }
            catch (DatosLlegaronErradosException e)
            {
                return BadRequest(e.Message);
            }
            catch (RecursoNoExisteException e)
            {
                return NotFound(e.Message);
            }
            catch (BaseDeDatosException e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponse>> Renovar(RefreshRequest request)
        {
            try
            {
                var respuesta = await _sesionService.Renovar(request.RefreshToken);
                return Ok(respuesta);
            }
            catch (DatosLlegaronErradosException e)
            {
                return BadRequest(e.Message);
            }
            catch (SesionInvalidaException e)
            {
                return Unauthorized(e.Message);
            }
            catch (BaseDeDatosException e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpPost("logout")]
        [AllowAnonymous]
        public async Task<IActionResult> CerrarSesion(LogoutRequest request)
        {
            try
            {
                await _sesionService.Cerrar(request.RefreshToken);
                return NoContent();
            }
            catch (DatosLlegaronErradosException e)
            {
                return BadRequest(e.Message);
            }
            catch (BaseDeDatosException e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<RegisterResponse>> Register(RegisterRequest registerRequest)
        {
            try
            {
                RegisterResponse? registerResponse = await _registerService.Registrarse(registerRequest);
                return Ok(registerResponse);
            }
            catch (DatosLlegaronErradosException e)
            {
                return BadRequest(e.Message);
            }
            catch (RecursoExistenteException e)
            {
                return Conflict(e.Message);
            }
            catch (BaseDeDatosException e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }
        [HttpPost("empleados")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> CrearEmpleado(CrearEmpleadoRequest request)
        {
            try
            {
                await _registerService.CrearEmpleado(request);
                return NoContent();
            }
            catch (DatosLlegaronErradosException e) { return BadRequest(e.Message); }
            catch (RecursoExistenteException e) { return Conflict(e.Message); }
            catch (BaseDeDatosException e) { return StatusCode(StatusCodes.Status500InternalServerError, e.Message); }
        }
    }
}
