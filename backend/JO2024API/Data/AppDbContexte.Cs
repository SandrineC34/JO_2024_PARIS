using Microsoft.EntityFrameworkCore;
using JO2024API.Models;

namespace JO2024API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Tables principales
        public DbSet<User> Users { get; set; }
        public DbSet<Commande> Commandes { get; set; }
        public DbSet<Billet> Billets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuration User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Prenom).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Nom).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.MotDePasse).IsRequired();
                entity.Property(e => e.DateCreation).HasDefaultValueSql("GETUTCDATE()");
            });

            // Configuration Commande
            modelBuilder.Entity<Commande>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NumeroCommande).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.NumeroCommande).IsUnique();
                entity.Property(e => e.TypeOffre).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Sport).IsRequired().HasMaxLength(50);
                entity.Property(e => e.MontantTotal).HasColumnType("decimal(10,2)");
                entity.Property(e => e.DateCommande).HasDefaultValueSql("GETUTCDATE()");

                // Relations
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Commandes)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuration Billet
            modelBuilder.Entity<Billet>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NumeroBillet).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.NumeroBillet).IsUnique();
                entity.Property(e => e.Titre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Sport).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Lieu).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Secteur).HasMaxLength(50);
                entity.Property(e => e.Rangee).HasMaxLength(10);
                entity.Property(e => e.Sieges).HasMaxLength(20);
                entity.Property(e => e.QRCode).HasMaxLength(500);
                entity.Property(e => e.DateCreation).HasDefaultValueSql("GETUTCDATE()");

                // Relations
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Billets)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Commande)
                      .WithMany(c => c.Billets)
                      .HasForeignKey(e => e.CommandeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Données de test
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Utilisateur de test
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Prenom = "Jean",
                    Nom = "Dupont",
                    Email = "jean.dupont@email.com",
                    MotDePasse = BCrypt.Net.BCrypt.HashPassword("TestPassword123!"),
                    DateCreation = DateTime.Now.AddMonths(-3)
                }
            );

            // Commandes de test
            modelBuilder.Entity<Commande>().HasData(
                new Commande
                {
                    Id = 1,
                    NumeroCommande = "CMD-2024-001",
                    UserId = 1,
                    DateCommande = DateTime.Now.AddMonths(-2),
                    TypeOffre = "solo",
                    Sport = "Natation",
                    Quantite = 1,
                    MontantTotal = 75m,
                    Statut = CommandeStatut.Payee,
                    Description = "• Offre Solo - Natation\n• 1x billet à 75€"
                },
                new Commande
                {
                    Id = 2,
                    NumeroCommande = "CMD-2024-002",
                    UserId = 1,
                    DateCommande = DateTime.Now.AddMonths(-1),
                    TypeOffre = "duo",
                    Sport = "Basketball",
                    Quantite = 2,
                    MontantTotal = 130m,
                    Statut = CommandeStatut.Utilisee,
                    Description = "• Offre Duo - Basketball\n• 2x billets à 130€"
                },
                new Commande
                {
                    Id = 3,
                    NumeroCommande = "CMD-2024-003",
                    UserId = 1,
                    DateCommande = DateTime.Now.AddDays(-15),
                    TypeOffre = "famille",
                    Sport = "Athlétisme",
                    Quantite = 4,
                    MontantTotal = 220m,
                    Statut = CommandeStatut.Payee,
                    Description = "• Offre Famille - Athlétisme\n• 4x billets à 220€"
                }
            );

            // Billets de test
            modelBuilder.Entity<Billet>().HasData(
                new Billet
                {
                    Id = 1,
                    NumeroBillet = "JO2024-NAT-001",
                    UserId = 1,
                    CommandeId = 1,
                    Titre = "Natation - Finale 100m Nage Libre",
                    Sport = "Natation",
                    DateEvenement = new DateTime(2024, 7, 25, 20, 30, 0),
                    Lieu = "Centre Aquatique - Paris La Défense",
                    Secteur = "Secteur A",
                    Rangee = "Rangée 15",
                    Sieges = "Siège 12",
                    Statut = BilletStatut.Actif,
                    DateCreation = DateTime.Now.AddMonths(-2)
                },
                new Billet
                {
                    Id = 2,
                    NumeroBillet = "JO2024-BAS-002",
                    UserId = 1,
                    CommandeId = 2,
                    Titre = "Basketball - Finale Hommes",
                    Sport = "Basketball",
                    DateEvenement = new DateTime(2024, 8, 10, 21, 0, 0),
                    Lieu = "Accor Arena - Bercy",
                    Secteur = "Secteur B",
                    Rangee = "Rangée 8",
                    Sieges = "Siège 25-26",
                    Statut = BilletStatut.Scanne,
                    DateScan = new DateTime(2024, 8, 10, 19, 45, 0),
                    DateCreation = DateTime.Now.AddMonths(-1)
                },
                new Billet
                {
                    Id = 3,
                    NumeroBillet = "JO2024-ATH-003",
                    UserId = 1,
                    CommandeId = 3,
                    Titre = "Athlétisme - 100m Hommes",
                    Sport = "Athlétisme",
                    DateEvenement = new DateTime(2024, 8, 4, 21, 50, 0),
                    Lieu = "Stade de France - Saint-Denis",
                    Secteur = "Secteur C",
                    Rangee = "Rangée 20",
                    Sieges = "Sièges 1-4",
                    Statut = BilletStatut.Actif,
                    DateCreation = DateTime.Now.AddDays(-15)
                }
            );
        }
    }
}