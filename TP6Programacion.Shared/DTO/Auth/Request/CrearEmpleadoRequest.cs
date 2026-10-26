using System;
using System.Collections.Generic;
using System.Text;
using TP6Programacion.Shared.Enums;

namespace TP6Programacion.Shared.DTO.Auth.Request
{
    public record CrearEmpleadoRequest(
        string Username,
        string Password,
        string Email,
        Rol Rol);
}
