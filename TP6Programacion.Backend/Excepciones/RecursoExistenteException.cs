namespace TP6Programacion.Backend.Excepciones;

public class RecursoExistenteException : Exception
{
    // Error 409
    public RecursoExistenteException(string mensaje) :
        base(mensaje)
    {

    }
}