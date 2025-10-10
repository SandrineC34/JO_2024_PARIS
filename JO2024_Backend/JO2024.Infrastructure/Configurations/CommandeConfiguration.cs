// ============================================
// CommandeConfiguration.cs
// JO2024.Infrastructure/Configurations/CommandeConfiguration.cs
// ============================================
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using JO2024.Core.Entities;

namespace JO2024.Infrastructure.Configurations;

public class CommandeConfiguration : IEntityTypeConfiguration<Commande>
{
    public void Configure(EntityTypeBuilder<Commande> builder)
    {
        builder.ToTable("Commandes");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Numero)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(c => c.Numero)
            .IsUnique();

        builder.Property(c => c.MontantHT)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(c => c.MontantTVA)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(c => c.MontantTotal)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(c => c.DateAchat)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(c => c.Utilisateur)
            .WithMany(u => u.Commandes)
            .HasForeignKey(c => c.UtilisateurId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Items)
            .WithOne(ci => ci.Commande)
            .HasForeignKey(ci => ci.CommandeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Billets)
            .WithOne(b => b.Commande)
            .HasForeignKey(b => b.CommandeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}