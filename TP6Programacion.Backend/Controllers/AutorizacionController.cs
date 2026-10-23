using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
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

        // Mirá Program.cs en la parte de Forgery para entender lo que estoy haciendo acá.
        [HttpGet("web/csrf")]
        [AllowAnonymous]
        public IActionResult ObtenerTokenCsrf([FromServices] IAntiforgery antiforgery)
        {
            var tokens = antiforgery.GetAndStoreTokens(HttpContext);

            return Ok(new { token = tokens.RequestToken });
        }

        [HttpPost("web/login")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWeb(LoginRequest loginRequest)
        {
            try
            {
                var respuesta = await _loginService.Login(loginRequest);

                if (respuesta is null)
                    return Unauthorized();

                GuardarCookies(respuesta);

                return NoContent();
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

        [HttpPost("web/refresh")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RenovarWeb()
        {
            try
            {
                var refreshToken = Request.Cookies["refresh_token"];

                if (string.IsNullOrWhiteSpace(refreshToken))
                    throw new SesionInvalidaException();

                var respuesta = await _sesionService.Renovar(refreshToken);

                GuardarCookies(respuesta);

                return NoContent();
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

        [HttpPost("web/logout")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CerrarSesionWeb()
        {
            try
            {
                var refreshToken = Request.Cookies["refresh_token"];

                if (!string.IsNullOrWhiteSpace(refreshToken))
                    await _sesionService.Cerrar(refreshToken);

                EliminarCookies();
                return NoContent();
            }
            catch (BaseDeDatosException e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpGet("usuario-actual")]
        [Authorize]
        public async Task<ActionResult<UsuarioActualResponse>> ObtenerUsuarioActual()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(id, out var usuarioId))
                return Unauthorized();

            try
            {
                return Ok(await _usuarioService.ObtenerActual(usuarioId));
            }
            catch (SesionInvalidaException e)
            {
                return Unauthorized(e.Message);
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
        [HttpPost("web/register")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterWeb(RegisterRequest request)
        {
            try
            {
                var respuesta = await _registerService.RegistrarseCajero(request);
                GuardarCookies(new LoginResponse(respuesta.Token, respuesta.RefreshToken, respuesta.SesionExpira));
                return NoContent();
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

        private void EliminarCookies()
        {
            Response.Cookies.Delete("access_token", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = "/"
            });

            Response.Cookies.Delete("refresh_token", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = "/api/auth"
            });
        }

        private void GuardarCookies(LoginResponse respuesta)
        {
            var jwt = new JsonWebTokenHandler().ReadJsonWebToken(respuesta.Token!);

            Response.Cookies.Append("access_token", respuesta.Token!, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                Expires = new DateTimeOffset(jwt.ValidTo)
            });

            Response.Cookies.Append("refresh_token", respuesta.RefreshToken!, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = "/api/auth",
                Expires = new DateTimeOffset(respuesta.SesionExpira)
            });
        }
    }
}
