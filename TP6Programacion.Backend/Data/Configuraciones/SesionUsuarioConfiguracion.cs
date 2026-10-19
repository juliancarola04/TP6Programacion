using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TP6Programacion.Backend.Models;

namespace TP6Programacion.Backend.Data.Configuraciones;

public class SesionUsuarioConfiguracion
    : IEntityTypeConfiguration<SesionUsuario>
{
    public void Configure(EntityTypeBuilder<SesionUsuario> builder)
    {
        builder.ToTable("SesionesUsuarios");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.RefreshToken)
            .IsRequired()
            .HasMaxLength(64)
            .IsConcurrencyToken(); // Esto lo agregué por las dudas de que justo pase que se intente renovar 2 veces esto.

        builder.HasIndex(x => x.RefreshToken)
            .IsUnique();

        builder.HasOne(x => x.Usuario)
            .WithMany()
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}