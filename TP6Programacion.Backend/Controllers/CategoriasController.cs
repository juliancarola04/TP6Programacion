using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TP6Programacion.Backend.Excepciones;
using TP6Programacion.Backend.Models.ModeloAuxiliar;
using TP6Programacion.Backend.Services;
using TP6Programacion.Shared.DTO.Categoria.Request;
using TP6Programacion.Shared.DTO.Categoria.Response;
using TP6Programacion.Shared.DTO.Paginado.Request.Categoria;
using TP6Programacion.Shared.DTO.Paginado.Response;

namespace TP6Programacion.Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CategoriasController : ControllerBase
{
    private readonly CategoriaService _categoriaService;

    public CategoriasController(CategoriaService categoriaService)
    {
        _categoriaService = categoriaService;
    }

    [HttpGet]
    public async Task<ActionResult<PaginadoResponseDto<CategoriaResponse>>> ObtenerTodas(
        [FromQuery] ParametroPaginacionCategoriaRequest parametros)
    {
        try
        {
            PaginadoResponse<CategoriaResponse> categorias = await _categoriaService.ObtenerTodas(parametros);

            PaginadoResponseDto<CategoriaResponse> paginadoResponseDto = new PaginadoResponseDto<CategoriaResponse>(categorias.NumeroPagina,
                categorias.TamanoPagina, categorias.TotalRegistros, categorias.TotalPaginas, categorias.TienePaginaAnterior,
                categorias.TienePaginaPosterior, categorias.Datos);

            return Ok(paginadoResponseDto);
        }
        catch (BaseDeDatosException e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CategoriaResponse>> ObtenerPorId(int id)
    {
        try
        {
            return Ok(await _categoriaService.ObtenerPorId(id));
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
    public async Task<ActionResult<CategoriaResponse>> Crear(CrearCategoriaRequest dto)
    {
        try
        {
            CategoriaResponse categoria = await _categoriaService.Crear(dto);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = categoria.Id }, categoria);
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

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Actualizar(int id, ActualizarCategoriaRequest dto)
    {
        try
        {
            await _categoriaService.Actualizar(id, dto);
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
        catch (RecursoExistenteException e)
        {
            return Conflict(e.Message);
        }
        catch (BaseDeDatosException e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            await _categoriaService.Eliminar(id);
            return NoContent();
        }
        catch (RecursoNoExisteException e)
        {
            return NotFound(e.Message);
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
