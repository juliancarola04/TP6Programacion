using Microsoft.Maui.Storage;
using System.Text.Json;
using TP6Programacion.Shared.DTO.Auth.Response;

namespace TP6Programacion.Mobile.Services;

public class AlmacenamientoTokensService
{
    public async Task Guardar(LoginResponse response)
    {
        string contenido = JsonSerializer.Serialize(response);

        await SecureStorage.Default.SetAsync("sesion_usuario", contenido);
    }

    public async Task<LoginResponse?> Obtener()
    {
        string? contenido = await SecureStorage.Default.GetAsync("sesion_usuario");

        if (string.IsNullOrWhiteSpace(contenido))
        {
            return null;
        }

        return JsonSerializer.Deserialize<LoginResponse>(contenido);
    }

    public void Limpiar()
    {
        SecureStorage.Default.Remove("sesion_usuario");
    }
}
