// ============================================
// CommandeItemConfiguration.cs
// JO2024.Infrastructure/Configurations/CommandeItemConfiguration.cs
// ============================================
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using JO2024.Core.Entities;

namespace JO2024.Infrastructure.Configurations;

public class CommandeItemConfiguration : IEntityTypeConfiguration<CommandeItem>
{
    public void Configure(EntityTypeBuilder<CommandeItem> builder)
    {
        builder.ToTable("CommandeItems");

        builder.HasKey(ci => ci.Id);

        builder.Property(ci => ci.PrixUnitaire)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(ci => ci.PrixTotal)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.HasOne(ci => ci.Commande)
            .WithMany(c => c.Items)
            .HasForeignKey(ci => ci.CommandeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ci => ci.Offre)
            .WithMany(o => o.CommandeItems)
            .HasForeignKey(ci => ci.OffreId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}