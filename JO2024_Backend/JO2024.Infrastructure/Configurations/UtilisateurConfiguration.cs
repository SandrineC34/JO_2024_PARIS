// ============================================
// UtilisateurConfiguration.cs
// JO2024.Infrastructure/Configurations/UtilisateurConfiguration.cs
// ============================================
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using JO2024.Core.Entities;

namespace JO2024.Infrastructure.Configurations;

public class UtilisateurConfiguration : IEntityTypeConfiguration<Utilisateur>
{
    public void Configure(EntityTypeBuilder<Utilisateur> builder)
    {
        builder.ToTable("Utilisateurs");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.Prenom)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Nom)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.MotDePasseHash)
            .IsRequired();

        builder.Property(u => u.DateCreation)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasMany(u => u.Commandes)
            .WithOne(c => c.Utilisateur)
            .HasForeignKey(c => c.UtilisateurId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.Billets)
            .WithOne(b => b.Utilisateur)
            .HasForeignKey(b => b.UtilisateurId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}