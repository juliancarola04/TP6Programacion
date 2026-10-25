using TP6Programacion.Backend.Models.ModeloAuxiliar.Query.Usuario;
using TP6Programacion.Backend.Models.ModeloAuxiliar;
using TP6Programacion.Backend.Models;

namespace TP6Programacion.Backend.Repositories
{
    public interface IUsuarioRepository
    {
        Task<bool> ExistePorUsername(string username);
        Task<bool> ExistePorEmail(string email);
        Task<Usuario?> BuscarPorUsername(string username);
        Task<Usuario?> BuscarPorId(int id);
        Task DarDeBaja(Usuario usuario);
        Task<PaginadoResponse<Usuario>> ObtenerTodos(UsuarioQueryParametros usuarioQueryParametros);
        Task Actualizar(Usuario usuario);
        Task<int>ContarAdministradores();
    }
}
