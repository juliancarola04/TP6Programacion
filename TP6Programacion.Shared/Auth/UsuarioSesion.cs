using TP6Programacion.Shared.Enums;

namespace TP6Programacion.Shared.Auth;

public record UsuarioSesion(int Id, string Username, string Email, Rol Rol);