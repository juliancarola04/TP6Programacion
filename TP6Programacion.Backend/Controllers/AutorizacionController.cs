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

        public AutorizacionController(LoginService loginService, RegisterService registerService)
        {
            _loginService = loginService;
            _registerService = registerService;
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
    }
}
