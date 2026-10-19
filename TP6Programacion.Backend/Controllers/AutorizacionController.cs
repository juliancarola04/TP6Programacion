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

        public AutorizacionController(LoginService loginService, RegisterService registerService, SesionService sesionService)
        {
            _loginService = loginService;
            _registerService = registerService;
            _sesionService = sesionService;
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
                    throw new DatosLlegaronErradosException("El refresh token está vacío.");

                var respuesta = await _sesionService.Renovar(refreshToken);

                GuardarCookies(respuesta);

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
