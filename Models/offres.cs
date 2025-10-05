using Microsoft.EntityFrameworkCore;        // Importe Entity Framework Core pour les opérations ORM (mapping objet-relationnel)
using JO2024API.Models;                     // Importe les définitions des entités du projet (User, Offre...)

namespace JO2024API.Data
{
    // Cette classe représente le contexte de la base de données pour Entity Framework Core
    public class AppDbContext : DbContext
    {
        // Constructeur : reçoit les options de configuration (chaîne de connexion SQL...) au démarrage via injection de dépendances
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Déclaration des ensembles d'entités : chaque DbSet correspond à une table SQL
        public DbSet<User> Users { get; set; }                      // Table des utilisateurs
        public DbSet<Offre> Offres { get; set; }                    // Table des offres de billets
        public DbSet<OffreCaracteristique> OffreCaracteristiques { get; set; } // Table des caractéristiques des offres
        public DbSet<SportOption> SportOptions { get; set; }        // Table des sports proposés
        public DbSet<PanierItem> PanierItems { get; set; }          // Table des éléments de panier

        // Permet de configurer finement le mapping des entités, contraintes, relations et index
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Appelle la config de la classe mère (optionnelle, bonne pratique)

            // --- Configuration de la table Offre ---
            modelBuilder.Entity<Offre>(entity =>
            {
                entity.HasKey(e => e.Id);                           // Spécifie la clé primaire : Id
                entity.Property(e => e.Prix).HasPrecision(10, 2);   // Définit la précision du champ Prix (décimal)
                entity.Property(e => e.EconomieVsSolo).HasPrecision(10, 2);  // Idem pour EconomieVsSolo
                entity.HasIndex(e => e.Type).IsUnique();            // Crée un index UNIQUE sur le Type (solo/duo/famille...)
            });

            // --- Configuration de la table OffreCaracteristique ---
            modelBuilder.Entity<OffreCaracteristique>(entity =>
            {
                entity.HasKey(e => e.Id);                           // Clé primaire : Id
                // Définit la relation : chaque caractéristique appartient à une offre
                entity.HasOne(e => e.Offre)
                      .WithMany(o => o.Caracteristiques)
                      .HasForeignKey(e => e.OffreId)
                      .OnDelete(DeleteBehavior.Cascade);            // Suppression en CASCADE : si une offre est supprimée, ses caractéristiques aussi

                entity.HasIndex(e => new { e.OffreId, e.Ordre });   // Index COMPOSITE sur OffreId + Ordre pour optimiser recherches
            });

            // --- Configuration de la table SportOption ---
            modelBuilder.Entity<SportOption>(entity =>
            {
                entity.HasKey(e => e.Id);                           // Clé primaire : Id
                entity.HasIndex(e => e.Code).IsUnique();            // Index UNIQUE sur le code du sport (ex : "natation")
            });

            // --- Configuration de la table PanierItem ---
            modelBuilder.Entity<PanierItem>(entity =>
            {
                entity.HasKey(e => e.Id);                           // Clé primaire : Id
                entity.Property(e => e.PrixUnitaire).HasPrecision(10, 2); // Précision décimale pour PrixUnitaire

                // Relation optionnelle avec User, suppression : met à NULL UserId si un User est supprimé
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.SetNull);

                // Relation avec Offre : suppression en CASCADE, si une offre est supprimée, les éléments du panier aussi
                entity.HasOne(e => e.Offre)
                      .WithMany()
                      .HasForeignKey(e => e.OffreId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Index pour accélérer les recherches dans le panier (multi-user, multi-session)
                entity.HasIndex(e => e.SessionId);
                entity.HasIndex(e => e.UserId);
            });

            // --- Configuration de la table User ---
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);                           // Clé primaire : Id
                entity.HasIndex(e => e.Email).IsUnique();           // Index UNIQUE sur le champ Email (éviter les doublons)
            });
        }
    }
}
