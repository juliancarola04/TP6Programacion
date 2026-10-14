using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TP6Programacion.Backend.Models;

namespace TP6Programacion.Backend.Data.Configuraciones
{
    public class CategoriaConfiguracion : IEntityTypeConfiguration<Categoria>
    {
        public void Configure(EntityTypeBuilder<Categoria> builder)
        {
            builder.ToTable("Categorias");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Nombre)
                .IsRequired()
                .HasMaxLength(30);

            builder.Property(x => x.Descripcion)
                .IsRequired()
                .HasMaxLength(600);

        }
    }
}
