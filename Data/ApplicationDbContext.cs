using Microsoft.EntityFrameworkCore;
using JeuxOlympiques.Models;

namespace JeuxOlympiques.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Tables de la base de données
        public DbSet<Utilisateur> Utilisateurs { get; set; }
        public DbSet<Offre> Offres { get; set; }
        public DbSet<Commande> Commandes { get; set; }
        public DbSet<Billet> Billets { get; set; }
        public DbSet<Panier> Paniers { get; set; }
        public DbSet<LignePanier> LignesPanier { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuration de la table Utilisateur
            modelBuilder.Entity<Utilisateur>(entity =>
            {
                entity.ToTable("Utilisateurs");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Prenom).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Nom).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(20);
                entity.Property(e => e.DateCreation).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.DateModification).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Configuration de la table Offre
            modelBuilder.Entity<Offre>(entity =>
            {
                entity.ToTable("Offres");
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Nom).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Prix).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Actif).HasDefaultValue(true);
                entity.Property(e => e.DateCreation).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Configuration de la table Commande
            modelBuilder.Entity<Commande>(entity =>
            {
                entity.ToTable("Commandes");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Numero).IsUnique();
                
                entity.Property(e => e.Numero).IsRequired().HasMaxLength(50);
                entity.Property(e => e.MontantTotal).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Statut).IsRequired().HasMaxLength(20);
                entity.Property(e => e.DateCreation).HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Relation avec Utilisateur
                entity.HasOne(e => e.Utilisateur)
                      .WithMany(u => u.Commandes)
                      .HasForeignKey(e => e.UtilisateurId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuration de la table Billet
            modelBuilder.Entity<Billet>(entity =>
            {
                entity.ToTable("Billets");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Numero).IsUnique();
                
                entity.Property(e => e.Numero).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Titre).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Lieu).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Statut).IsRequired().HasMaxLength(20);
                entity.Property(e => e.DateCreation).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.DateModification).HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Relation avec Commande
                entity.HasOne(e => e.Commande)
                      .WithMany(c => c.Billets)
                      .HasForeignKey(e => e.CommandeId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Relation avec Offre
                entity.HasOne(e => e.Offre)
                      .WithMany(o => o.Billets)
                      .HasForeignKey(e => e.OffreId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuration de la table Panier
            modelBuilder.Entity<Panier>(entity =>
            {
                entity.ToTable("Paniers");
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.DateCreation).HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Un utilisateur a un seul panier actif
                entity.HasOne(e => e.Utilisateur)
                      .WithOne()
                      .HasForeignKey<Panier>(e => e.UtilisateurId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuration de la table LignePanier
            modelBuilder.Entity<LignePanier>(entity =>
            {
                entity.ToTable("LignesPanier");
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Quantite).HasDefaultValue(1);

                // Relation avec Panier
                entity.HasOne(e => e.Panier)
                      .WithMany(p => p.Lignes)
                      .HasForeignKey(e => e.PanierId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Relation avec Offre
                entity.HasOne(e => e.Offre)
                      .WithMany()
                      .HasForeignKey(e => e.OffreId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Données de test (Seed Data)
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Offres par défaut
            modelBuilder.Entity<Offre>().HasData(
                new Offre
                {
                    Id = 1,
                    Type = "Solo",
                    Nom = "Billet Solo",
                    Description = "Accès pour 1 personne",
                    Prix = 75.00m,
                    NombrePersonnes = 1,
                    Actif = true,
                    DateCreation = DateTime.Now
                },
                new Offre
                {
                    Id = 2,
                    Type = "Duo",
                    Nom = "Billet Duo",
                    Description = "Accès pour 2 personnes",
                    Prix = 130.00m,
                    NombrePersonnes = 2,
                    Actif = true,
                    DateCreation = DateTime.Now
                },
                new Offre
                {
                    Id = 3,
                    Type = "Famille",
                    Nom = "Billet Famille",
                    Description = "Accès pour 4 personnes maximum",
                    Prix = 220.00m,
                    NombrePersonnes = 4,
                    Actif = true,
                    DateCreation = DateTime.Now
                }
            );

            // Administrateur par défaut (fourni par l'organisation)
            // Mot de passe: AdminJO2024!
            modelBuilder.Entity<Utilisateur>().HasData(
                new Utilisateur
                {
                    Id = 1,
                    Prenom = "Admin",
                    Nom = "JO2024",
                    Email = "admin@jo2024.fr",
                    MotDePasseHash = "$2a$11$XYZ...", // Hash BCrypt du mot de passe
                    CleUtilisateur = Guid.NewGuid().ToString(),
                    Role = "Admin",
                    DateCreation = DateTime.Now,
                    DateModification = DateTime.Now
                }
            );
        }
    }

    // Classes pour Panier et LignePanier
    public class Panier
    {
        public int Id { get; set; }
        public int UtilisateurId { get; set; }
        public Utilisateur Utilisateur { get; set; }
        public DateTime DateCreation { get; set; }
        public ICollection<LignePanier> Lignes { get; set; }
    }

    public class LignePanier
    {
        public int Id { get; set; }
        public int PanierId { get; set; }
        public Panier Panier { get; set; }
        public int OffreId { get; set; }
        public Offre Offre { get; set; }
        public int Quantite { get; set; }
    }
}