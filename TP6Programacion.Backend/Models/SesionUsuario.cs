namespace TP6Programacion.Backend.Models
{
    public class SesionUsuario
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime Expira { get; set; }
        public DateTime? Revocada { get; set; }
        public Usuario Usuario { get; set; } = null!;
    }
}