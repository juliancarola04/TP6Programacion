using TP6Programacion.Backend.Models;
using TP6Programacion.Shared.Enums;
namespace TP6Programacion.Backend.PasarelaSimulada.Pagos
{
    public interface IPasarelaPago
    {
        Task<ResultadoPasarelaPago> ProcesarPagoAsync(
            int ventaId,
            decimal monto,
            MetodoPago metodo);
    }
}
