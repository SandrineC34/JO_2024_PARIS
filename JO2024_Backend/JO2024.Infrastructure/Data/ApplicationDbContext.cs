// ============================================
// ApplicationDbContext.cs
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

        // Appliquer les configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Seed data initial
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        var now = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Offres initiales
        modelBuilder.Entity<Offre>().HasData(
            new Offre
            {
                Id = 1,
                Type = "solo",
                Nom = "Offre Solo",
                Description = "Accès pour 1 personne",
                Prix = 75.00m,
                NombrePersonnes = 1,
                EstActif = true,
                DateCreation = now
            },
            new Offre
            {
                Id = 2,
                Type = "duo",
                Nom = "Offre Duo",
                Description = "Accès pour 2 personnes - Économie de 20€",
                Prix = 130.00m,
                NombrePersonnes = 2,
                EstActif = true,
                DateCreation = now
            },
            new Offre
            {
                Id = 3,
                Type = "famille",
                Nom = "Offre Famille",
                Description = "Accès pour 4 personnes - Économie de 80€",
                Prix = 220.00m,
                NombrePersonnes = 4,
                EstActif = true,
                DateCreation = now
            }
        );
    }
}