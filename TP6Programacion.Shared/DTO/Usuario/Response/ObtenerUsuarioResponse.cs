namespace TP6Programacion.Shared.DTO.Usuario.Response;

public record ObtenerUsuarioResponse(
    int Id,
    string Username,
    string Email,
    bool EsAdministrador);