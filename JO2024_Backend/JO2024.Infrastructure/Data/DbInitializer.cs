// ============================================
// DbInitializer.cs
// JO2024.Infrastructure/Data/DbInitializer.cs
// ============================================
using JO2024.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace JO2024.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task Initialize(ApplicationDbContext context)
    {
        // Créer la base de données si elle n'existe pas
        await context.Database.MigrateAsync();

        // Vérifier si les données existent déjà
        if (await context.Offres.AnyAsync())
        {
            return; // La base de données est déjà initialisée
        }

        // Ajouter des données de test supplémentaires si nécessaire
        var utilisateurTest = new Utilisateur
        {
            Prenom = "Test",
            Nom = "Utilisateur",
            Email = "test@jo2024.fr",
            MotDePasseHash = BCrypt.Net.BCrypt.HashPassword("Test@123"),
            DateCreation = DateTime.UtcNow,
            EstActif = true
        };

        context.Utilisateurs.Add(utilisateurTest);
        await context.SaveChangesAsync();
    }
}