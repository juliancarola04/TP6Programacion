using TP6Programacion.Backend.Excepciones;
using TP6Programacion.Backend.Models;
using TP6Programacion.Backend.PasarelaSimulada.Pagos;
using TP6Programacion.Backend.Repositories;
using TP6Programacion.Shared.Enums;

namespace TP6Programacion.Backend.Services
{
    public class PagoService
    {
        private readonly IPagoRepository _pagoRepo;
        private readonly IVentaRepository _ventaRepo;
        private readonly IPasarelaPago _pasarela;
        private readonly IClienteRepository _clienteRepo;
        private readonly VentaService _ventaService;

        public PagoService(
            IPagoRepository pagoRepo,
            IVentaRepository ventaRepo,
            IPasarelaPago pasarela,
            IClienteRepository clienteRepo,
            VentaService ventaService)
        {
            _pagoRepo = pagoRepo;
            _ventaRepo = ventaRepo;
            _pasarela = pasarela;
            _clienteRepo = clienteRepo;
            _ventaService = ventaService;
        }

        public async Task<ResultadoPasarelaPago> ProcesarPago(int ventaId,int usuarioId,MetodoPago metodo)
        {
            if (!Enum.IsDefined(metodo))
                throw new DatosLlegaronErradosException("El método de pago no es válido.");

            Venta? venta = await _ventaRepo.ObtenerParaProcesarPago(ventaId);

            Cliente? cliente = await _clienteRepo.ObtenerPorUsuarioId(usuarioId); //para verificar de quien es la venta

            if (venta is null || cliente is null || venta.ClienteId != cliente.Id)
            {
                throw new RecursoNoExisteException(
                    "No existe ninguna venta con ese id.");
            }

            if (venta.Estado != EstadoVenta.Pendiente)
            {
                throw new DatosLlegaronErradosException(
                    "Solo se pueden procesar pagos de ventas pendientes.");
            }

            if (venta.FechaExpiracion is null ||
                venta.FechaExpiracion <= DateTime.UtcNow)
            {
                throw new DatosLlegaronErradosException(
                    "La reserva venció. No se puede procesar el pago.");
            }

            if (venta.Pagos.Any(p => p.Estado == EstadoPago.Pendiente))
            {
                throw new DatosLlegaronErradosException(
                    "La venta ya tiene un pago pendiente de procesamiento.");
            }

            if (metodo == MetodoPago.Efectivo)
            {
                throw new DatosLlegaronErradosException(
                    "El pago online no admite el método Efectivo.");
            }

            Pago pago = new()
            {
                VentaId = venta.Id,
                Metodo = metodo,
                Monto = venta.Total,
                Estado = EstadoPago.Pendiente,
                Fecha = DateTime.UtcNow
            };

            await _pagoRepo.Crear(pago);

            ResultadoPasarelaPago resultado =
                await _pasarela.ProcesarPagoAsync(
                    venta.Id,
                    pago.Monto,
                    pago.Metodo);

            pago.ReferenciaPago = resultado.ReferenciaPago;

            await _pagoRepo.GuardarCambios();

            if (resultado.Estado == EstadoPago.Aprobado)
            {
                await _ventaService.AprobarPago(venta.Id, pago.Id);
            }
            else if (resultado.Estado == EstadoPago.Rechazado)
            {
                await _ventaService.RechazarPago(venta.Id, pago.Id);
            }
            else
            {
                throw new DatosLlegaronErradosException(
                    "La pasarela devolvió un estado de pago desconocido.");
            }

            return resultado;
        }
    }
}
