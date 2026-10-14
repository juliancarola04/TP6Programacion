namespace TP6Programacion.Shared.DTO.Auth.Request;

public record LoginRequest(
    string Username,
    string Password);