using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TP6Programacion.Backend.Models;

namespace TP6Programacion.Backend.Data.Configuraciones
{
    public class DetalleVentaConfiguracion : IEntityTypeConfiguration<DetalleVenta>
    {
        public void Configure(EntityTypeBuilder<DetalleVenta> builder)
        {
            builder.ToTable("DetallesVentas");

            builder.HasKey(x => new {x.ProductoId, x.VentaId});

            builder.Property(x => x.Cantidad)
                .IsRequired();

            builder.Property(x => x.PrecioUnitario)
                .IsRequired();

            builder.HasOne(x => x.Producto)
                .WithMany(x => x.DetallesVentas)
                .HasForeignKey(x => x.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Venta)
                .WithMany(x => x.DetallesVentas)
                .HasForeignKey(x => x.VentaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
