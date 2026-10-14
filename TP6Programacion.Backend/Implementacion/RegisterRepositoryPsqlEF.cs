using TP6Programacion.Backend.Data;
using TP6Programacion.Backend.Models;
using TP6Programacion.Backend.Repositories;

namespace TP6Programacion.Backend.Implementacion
{
    public class RegisterRepositoryPsqlEF : IRegisterRepository
    {
        private readonly DataContext _dataContext;
        public RegisterRepositoryPsqlEF(DataContext dataContext)
        {
            _dataContext = dataContext;
        }
        
        public async Task Registrarse (Usuario usuario)
        {
            _dataContext.Usuarios.Add(usuario);

            await _dataContext.SaveChangesAsync();
        }
    }
}
