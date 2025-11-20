// ============================================
// Factory pour créer des contextes de test
// JO2024.Tests/Integration/TestDbContextFactory.cs
// ============================================

using Microsoft.EntityFrameworkCore;
using JO2024.Infrastructure.Data;
using JO2024.Core.Entities;

namespace JO2024.Tests.Integration;

/// <summary>
/// Factory pour créer des contextes de base de données pour les tests
/// </summary>
public static class TestDbContextFactory
{
    /// <summary>
    /// Crée un nouveau contexte InMemory avec un nom unique
    /// </summary>
    public static ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    /// <summary>
    /// Crée un contexte avec des données de test pré-remplies
    /// </summary>
    public static ApplicationDbContext CreateSeededContext()
    {
        var context = CreateInMemoryContext();
        SeedTestData(context);
        return context;
    }

    /// <summary>
    /// Remplit le contexte avec des données de test
    /// </summary>
    private static void SeedTestData(ApplicationDbContext context)
    {
        // Utilisateurs de test
        var users = new[]
        {
            new Utilisateur
            {
                Id = 1,
                Prenom = "Admin",
                Nom = "Test",
                Email = "admin@test.com",
                MotDePasseHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Role = "Admin",
                EstActif = true,
                DateCreation = DateTime.UtcNow,
                NewsletterAbonne = false
            },
            new Utilisateur
            {
                Id = 2,
                Prenom = "User",
                Nom = "Standard",
                Email = "user@test.com",
                MotDePasseHash = BCrypt.Net.BCrypt.HashPassword("User123!"),
                Role = "Utilisateur",
                EstActif = true,
                DateCreation = DateTime.UtcNow,
                NewsletterAbonne = true,
                NewsletterCategories = "{\"Sport\":true,\"Evenements\":false,\"Billets\":true}"
            },
            new Utilisateur
            {
                Id = 3,
                Prenom = "Inactive",
                Nom = "User",
                Email = "inactive@test.com",
                MotDePasseHash = BCrypt.Net.BCrypt.HashPassword("Inactive123!"),
                Role = "Utilisateur",
                EstActif = false,
                DateCreation = DateTime.UtcNow
            }
        };

        context.Utilisateurs.AddRange(users);

        // Offres de test
        var offres = new[]
        {
            new Offre
            {
                Id = 1,
                Type = "Solo",
                Nom = "Billet Solo",
                Description = "Un billet individuel",
                Prix = 50,
                NombrePersonnes = 1,
                Caracteristiques = "Accès standard",
                EstActif = true,
                DateCreation = DateTime.UtcNow
            },
            new Offre
            {
                Id = 2,
                Type = "Duo",
                Nom = "Pack Duo",
                Description = "Deux billets",
                Prix = 90,
                NombrePersonnes = 2,
                Caracteristiques = "Accès standard",
                EstActif = true,
                DateCreation = DateTime.UtcNow
            },
            new Offre
            {
                Id = 3,
                Type = "Famille",
                Nom = "Pack Famille",
                Description = "Quatre billets",
                Prix = 150,
                NombrePersonnes = 4,
                Caracteristiques = "Accès privilégié",
                EstActif = true,
                DateCreation = DateTime.UtcNow
            }
        };

        context.Offres.AddRange(offres);

        context.SaveChanges();
    }

    /// <summary>
    /// Crée un utilisateur de test
    /// </summary>
    public static Utilisateur CreateTestUser(string email = "test@example.com", string password = "Test123!")
    {
        return new Utilisateur
        {
            Prenom = "Test",
            Nom = "User",
            Email = email,
            MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = "Utilisateur",
            EstActif = true,
            DateCreation = DateTime.UtcNow,
            NewsletterAbonne = false
        };
    }

    /// <summary>
    /// Crée un utilisateur avec abonnement newsletter
    /// </summary>
    public static Utilisateur CreateNewsletterUser(string email = "newsletter@test.com")
    {
        return new Utilisateur
        {
            Prenom = "Newsletter",
            Nom = "User",
            Email = email,
            MotDePasseHash = BCrypt.Net.BCrypt.HashPassword("Newsletter123!"),
            Role = "Utilisateur",
            EstActif = true,
            DateCreation = DateTime.UtcNow,
            NewsletterAbonne = true,
            NewsletterCategories = "{\"Sport\":true,\"Evenements\":true,\"Billets\":false}",
            NewsletterSports = "[{\"Id\":\"natation\",\"Name\":\"Natation\"},{\"Id\":\"athletisme\",\"Name\":\"Athlétisme\"}]",
            NewsletterDateInscription = DateTime.UtcNow
        };
    }
}