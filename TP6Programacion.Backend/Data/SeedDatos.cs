using Microsoft.EntityFrameworkCore;
using TP6Programacion.Backend.Models;
using TP6Programacion.Shared.Enums;

namespace TP6Programacion.Backend.Data;

public static class SeedDatos
{
    public static async Task CrearAdministrador(DataContext context)
    {
        bool existe = await context.Usuarios
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Username == "admin");

        if (existe)
            return;

        var usuario = new Usuario
        {
            Username = "admin",
            Password = BCrypt.Net.BCrypt.EnhancedHashPassword("admin"),
            Email = "admin@tp6.local",
            Rol = Rol.Administrador
        };

        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();
    }
}
