using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using TP6Programacion.Mobile.Models;

namespace TP6Programacion.Mobile.Services;

public class CarritoService
{
    // Vamos a usar esto para que dos personas que se logean en el mismo celu no compartan carrito.
    private readonly AuthenticationStateProvider _estadoAutenticacion;
    
    public CarritoService(AuthenticationStateProvider estadoAutenticacion)
    {
        _estadoAutenticacion = estadoAutenticacion;
    }
    
    // Y este va a ser el método que se va a encargar de eso.
    private async Task<string> ObtenerClave()
    {
        var estado = await _estadoAutenticacion.GetAuthenticationStateAsync();
        
        string? id = estado.User.FindFirst("sub")?.Value;

        if (!int.TryParse(id, out int usuarioId) || usuarioId <= 0)
        {
            throw new InvalidOperationException("No se pudo identificar al usuario.");
        }

        return $"carrito_{usuarioId}";
    }
    
    public async Task<List<CarritoItem>> Obtener()
    {
        string clave = await ObtenerClave();
        
        string? jsonItemsCarrito = await SecureStorage.GetAsync(clave);

        if (string.IsNullOrWhiteSpace(jsonItemsCarrito))
        {
            return new List<CarritoItem>();
        }

        return JsonSerializer.Deserialize<List<CarritoItem>>(jsonItemsCarrito) ?? new List<CarritoItem>();
    }

    public async Task Agregar(int productoId, int cantidad = 1)
    {
        if (productoId <= 0 || cantidad <= 0)
        {
            throw new InvalidOperationException("El producto y/o la cantidad tienen que ser validas.");
        }

        List<CarritoItem> items = await Obtener();

        CarritoItem? item = items.FirstOrDefault(i => i.ProductoId == productoId);

        /* Acá, Sol, básicamente lo que hice es que si aún el cliente no tenía ese producto que lo agregue, pero si ya tenía algo con ese producto se sumen
        las cantidades. */
        
        if (item is null)
        {
            items.Add(new CarritoItem()
            {
                ProductoId = productoId,
                Cantidad = cantidad
            });
        }
        else
        {
            // Hace referencia al mismo item porque apunta al mismo objeto.
            item.Cantidad = item.Cantidad + cantidad;
        }
        
        await Guardar(items);
    }

    public async Task Vaciar()
    {
        string clave = await ObtenerClave();
        
        SecureStorage.Default.Remove(clave);
    }

    private async Task Guardar(List<CarritoItem> items)
    {
        string clave = await ObtenerClave();
        
        string jsonItemsCarrito = JsonSerializer.Serialize(items);

        await SecureStorage.Default.SetAsync(clave, jsonItemsCarrito);
    }
}