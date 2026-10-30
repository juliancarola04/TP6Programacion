using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using System.Security.Claims;
using TP6Programacion.Backend.Excepciones;
using TP6Programacion.Backend.GeneracionPDFs;
using TP6Programacion.Backend.Models.ModeloAuxiliar;
using TP6Programacion.Backend.Services;
using TP6Programacion.Backend.Utilidades;
using TP6Programacion.Shared.DTO.Paginado.Request.Venta;
using TP6Programacion.Shared.DTO.Paginado.Response;
using TP6Programacion.Shared.DTO.PDFs.Response;
using TP6Programacion.Shared.DTO.Venta.Request;
using TP6Programacion.Shared.DTO.Venta.Response;

namespace TP6Programacion.Backend.Controllers;
[Route("api/[controller]")]
[ApiController]
public class VentasController : ControllerBase
{
    private readonly VentaService _ventaService;
    private readonly PagoService _pagoService;
    public VentasController(VentaService ventaService, PagoService pagoService)
    {
        _ventaService = ventaService;
        _pagoService = pagoService;
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
    [HttpPost("presencial")]
    [Authorize(Roles = "Administrador,Cajero")]
    public async Task<ActionResult<VentaResponse>> CrearPresencial(CrearVentaPresencialRequest dto)
    {
        int? usuarioId = User.ObtenerUsuarioId();
        if (usuarioId is null) return Unauthorized("No se pudo identificar al usuario a partir del token.");

        try
        {
            VentaResponse venta = await _ventaService.CrearVentaPresencial(dto, usuarioId.Value);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = venta.Id }, venta);
        }
        catch (DatosLlegaronErradosException e) { return BadRequest(e.Message); }
        catch (RecursoNoExisteException e) { return NotFound(e.Message); }
        catch (BaseDeDatosException e) { return StatusCode(StatusCodes.Status500InternalServerError, e.Message); }
    }

    [HttpPost("online")]
    [Authorize(Roles = "Cliente")]
    public async Task<ActionResult<VentaResponse>> CrearCompraOnline(CrearCompraOnlineRequest dto)
    {
        int? usuarioId = User.ObtenerUsuarioId();
        if (usuarioId is null) return Unauthorized("No se pudo identificar al usuario a partir del token.");

        try
        {
            VentaResponse venta = await _ventaService.CrearCompraOnline(dto, usuarioId.Value);
            return CreatedAtAction(nameof(ObtenerMiCompra), new { id = venta.Id }, venta);
        }
        catch (DatosLlegaronErradosException e) { return BadRequest(e.Message); }
        catch (RecursoNoExisteException e) { return NotFound(e.Message); }
        catch (BaseDeDatosException e) { return StatusCode(StatusCodes.Status500InternalServerError, e.Message); }
    }
    [HttpPost("{id:int}/pago")]
    [Authorize(Roles = "Cliente")]
    public async Task<ActionResult<VentaResponse>> Pagar(int id, PagarVentaRequest dto)
    {
        int? usuarioId = User.ObtenerUsuarioId();
        if (usuarioId is null) return Unauthorized("No se pudo identificar al usuario a partir del token.");

        try
        {
            await _pagoService.ProcesarPago(id, usuarioId.Value, dto.Metodo);
            return Ok(await _ventaService.ObtenerMiCompra(id, usuarioId.Value)); // la app ve el estado final
        }
        catch (DatosLlegaronErradosException e) { return BadRequest(e.Message); }
        catch (RecursoNoExisteException e) { return NotFound(e.Message); }
        catch (BaseDeDatosException e) { return StatusCode(StatusCodes.Status500InternalServerError, e.Message); }
    }

    [HttpGet("mis-compras")]
    [Authorize(Roles = "Cliente")]
    public async Task<ActionResult<PaginadoResponseDto<VentaListadoResponse>>> ObtenerMisCompras(
        [FromQuery] ParametroPaginacionVentaRequest parametros)
    {
        int? usuarioId = User.ObtenerUsuarioId();
        if (usuarioId is null) return Unauthorized();

        PaginadoResponse<VentaListadoResponse> ventas =
            await _ventaService.ObtenerMisCompras(usuarioId.Value, parametros);

        return Ok(new PaginadoResponseDto<VentaListadoResponse>(ventas.NumeroPagina, ventas.TamanoPagina,
            ventas.TotalRegistros, ventas.TotalPaginas, ventas.TienePaginaAnterior,
            ventas.TienePaginaPosterior, ventas.Datos));
    }

    [HttpGet("mis-compras/{id:int}")]
    [Authorize(Roles = "Cliente")]
    public async Task<ActionResult<VentaResponse>> ObtenerMiCompra(int id)
    {
        int? usuarioId = User.ObtenerUsuarioId();
        if (usuarioId is null) return Unauthorized();

        try { return Ok(await _ventaService.ObtenerMiCompra(id, usuarioId.Value)); }
        catch (RecursoNoExisteException e) { return NotFound(e.Message); }
    }

    // Este va a ser el que vamos a usar para mostrar las compras del cliente. Fijate si querés cambiarle el rol y lo querés adaptar a algo tuyo :3
    [HttpGet("mis-compras/{id:int}/pdf")]
    [Authorize(Roles = "Cliente")]
    public async Task<IActionResult> MostrarTicket(int id)
    {
        string? idJWT = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(idJWT, out int usuarioId))
        {
            return Unauthorized();
        }

        try
        {
            ComprobanteVentaResponse comprobante = await _ventaService.ObtenerParaTicket(id, usuarioId);

            var documento = new ReporteVenta(comprobante);

            byte[] pdf = documento.GeneratePdf();

            return File(pdf, "application/pdf", $"Compra-{comprobante.Venta.Id}.pdf");
        }
        catch (RecursoNoExisteException e)
        {
            return NotFound(e.Message);
        }
    }
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


