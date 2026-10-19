namespace TP6Programacion.Shared.DTO.Auth.Response;

public record UsuarioActualResponse(
    int Id,
    string Username,
    string Email,
    bool EsAdministrador);