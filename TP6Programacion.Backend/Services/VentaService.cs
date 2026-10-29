using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data.Common;
using TP6Programacion.Backend.Excepciones;
using TP6Programacion.Backend.Models;
using TP6Programacion.Backend.Models.ModeloAuxiliar;
using TP6Programacion.Backend.Models.ModeloAuxiliar.Query.Venta;
using TP6Programacion.Backend.Repositories;
using TP6Programacion.Shared.DTO.Paginado.Request.Venta;
using TP6Programacion.Shared.DTO.Venta.Request;
using TP6Programacion.Shared.DTO.Venta.Response;
using TP6Programacion.Shared.Enums;

namespace TP6Programacion.Backend.Services
{
    public class VentaService
    {
        private readonly IVentaRepository _repo;
        private readonly IProductoRepository _productoRepo;
        private readonly IClienteRepository _clienteRepo;

        public VentaService(IVentaRepository repo, IProductoRepository productoRepo, IClienteRepository clienteRepo)
        {
            _repo = repo;
            _productoRepo = productoRepo;
            _clienteRepo = clienteRepo;
        }
        public async Task<PaginadoResponse<VentaListadoResponse>> ObtenerTodas(
           ParametroPaginacionVentaRequest parametros, int? clienteIdForzado = null)
        {
            try
            {
                int numeroPagina = parametros.NumeroPagina is null || parametros.NumeroPagina < 1
                    ? 1
                    : parametros.NumeroPagina.Value;

                int tamanoPagina = parametros.TamanoPagina is null || parametros.TamanoPagina < 1
                    ? 20
                    : parametros.TamanoPagina > 50 ? 50 : parametros.TamanoPagina.Value;

                int? clienteId = clienteIdForzado ?? parametros.ClienteId;
                EstadoVenta? estado = parametros.Estado;

                string? direccion =
                    string.IsNullOrWhiteSpace(parametros.Direccion) ||
                    parametros.Direccion?.ToLower() is not ("asc" or "desc")
                        ? "desc"
                        : parametros.Direccion;

                string? buscar = parametros.Buscar?.Trim().ToLower();

                string? ordenarPor = string.IsNullOrWhiteSpace(parametros.OrdenarPor) ? "fecha" : parametros.OrdenarPor;

                VentaQueryParametros ventaQueryParametros = new VentaQueryParametros
                {
                    NumeroPagina = numeroPagina,
                    TamanoPagina = tamanoPagina,
                    ClienteId = clienteId,
                    Estado = estado,
                    Buscar = buscar,
                    OrdenarPor = ordenarPor,
                    Direccion = direccion
                };

                PaginadoResponse<Venta> resultado = await _repo.ObtenerTodas(ventaQueryParametros);

                List<VentaListadoResponse> ventas = resultado.Datos.Select(v => new VentaListadoResponse(
                    v.Id, v.Fecha, v.Total, v.ClienteId, v.Cliente?.Nombre, v.Estado
                )).ToList();

                return new PaginadoResponse<VentaListadoResponse>(
                    ventas,
                    resultado.NumeroPagina,
                    resultado.TamanoPagina,
                    resultado.TotalRegistros
                );
            }
            catch (DbException e)
            {
                throw new BaseDeDatosException($"Ocurrió un problema: {e.Message}");
            }
        }
        public async Task<PaginadoResponse<VentaListadoResponse>> ObtenerMisCompras(
            int usuarioId, ParametroPaginacionVentaRequest parametros)
        {
            Cliente? cliente = await _clienteRepo.ObtenerPorUsuarioId(usuarioId);

            if (cliente is null)
                return new PaginadoResponse<VentaListadoResponse>(new List<VentaListadoResponse>(), 1, 20, 0);

            return await ObtenerTodas(parametros, cliente.Id);
        }
        public async Task<VentaResponse> ObtenerMiCompra(int id, int usuarioId)
        {
            Cliente? cliente = await _clienteRepo.ObtenerPorUsuarioId(usuarioId);
            Venta? venta = await _repo.ObtenerPorId(id);

            if (venta is null || cliente is null || venta.ClienteId != cliente.Id)
                throw new RecursoNoExisteException("No existe ninguna compra con ese id.");

            return MapearADto(venta);
        }
        public async Task<VentaResponse> ObtenerPorId(int id)
        {
            try
            {
                Venta? venta = await _repo.ObtenerPorId(id);

                if (venta is null)
                {
                    throw new RecursoNoExisteException("No existe ninguna venta con ese id.");
                }

                return MapearADto(venta);
            }
            catch (DbException e)
            {
                throw new BaseDeDatosException($"Ocurrió un problema: {e.Message}");
            }
        }
        [HttpPost("presencial")]
        [Authorize(Roles = "Administrador,Cajero")]
        public async Task<VentaResponse> CrearVentaPresencial(CrearVentaPresencialRequest dto,int usuarioId)
        {
            Cliente? cliente = null;

            if (dto.ClienteId.HasValue)
            {
                cliente = await _clienteRepo.ObtenerPorId(dto.ClienteId.Value);

                if (cliente is null)
                {
                    throw new RecursoNoExisteException("No existe ningún cliente con ese id.");
                }
            }

            var (detalles, total) = await PrepararDetallesYDescontarStock(dto.Items);

            Venta venta = new()
            {
                Fecha = DateTime.UtcNow,
                Total = total,
                ClienteId = cliente?.Id,
                UsuarioId = usuarioId,
                DetallesVentas = detalles,
                Estado = EstadoVenta.Confirmada,
                FechaExpiracion = null
            };
            venta.Pagos.Add(new Pago
            {
                Metodo = dto.Metodo,
                Monto = total,
                Estado = EstadoPago.Aprobado,
                Fecha = DateTime.UtcNow,
                ReferenciaPago = dto.ReferenciaPago
            });

            await _repo.Crear(venta);

            Venta? ventaCompleta = await _repo.ObtenerPorId(venta.Id);

            return MapearADto(ventaCompleta!);
        }

