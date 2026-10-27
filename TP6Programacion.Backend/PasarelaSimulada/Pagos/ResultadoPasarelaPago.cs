using TP6Programacion.Shared.Enums;

namespace TP6Programacion.Backend.PasarelaSimulada.Pagos
{
    public class ResultadoPasarelaPago
    {
        public required string ReferenciaPago { get; set; }

        public EstadoPago Estado { get; set; }

        public required string Mensaje { get; set; }
    }
}
