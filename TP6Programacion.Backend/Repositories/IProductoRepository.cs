using TP6Programacion.Backend.Models.ModeloAuxiliar.Query.Producto;
using TP6Programacion.Backend.Models.ModeloAuxiliar;
using TP6Programacion.Backend.Models;

namespace TP6Programacion.Backend.Repositories
{
    public interface IProductoRepository
    {
        Task<PaginadoResponse<Producto>> ObtenerTodos(ProductoQueryParametros parametros);
        Task<Producto?> ObtenerPorId(int id);
        Task<bool> ExistePorNombre(string nombre);
        Task<bool> Existe(int id);
        Task Crear(Producto producto);
        Task Actualizar(Producto producto);
        Task Eliminar(Producto producto);
        Task<List<Producto>> ObtenerPorIds(List<int> productoIds);
    }
}
