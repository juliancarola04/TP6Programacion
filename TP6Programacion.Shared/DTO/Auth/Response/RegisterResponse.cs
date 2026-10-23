namespace TP6Programacion.Shared.DTO.Auth.Response;

public record RegisterResponse(string? Token, string? RefreshToken, DateTime SesionExpira);
