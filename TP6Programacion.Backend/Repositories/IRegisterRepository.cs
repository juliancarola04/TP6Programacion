using TP6Programacion.Backend.Models;

namespace TP6Programacion.Backend.Repositories
{
    public interface IRegisterRepository
    {
        Task Registrarse(Usuario usuario);
    }
}
