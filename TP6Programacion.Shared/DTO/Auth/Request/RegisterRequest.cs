namespace TP6Programacion.Shared.DTO.Auth.Request;

public record RegisterRequest(
    string Username,
    string Password,
    string Email,
    string Nombre,
    string Dni,
    string Telefono,
    string Direccion,
    bool EsWeb = false);