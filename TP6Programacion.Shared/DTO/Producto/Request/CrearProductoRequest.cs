namespace TP6Programacion.Shared.DTO.Producto.Request;

public record CrearProductoRequest(
    string Nombre,
    decimal PrecioCompra,
    decimal PrecioVenta,
    int Stock,
    int CategoriaId);