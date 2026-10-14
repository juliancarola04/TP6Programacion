namespace TP6Programacion.Backend.Excepciones;

public class RecursoNoExisteException : Exception
{
    // Error 404
    public RecursoNoExisteException(string mensaje) :
        base(mensaje)
    {

    }
}