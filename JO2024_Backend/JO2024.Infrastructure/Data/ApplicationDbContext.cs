// ============================================
// ApplicationDbContext.cs - Version mise à jour avec Role
// JO2024.Infrastructure/Data/ApplicationDbContext.cs
// ============================================
using Microsoft.EntityFrameworkCore;
using JO2024.Core.Entities;

namespace JO2024.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Utilisateur> Utilisateurs { get; set; }
    public DbSet<Offre> Offres { get; set; }
    public DbSet<Commande> Commandes { get; set; }
    public DbSet<CommandeItem> CommandeItems { get; set; }
    public DbSet<Billet> Billets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ============================================
        // Configuration de l'entité Utilisateur
        // ============================================
        modelBuilder.Entity<Utilisateur>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Prenom).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Nom).IsRequired().HasMaxLength(100);
            entity.Property(e => e.MotDePasseHash).IsRequired();
            
            // ✅ Configuration de la propriété Role
            entity.Property(e => e.Role)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Utilisateur");
            
            entity.Property(e => e.DateCreation).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.EstActif).HasDefaultValue(true);
            
            // Configuration des propriétés Newsletter
            entity.Property(e => e.NewsletterAbonne).HasDefaultValue(false);
            entity.Property(e => e.NewsletterCategories).HasColumnType("nvarchar(max)");
            entity.Property(e => e.NewsletterSports).HasColumnType("nvarchar(max)");
            entity.Property(e => e.NewsletterUnsubscribeToken).HasMaxLength(255);
            
            // Propriétés de réinitialisation de mot de passe
            entity.Property(e => e.ResetPasswordToken).HasMaxLength(255);
        });

        // ============================================
        // Configuration de l'entité Offre
        // ============================================
        modelBuilder.Entity<Offre>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Nom).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Prix).HasColumnType("decimal(10,2)").IsRequired();
            entity.Property(e => e.NombrePersonnes).IsRequired();
            entity.Property(e => e.Caracteristiques).HasMaxLength(500);
            entity.Property(e => e.EstActif).HasDefaultValue(true);
            entity.Property(e => e.DateCreation).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // ============================================
        // Configuration de l'entité Commande
        // ============================================
        modelBuilder.Entity<Commande>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Numero).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Numero).IsUnique();
            entity.Property(e => e.DateAchat).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.MontantHT).HasColumnType("decimal(10,2)").IsRequired();
            entity.Property(e => e.MontantTVA).HasColumnType("decimal(10,2)").IsRequired();
            entity.Property(e => e.MontantTotal).HasColumnType("decimal(10,2)").IsRequired();
            entity.Property(e => e.Statut).IsRequired().HasMaxLength(50).HasDefaultValue("Payée");
            entity.Property(e => e.MethodePaiement).HasMaxLength(100);

            // Relations
            entity.HasOne(e => e.Utilisateur)
                  .WithMany(u => u.Commandes)
                  .HasForeignKey(e => e.UtilisateurId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ============================================
        // Configuration de l'entité CommandeItem
        // ============================================
        modelBuilder.Entity<CommandeItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Quantite).IsRequired();
            entity.Property(e => e.PrixUnitaire).HasColumnType("decimal(10,2)").IsRequired();
            entity.Property(e => e.PrixTotal).HasColumnType("decimal(10,2)").IsRequired();
            entity.Property(e => e.Sport).HasMaxLength(100);

            // Relations
            entity.HasOne(e => e.Commande)
                  .WithMany(c => c.Items)
                  .HasForeignKey(e => e.CommandeId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Offre)
                  .WithMany(o => o.CommandeItems)
                  .HasForeignKey(e => e.OffreId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ============================================
        // Configuration de l'entité Billet
        // ============================================
        modelBuilder.Entity<Billet>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Numero).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Numero).IsUnique();
            entity.Property(e => e.Titre).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Sport).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Lieu).IsRequired().HasMaxLength(200);
            entity.Property(e => e.DateEpreuve).IsRequired();
            entity.Property(e => e.Place).HasMaxLength(50);
            entity.Property(e => e.Statut).IsRequired().HasMaxLength(50).HasDefaultValue("Actif");
            entity.Property(e => e.CodeQR).IsRequired();
            entity.Property(e => e.DateCreation).HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relations
            entity.HasOne(e => e.Commande)
                  .WithMany(c => c.Billets)
                  .HasForeignKey(e => e.CommandeId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Utilisateur)
                  .WithMany(u => u.Billets)
                  .HasForeignKey(e => e.UtilisateurId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}