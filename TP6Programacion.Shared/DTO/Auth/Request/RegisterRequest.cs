namespace TP6Programacion.Shared.DTO.Auth.Request;

public record RegisterRequest(
    string Username,
    string Password,
    string Email,
    bool EsWeb = false);