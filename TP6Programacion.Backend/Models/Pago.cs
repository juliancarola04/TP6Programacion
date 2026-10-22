using TP6Programacion.Shared.Enums;

namespace TP6Programacion.Backend.Models
{
    public class Pago
    {
        public int Id { get; set; }

        public int VentaId { get; set; }
        public Venta Venta { get; set; } = null!;

        public MetodoPago Metodo { get; set; }
        public decimal Monto { get; set; }

        public EstadoPago Estado { get; set; } = EstadoPago.Pendiente;

        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        public string? ReferenciaPago { get; set; }
    }
}
