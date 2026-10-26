using TP6Programacion.Shared.Enums;
namespace TP6Programacion.Backend.PasarelaSimulada.Pagos
{
    public class PasarelaPagoSimulada : IPasarelaPago
    {
        public Task<ResultadoPasarelaPago> ProcesarPagoAsync(
                int ventaId,
                decimal monto,
                MetodoPago metodo)
        {
            // Simulación inicial: todos los pagos se aprueban.
            var resultado = new ResultadoPasarelaPago
            {
                ReferenciaPago = $"SIM-{Guid.NewGuid():N}",
                Estado = "Aprobado",
                Mensaje = "Pago aprobado por la pasarela simulada."
            };

            return Task.FromResult(resultado);
        }
    }
}
