namespace TP6Programacion.Backend.Models
{
    public class Auditoria
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        // Puede ser nulo si no se identifica al usuario
        public int? UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        public required string Accion { get; set; }
        public required string Entidad { get; set; }

        public int EntidadId { get; set; }

        public string? Detalle { get; set; }
        public string? DireccionIp { get; set; }
    }
}
