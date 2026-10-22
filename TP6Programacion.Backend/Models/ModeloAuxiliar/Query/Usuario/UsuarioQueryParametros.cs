using TP6Programacion.Shared.Enums;
namespace TP6Programacion.Backend.Models.ModeloAuxiliar.Query.Usuario;

public record UsuarioQueryParametros : QueryParametros
{
    public bool? Eliminado { get; init; }
    public Rol? Rol { get; init; }
}