using TP6Programacion.Backend.Models.ModeloAuxiliar.Query.Ingreso;
using TP6Programacion.Backend.Models.ModeloAuxiliar;
using TP6Programacion.Backend.Models;

namespace TP6Programacion.Backend.Repositories
{
    public interface IIngresoRepository
    {
        Task<PaginadoResponse<Ingreso>> ObtenerTodos(IngresoQueryParametros parametros);
        Task<Ingreso?> ObtenerPorId(int id);
        Task<Ingreso?> ObtenerParaAnular(int id);
        Task Crear(Ingreso ingreso);
        Task GuardarCambios();
    }
}
