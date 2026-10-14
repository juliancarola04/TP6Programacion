using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TP6Programacion.Backend.Models;

namespace TP6Programacion.Backend.Data.Configuraciones
{
    public class UsuarioConfiguracion : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.ToTable("Usuarios");

            builder.HasKey(x => x.Id);

            builder.HasIndex(x => x.Username)
                .IsUnique();
            
            builder.HasIndex(x => x.Email)
                .IsUnique();
            
            builder.Property(x => x.Username)
                .IsRequired()
                .HasMaxLength(30);

            builder.Property(x => x.Password)
                .IsRequired();

            builder.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(320);
        }
    }
}
