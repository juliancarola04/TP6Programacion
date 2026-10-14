using TP6Programacion.Backend.Models.ModeloAuxiliar.Query.Categoria;
using TP6Programacion.Backend.Models.ModeloAuxiliar;
using TP6Programacion.Backend.Models;

namespace TP6Programacion.Backend.Repositories
{
    public interface ICategoriaRepository
    {
        Task<PaginadoResponse<Categoria>> ObtenerTodas(CategoriaQueryParametros parametros);
        Task<Categoria?> ObtenerPorId(int id);
        Task<bool> ExistePorNombre(string nombre);
        Task<bool> TieneProductosAsociados(int categoriaId);

        Task Crear(Categoria categoria);
        Task Actualizar(Categoria categoria);
        Task Eliminar(Categoria categoria);
    }
}
