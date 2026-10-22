using TP6Programacion.Shared.Enums;

namespace TP6Programacion.Backend.Models
{
    public class Venta
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public DateTime? FechaExpiracion { get; set; }
        public decimal Total { get; set; }
        public EstadoVenta Estado { get; set; } = EstadoVenta.Pendiente;
        public int? ClienteId { get; set; }
        public Cliente? Cliente { get; set; }
        public int? UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }
        public ICollection<DetalleVenta> DetallesVentas { get; set; } = new List<DetalleVenta>();
        public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
    }
}
