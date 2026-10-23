using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TP6Programacion.Backend.Models;

namespace TP6Programacion.Backend.Data.Configuraciones
{
    public class VentaConfiguracion : IEntityTypeConfiguration<Venta>
    {
        public void Configure(EntityTypeBuilder<Venta> builder)
        {
            builder.ToTable("Ventas", table =>
            {
                // Debe existir un Cliente o un Usuario como mínimo.
                table.HasCheckConstraint(
                    "CK_Ventas_ClienteOUsuario",
                    "\"ClienteId\" IS NOT NULL OR \"UsuarioId\" IS NOT NULL"
                );

                // FechaExpiracion solo se permite en ventas
                // pendientes y online.
                table.HasCheckConstraint(
                    "CK_Ventas_FechaExpiracion",
                    "\"FechaExpiracion\" IS NULL OR " +
                    "(\"Estado\" = 1 AND \"UsuarioId\" IS NULL " +
                    "AND \"ClienteId\" IS NOT NULL)"
                );
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Total)
                .IsRequired()
                .HasPrecision(12,2);

            builder.Property(x => x.Fecha)
                .IsRequired();
            
            builder.Property(x => x.Estado)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.FechaExpiracion)
                .IsRequired(false);

            builder.HasOne(x => x.Usuario)
                .WithMany(x => x.Ventas)
                .HasForeignKey(x => x.UsuarioId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Cliente)
                .WithMany(x => x.Ventas)
                .HasForeignKey(x => x.ClienteId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.UsuarioId);
            builder.HasIndex(x => x.ClienteId);
        }
    }
}
