// Data/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using JO2024API.Models;

namespace JO2024API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSets pour chaque table
        public DbSet<User> Users { get; set; }
        public DbSet<Commande> Commandes { get; set; }
        public DbSet<Billet> Billets { get; set; }
        public DbSet<Panier> Paniers { get; set; }
        public DbSet<Paiement> Paiements { get; set; }
        public DbSet<SessionUtilisateur> SessionsUtilisateur { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuration des relations et contraintes
            
            // User -> Commandes (1:n)
            modelBuilder.Entity<Commande>()
                .HasOne(c => c.User)
                .WithMany(u => u.Commandes)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // User -> Panier (1:1)
            modelBuilder.Entity<Panier>()
                .HasOne(p => p.User)
                .WithOne(u => u.Panier)
                .HasForeignKey<Panier>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> Sessions (1:n)
            modelBuilder.Entity<SessionUtilisateur>()
                .HasOne(s => s.User)
                .WithMany(u => u.Sessions)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Commande -> Billets (1:n)
            modelBuilder.Entity<Billet>()
                .HasOne(b => b.Commande)
                .WithMany(c => c.Billets)
                .HasForeignKey(b => b.CommandeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Billet -> User (n:1)
            modelBuilder.Entity<Billet>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Commande -> Paiement (1:1)
            modelBuilder.Entity<Paiement>()
                .HasOne(p => p.Commande)
                .WithOne(c => c.Paiement)
                .HasForeignKey<Paiement>(p => p.CommandeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Index pour les performances
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Billet>()
                .HasIndex(b => b.NumeroBillet)
                .IsUnique();

            modelBuilder.Entity<Commande>()
                .HasIndex(c => c.NumeroCommande)
                .IsUnique();

            modelBuilder.Entity<SessionUtilisateur>()
                .HasIndex(s => s.TokenSession)
                .IsUnique();

            // Configuration des types de données
            modelBuilder.Entity<Commande>()
                .Property(c => c.MontantTotal)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<Paiement>()
                .Property(p => p.Montant)
                .HasColumnType("decimal(10,2)");

            // Données de test (optionnel)
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Données de test pour développement
            var testUser = new User
            {
                Id = 1,
                Nom = "Dupont",
                Prenom = "Jean",
                Email = "jean.dupont@email.com",
                MotDePasse = BCrypt.Net.BCrypt.HashPassword("Test123!"),
                CleUtilisateur = Guid.NewGuid().ToString(),
                DateCreation = DateTime.UtcNow,
                Adresse = "123 Rue de la Paix, 75001 Paris",
                Telephone = "0123456789"
            };

            modelBuilder.Entity<User>().HasData(testUser);

            var testCommande = new Commande
            {
                Id = 1,
                NumeroCommande = "CMD-2024-001",
                UserId = 1,
                MontantTotal = 75m,
                Statut = CommandeStatut.Payee,
                DateCommande = DateTime.UtcNow.AddDays(-30),
                Description = "Offre Solo - Natation • 1x billet à 75€"
            };

            modelBuilder.Entity<Commande>().HasData(testCommande);

            var testBillets = new[]
            {
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
                    DateGeneration = DateTime.UtcNow.AddDays(-30)
                },
                new Billet
                {
                    Id = 2,
                    NumeroBillet = "JO2024-BAS-002",
                    UserId = 1,
                    CommandeId = 1,
                    Titre = "Basketball - Finale Hommes",
                    Sport = "Basketball",
                    DateEvenement = new DateTime(2024, 8, 10, 21, 0, 0),
                    Lieu = "Accor Arena - Bercy",
                    Secteur = "Secteur B",
                    Rangee = "Rangée 8",
                    Sieges = "Siège 25-26",
                    Statut = BilletStatut.Scanne,
                    DateGeneration = DateTime.UtcNow.AddDays(-25),
                    DateScan = DateTime.UtcNow.AddDays(-1)
                }
            };

            modelBuilder.Entity<Billet>().HasData(testBillets);
        }
    }
}