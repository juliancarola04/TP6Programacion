using TP6Programacion.Backend.Models;

namespace TP6Programacion.Backend.Repositories
{
    public interface IPagoRepository
    {
        Task Crear(Pago pago);
        Task GuardarCambios();
    }
}
