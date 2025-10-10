// ============================================
// OffreConfiguration.cs
// JO2024.Infrastructure/Configurations/OffreConfiguration.cs
// ============================================
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using JO2024.Core.Entities;

namespace JO2024.Infrastructure.Configurations;

public class OffreConfiguration : IEntityTypeConfiguration<Offre>
{
    public void Configure(EntityTypeBuilder<Offre> builder)
    {
        builder.ToTable("Offres");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Type)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(o => o.Nom)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(o => o.Prix)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(o => o.DateCreation)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasMany(o => o.CommandeItems)
            .WithOne(ci => ci.Offre)
            .HasForeignKey(ci => ci.OffreId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