        [HttpPost("online")]
        [Authorize(Roles = "Cliente")]
        public async Task<VentaResponse> CrearCompraOnline(CrearCompraOnlineRequest dto,int usuarioId)
        {
            Cliente? cliente =
                await _clienteRepo.ObtenerPorUsuarioId(usuarioId);

            if (cliente is null)
            {
                throw new RecursoNoExisteException(
                    "El usuario autenticado no tiene un cliente asociado.");
            }

            var (detalles, total) =
                await PrepararDetallesYDescontarStock(dto.Items);

            Venta venta = new()
            {
                Fecha = DateTime.UtcNow,
                FechaExpiracion = DateTime.UtcNow.AddMinutes(15),
                Total = total,
                ClienteId = cliente.Id,
                UsuarioId = null,
                DetallesVentas = detalles,
                Estado = EstadoVenta.Pendiente
            };

            await _repo.Crear(venta);

            Venta? ventaCompleta = await _repo.ObtenerPorId(venta.Id);

            return MapearADto(ventaCompleta!);
        }
        private static VentaResponse MapearADto(Venta venta)
        {
            List<VentaDetalleResponse> detalles = venta.DetallesVentas.Select(d => new VentaDetalleResponse(
                d.ProductoId,
                d.Producto.Nombre,
                d.Cantidad,
                d.PrecioUnitario,
                d.PrecioUnitario * d.Cantidad
            )).ToList();

            return new VentaResponse(
                venta.Id,
                venta.Fecha,
                venta.Total,
                venta.ClienteId,
                venta.Cliente?.Nombre,
                venta.UsuarioId,
                venta.Usuario?.Username,
                venta.Estado,
                detalles);
        }

