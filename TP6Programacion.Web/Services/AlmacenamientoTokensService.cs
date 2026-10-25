using Microsoft.JSInterop;
using System.Text.Json;
using TP6Programacion.Shared.DTO.Auth.Response;

namespace TP6Programacion.Web.Services;

public class AlmacenamientoTokensService
{
    private readonly IJSRuntime _jsRuntime;

    public AlmacenamientoTokensService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task Guardar(LoginResponse response)
    {
        string contenido = JsonSerializer.Serialize(response);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "sesion_usuario", contenido);
    }

    public async Task<LoginResponse?> Obtener()
    {
        string? contenido = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "sesion_usuario");

        if (string.IsNullOrWhiteSpace(contenido))
            return null;

        try
        {
            return JsonSerializer.Deserialize<LoginResponse>(contenido);
        }
        catch (JsonException)
        {
            await Limpiar();
            return null;
        }
    }

    public async Task Limpiar()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "sesion_usuario");
    }
}