using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TP6Programacion.Backend.Excepciones;
using TP6Programacion.Backend.Models.ModeloAuxiliar;
using TP6Programacion.Backend.Services;
using TP6Programacion.Shared.DTO.Ingreso.Request;
using TP6Programacion.Shared.DTO.Ingreso.Response;
using TP6Programacion.Shared.DTO.Paginado.Request.Ingreso;
using TP6Programacion.Shared.DTO.Paginado.Response;

namespace TP6Programacion.Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class IngresosController : ControllerBase
{
    private readonly IngresoService _ingresoService;

    public IngresosController(IngresoService ingresoService)
    {
        _ingresoService = ingresoService;
    }

    [HttpGet]
    public async Task<ActionResult<PaginadoResponseDto<IngresoListadoResponse>>> ObtenerTodos(
        [FromQuery] ParametroPaginacionIngresoRequest parametros)
    {
        try
        {
            PaginadoResponse<IngresoListadoResponse> ingresos = await _ingresoService.ObtenerTodos(parametros);

            PaginadoResponseDto<IngresoListadoResponse> paginadoResponseDto = new PaginadoResponseDto<IngresoListadoResponse>(ingresos.NumeroPagina,
                ingresos.TamanoPagina, ingresos.TotalRegistros, ingresos.TotalPaginas, ingresos.TienePaginaAnterior,
                ingresos.TienePaginaPosterior, ingresos.Datos);

            return Ok(paginadoResponseDto);
        }
        catch (BaseDeDatosException e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<IngresoResponse>> ObtenerPorId(int id)
    {
        try
        {
            return Ok(await _ingresoService.ObtenerPorId(id));
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

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<IngresoResponse>> Crear(CrearIngresoRequest dto)
    {
        string? usuarioIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (usuarioIdClaim is null || !int.TryParse(usuarioIdClaim, out int usuarioId))
        {
            return Unauthorized("No se pudo identificar al usuario a partir del token.");
        }

        try
        {
            IngresoResponse ingreso = await _ingresoService.Crear(dto, usuarioId);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = ingreso.Id }, ingreso);
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

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Anular(int id)
    {
        try
        {
            await _ingresoService.Anular(id);
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
}
