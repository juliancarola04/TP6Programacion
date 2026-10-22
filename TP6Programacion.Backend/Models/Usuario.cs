using TP6Programacion.Shared.Enums;

namespace TP6Programacion.Backend.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
        public required string Email { get; set; }
        public Rol Rol { get; set; } = Rol.Cliente;
        public bool Eliminado { get; set; } = false;
        public ICollection<Venta>? Ventas { get; set; }
        public ICollection<Ingreso>? Ingresos { get; set; }
        public ICollection<Auditoria> Auditorias { get; set; } = new List<Auditoria>();
        public Cliente? Cliente { get; set; }
    }
}
