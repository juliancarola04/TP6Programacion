using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TP6Programacion.Backend.Models;

namespace TP6Programacion.Backend.Data.Configuraciones
{
    public class IngresoConfiguracion : IEntityTypeConfiguration<Ingreso>
    {
        public void Configure(EntityTypeBuilder<Ingreso> builder)
        {
            builder.ToTable("Ingresos");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Total)
                .IsRequired();

            builder.Property(x => x.Fecha)
                .IsRequired();

            builder.HasOne(x => x.Proveedor)
                .WithMany(x => x.Ingresos)
                .HasForeignKey(x => x.ProveedorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Usuario)
                .WithMany(x => x.Ingresos)
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.UsuarioId);
            builder.HasIndex(x => x.ProveedorId);
        }
    }
}
