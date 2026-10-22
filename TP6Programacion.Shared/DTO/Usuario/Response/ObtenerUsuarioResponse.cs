using TP6Programacion.Shared.Enums;

namespace TP6Programacion.Shared.DTO.Usuario.Response;

public record ObtenerUsuarioResponse(
    int Id,
    string Username,
    string Email,
    Rol Rol);