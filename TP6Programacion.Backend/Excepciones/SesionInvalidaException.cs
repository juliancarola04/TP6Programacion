namespace TP6Programacion.Backend.Excepciones;

public class SesionInvalidaException : Exception
{
    public SesionInvalidaException() : base("La sesión no es válida o venció.")
    {
    }
}