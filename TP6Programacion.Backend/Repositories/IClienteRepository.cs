using TP6Programacion.Backend.Models.ModeloAuxiliar.Query.Cliente;
using TP6Programacion.Backend.Models.ModeloAuxiliar;
using TP6Programacion.Backend.Models;

namespace TP6Programacion.Backend.Repositories
{
    public interface IClienteRepository
    {
        Task<PaginadoResponse<Cliente>> ObtenerTodos(ClienteQueryParametros parametros);
        Task<Cliente?> ObtenerPorUsuarioId(int usuarioId);
        Task<Cliente?> ObtenerPorId(int id);
        Task<bool> ExistePorDni(string dni);
        Task<bool> ExistePorEmail(string email);
        Task<bool> TieneVentasAsociadas(int clienteId);

        Task Crear(Cliente cliente);
        Task Actualizar(Cliente cliente);
        Task Eliminar(Cliente cliente);
    }
}