        public async Task Anular(int id)
        {
            Venta? venta = await _repo.ObtenerParaAnular(id);

            if (venta is null)
            {
                throw new RecursoNoExisteException(
                    "No existe ninguna venta con ese id.");
            }

            if (venta.Estado == EstadoVenta.Anulada)
            {
                throw new DatosLlegaronErradosException(
                    "La venta ya se encuentra anulada.");
            }

            if (venta.Estado != EstadoVenta.Confirmada)
            {
                throw new DatosLlegaronErradosException(
                    "Solo se pueden anular ventas confirmadas.");
            }

            foreach (DetalleVenta detalle in venta.DetallesVentas)
            {
                detalle.Producto.Stock += detalle.Cantidad;
            }

            foreach (Pago pago in venta.Pagos)
            {
                if (pago.Estado == EstadoPago.Aprobado)
                {
                    pago.Estado = EstadoPago.Reembolsado;
                }
            }

            venta.Estado = EstadoVenta.Anulada;
            venta.FechaExpiracion = null;

            await _repo.GuardarCambios();
        }
        private async Task<(List<DetalleVenta> Detalles, decimal Total)>PrepararDetallesYDescontarStock(
                List<CrearVentaItemRequest> items)
        {
            if (items is null || items.Count == 0)
            {
                throw new DatosLlegaronErradosException(
                    "La venta debe tener al menos un producto.");
            }

            if (items.Any(i => i.Cantidad <= 0))
            {
                throw new DatosLlegaronErradosException(
                    "La cantidad de cada producto debe ser mayor a cero.");
            }

            var itemsAgrupados = items
                .GroupBy(i => i.ProductoId)
                .Select(g => new
                {
                    ProductoId = g.Key,
                    Cantidad = g.Sum(i => i.Cantidad)
                })
                .ToList();

            List<DetalleVenta> detalles = new();
            decimal total = 0;

            foreach (var item in itemsAgrupados)
            {
                Producto? producto =
                    await _productoRepo.ObtenerPorId(item.ProductoId);

                if (producto is null || producto.Eliminado)
                {
                    throw new RecursoNoExisteException(
                        $"No existe un producto disponible con id {item.ProductoId}."); //mejor cambiarlo a producto nombre
                }

                if (producto.Stock < item.Cantidad)
                {
                    throw new DatosLlegaronErradosException(
                        $"Stock insuficiente para '{producto.Nombre}'. " +
                        $"Disponible: {producto.Stock}, solicitado: {item.Cantidad}.");
                }

                decimal precioUnitario = producto.PrecioVenta;

                producto.Stock -= item.Cantidad;

                total += precioUnitario * item.Cantidad;

                detalles.Add(new DetalleVenta
                {
                    ProductoId = producto.Id,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = precioUnitario
                });
            }

            return (detalles, total);
        }
        internal async Task AprobarPago(int ventaId, int pagoId)
        {
            Venta? venta = await _repo.ObtenerParaProcesarPago(ventaId);

            if (venta is null)
            {
                throw new RecursoNoExisteException(
                    "No existe ninguna venta con ese id.");
            }

            if (venta.Estado != EstadoVenta.Pendiente)
            {
                throw new DatosLlegaronErradosException(
                    "Solo se pueden aprobar pagos de ventas pendientes.");
            }

            if (venta.FechaExpiracion is null ||
                venta.FechaExpiracion <= DateTime.UtcNow)
            {
                throw new DatosLlegaronErradosException(
                    "La reserva venció y no puede confirmarse.");
            }

            Pago? pago = venta.Pagos.FirstOrDefault(p => p.Id == pagoId);

            if (pago is null)
            {
                throw new RecursoNoExisteException(
                    "El pago no pertenece a esta venta o no existe.");
            }

            if (pago.Estado != EstadoPago.Pendiente)
            {
                throw new DatosLlegaronErradosException(
                    "El pago ya fue procesado.");
            }

            if (pago.Monto != venta.Total)
            {
                throw new DatosLlegaronErradosException(
                    "El monto del pago no coincide con el total de la venta.");
            }

            pago.Estado = EstadoPago.Aprobado;

            venta.Estado = EstadoVenta.Confirmada;
            venta.FechaExpiracion = null;

            await _repo.GuardarCambios();
        }

        internal async Task RechazarPago(int ventaId, int pagoId)
        {
            Venta? venta = await _repo.ObtenerParaProcesarPago(ventaId);

            if (venta is null)
            {
                throw new RecursoNoExisteException(
                    "No existe ninguna venta con ese id.");
            }

            Pago? pago = venta.Pagos.FirstOrDefault(p => p.Id == pagoId);

            if (pago is null)
            {
                throw new RecursoNoExisteException(
                    "El pago no pertenece a esta venta o no existe.");
            }

            await CancelarReservaPendiente(venta, pago);
        }
        private async Task CancelarReservaPendiente(Venta venta, Pago? pago)
        {
            if (venta.Estado != EstadoVenta.Pendiente)
                throw new DatosLlegaronErradosException("La venta ya no está pendiente.");

            if (pago is not null && pago.Estado != EstadoPago.Pendiente)
                throw new DatosLlegaronErradosException("El pago ya fue procesado.");

            foreach (DetalleVenta detalle in venta.DetallesVentas)
                detalle.Producto.Stock += detalle.Cantidad;

            if (pago is not null)
                pago.Estado = EstadoPago.Rechazado;

            venta.Estado = EstadoVenta.Cancelada;
            venta.FechaExpiracion = null;

            await _repo.GuardarCambios();
        }
        internal async Task VencerReserva(int ventaId)
        {
            Venta? venta = await _repo.ObtenerParaProcesarPago(ventaId);

            if (venta is null)
            {
                throw new RecursoNoExisteException(
                    "No existe ninguna venta con ese id.");
            }

            if (venta.Estado != EstadoVenta.Pendiente)
            {
                return;
            }

            if (venta.FechaExpiracion is null ||
                venta.FechaExpiracion > DateTime.UtcNow)
            {
                return;
            }

            Pago? pago = venta.Pagos
                .FirstOrDefault(p => p.Estado == EstadoPago.Pendiente);

            await CancelarReservaPendiente(venta, pago);
        }
    }
}
