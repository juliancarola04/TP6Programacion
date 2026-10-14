using TP6Programacion.Backend.Models;

namespace TP6Programacion.Backend.Repositories;

public interface IImagenRepository
{
    Task<Imagen?> ObtenerPorProductoId(int productoId);
    Task Crear(Imagen imagen);
    Task Eliminar(Imagen imagen);
}
