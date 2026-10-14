namespace TP6Programacion.Shared.DTO.Usuario.Request;

public record ActualizarUsuarioRequest(
    string? Username,
    string? Password,
    string? Email);