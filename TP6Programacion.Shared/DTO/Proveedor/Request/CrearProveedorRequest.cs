namespace TP6Programacion.Shared.DTO.Proveedor.Request;

public record CrearProveedorRequest(
    string RazonSocial,
    string Cuit,
    string Direccion,
    string Email,
    string Telefono);