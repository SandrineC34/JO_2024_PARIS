using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Utilisateur> Utilisateurs { get; set; }
        public DbSet<Offre> Offres { get; set; }
        // Ajoute ici les autres DbSet nécessaires

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

            // Les autres configurations sont à adapter selon tes autres modèles

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

            // Administrateur par défaut
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
}
