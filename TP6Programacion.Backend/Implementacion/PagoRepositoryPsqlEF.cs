using TP6Programacion.Backend.Data;
using TP6Programacion.Backend.Models;
using TP6Programacion.Backend.Repositories;

namespace TP6Programacion.Backend.Implementacion
{
    public class PagoRepositoryPsqlEF : IPagoRepository
    {
        private readonly DataContext _dataContext;

        public PagoRepositoryPsqlEF(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task Crear(Pago pago)
        {
            _dataContext.Pagos.Add(pago);
            await _dataContext.SaveChangesAsync();
        }

        public async Task GuardarCambios()
        {
            await _dataContext.SaveChangesAsync();
        }
    }
}

