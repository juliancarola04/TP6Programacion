using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TP6Programacion.Backend.Models;

namespace TP6Programacion.Backend.Data.Configuraciones
{
    public class AuditoriaConfiguracion : IEntityTypeConfiguration<Auditoria>
    {
        public void Configure(EntityTypeBuilder<Auditoria> builder)
        {
            builder.ToTable("Auditorias");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Fecha).IsRequired();
            builder.Property(x => x.Accion).IsRequired().HasMaxLength(50);
            builder.Property(x => x.Entidad).IsRequired().HasMaxLength(50);
            builder.Property(x => x.DireccionIp).HasMaxLength(45); // alcanza para IPv6

            builder.HasOne(x => x.Usuario)
                .WithMany(x => x.Auditorias)
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.Fecha);
            builder.HasIndex(x => new { x.Entidad, x.EntidadId });
            builder.HasIndex(x => x.UsuarioId);
        }
    }
}
