using TP6Programacion.Shared.DTO.Auth.Response;

namespace TP6Programacion.Web.Services;

public class SesionUsuarioService
{
    // Los tokens viven en cookies HttpOnly; solo los datos visibles se guardan en memoria.
    public UsuarioActualResponse? UsuarioActual { get; private set; }

    public void EstablecerUsuario(UsuarioActualResponse usuario)
    {
        UsuarioActual = usuario;
    }

    public void Limpiar()
    {
        UsuarioActual = null;
    }
}
