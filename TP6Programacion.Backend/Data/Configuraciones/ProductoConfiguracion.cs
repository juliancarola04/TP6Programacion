using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TP6Programacion.Backend.Models;

namespace TP6Programacion.Backend.Data.Configuraciones
{
    public class ProductoConfiguracion : IEntityTypeConfiguration<Producto>
    {
        public void Configure(EntityTypeBuilder<Producto> builder)
        {
            builder.ToTable("Productos");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Nombre)
                .IsRequired()
                .HasMaxLength(60);

            builder.Property(x => x.PrecioCompra)
                .IsRequired();

            builder.Property(x => x.PrecioVenta)
                .IsRequired();

            builder.Property(x => x.Stock)
                .IsRequired();

            builder.HasOne(x => x.Categoria)
                .WithMany(x => x.Productos)
                .HasForeignKey(x => x.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.CategoriaId);
        }
    }
}
