using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TP6Programacion.Backend.Excepciones;
using TP6Programacion.Backend.Models.ModeloAuxiliar;
using TP6Programacion.Backend.Services;
using TP6Programacion.Shared.DTO.Paginado.Request.Venta;
using TP6Programacion.Shared.DTO.Paginado.Response;
using TP6Programacion.Shared.DTO.Venta.Request;
using TP6Programacion.Shared.DTO.Venta.Response;

namespace TP6Programacion.Backend.Controllers;
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class VentasController : ControllerBase
{
    private readonly VentaService _ventaService;
    public VentasController(VentaService ventaService)
    {
        _ventaService = ventaService;
    }
    [HttpGet]
    [Authorize(Roles = "Administrador,Cajero")]

    public async Task<ActionResult<PaginadoResponseDto<VentaListadoResponse>>> ObtenerTodas(
        [FromQuery] ParametroPaginacionVentaRequest parametros)
    {
        try
        {
            PaginadoResponse<VentaListadoResponse> ventas = await _ventaService.ObtenerTodas(parametros);

            PaginadoResponseDto<VentaListadoResponse> paginadoResponseDto = new PaginadoResponseDto<VentaListadoResponse>(ventas.NumeroPagina,
                ventas.TamanoPagina, ventas.TotalRegistros, ventas.TotalPaginas, ventas.TienePaginaAnterior,
                ventas.TienePaginaPosterior, ventas.Datos);

            return Ok(paginadoResponseDto);
        }
        catch (BaseDeDatosException e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
        }
    }
    [HttpGet("{id}")]
    [Authorize(Roles = "Administrador,Cajero")]
    public async Task<ActionResult<VentaResponse>> ObtenerPorId(int id)
    {
        try
        {
            return Ok(await _ventaService.ObtenerPorId(id));
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
    /*[HttpPost]
    [Authorize(Roles = "Administrador,Cajero")]
    public async Task<ActionResult<VentaResponse>> Crear(CrearVentaRequest dto)
    {
        string? usuarioIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (usuarioIdClaim is null || !int.TryParse(usuarioIdClaim, out int usuarioId))
        {
            return Unauthorized("No se pudo identificar al usuario a partir del token.");
        }

        try
        {
            VentaResponse venta = await _ventaService.Crear(dto, usuarioId);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = venta.Id }, venta);
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
    }*/
    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Anular(int id)
    {
        try
        {
            await _ventaService.Anular(id);
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


