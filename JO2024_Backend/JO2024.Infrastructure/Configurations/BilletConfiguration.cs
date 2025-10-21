// ============================================
// BilletConfiguration.cs
// JO2024.Infrastructure/Configurations/BilletConfiguration.cs
// ============================================
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using JO2024.Core.Entities;

namespace JO2024.Infrastructure.Configurations;

public class BilletConfiguration : IEntityTypeConfiguration<Billet>
{
    public void Configure(EntityTypeBuilder<Billet> builder)
    {
        builder.ToTable("Billets");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Numero)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(b => b.Numero)
            .IsUnique();

        builder.Property(b => b.Titre)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.Sport)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(b => b.Lieu)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.CodeQR)
            .IsRequired();

        builder.Property(b => b.DateCreation)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(b => b.Commande)
            .WithMany(c => c.Billets)
            .HasForeignKey(b => b.CommandeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(b => b.Utilisateur)
            .WithMany(u => u.Billets)
            .HasForeignKey(b => b.UtilisateurId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}