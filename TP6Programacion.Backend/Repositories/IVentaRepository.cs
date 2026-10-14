using TP6Programacion.Backend.Models.ModeloAuxiliar.Query.Venta;
using TP6Programacion.Backend.Models.ModeloAuxiliar;
using TP6Programacion.Backend.Models;

namespace TP6Programacion.Backend.Repositories
{
    public interface IVentaRepository
    {
        Task<PaginadoResponse<Venta>> ObtenerTodas(VentaQueryParametros parametros);
        Task<Venta?> ObtenerPorId(int id);
        Task<Venta?> ObtenerParaAnular(int id);
        Task Crear(Venta venta);
        Task GuardarCambios();
    }
}
