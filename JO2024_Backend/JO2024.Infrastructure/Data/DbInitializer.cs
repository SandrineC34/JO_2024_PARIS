// ============================================
// DbInitializer.cs
// JO2024.Infrastructure/Data/DbInitializer.cs
// ============================================
using JO2024.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pomelo.EntityFrameworkCore.MySql;

namespace JO2024.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task Initialize(ApplicationDbContext context, ILogger logger)
    {
        try
        {
            logger.LogInformation("Début de l'initialisation de la base de données...");

            // Attendre que MySQL soit prêt (max 60 secondes)
            var retryCount = 0;
            var maxRetries = 12;
            while (retryCount < maxRetries)
            {
                try
                {
                    await context.Database.CanConnectAsync();
                    logger.LogInformation("Connexion à la base de données établie");
                    break;
                }
                catch (Exception ex)
                {
                    retryCount++;
                    if (retryCount >= maxRetries)
                    {
                        logger.LogError(ex, "Impossible de se connecter à la base de données après {RetryCount} tentatives", maxRetries);
                        throw;
                    }
                    logger.LogWarning("Tentative de connexion {RetryCount}/{MaxRetries}...", retryCount, maxRetries);
                    await Task.Delay(5000); // Attendre 5 secondes
                }
            }

            // Appliquer les migrations
            logger.LogInformation("Application des migrations...");
            await context.Database.MigrateAsync();
            logger.LogInformation("Migrations appliquées avec succès");

            // Vérifier si les données existent déjà
            if (await context.Offres.AnyAsync())
            {
                logger.LogInformation("La base de données contient déjà des données");
                return;
            }

            logger.LogInformation("Insertion des données initiales...");

            // ============================================
            // SEED DES OFFRES
            // ============================================
            var now = DateTime.UtcNow;
            var offres = new List<Offre>
            {
                new Offre
                {
                    Type = "solo",
                    Nom = "Offre Solo",
                    Description = "Accès pour 1 personne à une épreuve olympique",
                    Prix = 75.00m,
                    NombrePersonnes = 1,
                    EstActif = true,
                    DateCreation = now,
                    Caracteristiques = "{\"avantages\":[\"1 billet\",\"Accès standard\",\"Programme officiel\"]}"
                },
                new Offre
                {
                    Type = "duo",
                    Nom = "Offre Duo",
                    Description = "Accès pour 2 personnes - Économie de 20€",
                    Prix = 130.00m,
                    NombrePersonnes = 2,
                    EstActif = true,
                    DateCreation = now,
                    Caracteristiques = "{\"avantages\":[\"2 billets\",\"Économie de 20€\",\"Places côte à côte\",\"Programme officiel x2\"]}"
                },
                new Offre
                {
                    Type = "famille",
                    Nom = "Offre Famille",
                    Description = "Accès pour 4 personnes (2 adultes + 2 enfants) - Économie de 80€",
                    Prix = 220.00m,
                    NombrePersonnes = 4,
                    EstActif = true,
                    DateCreation = now,
                    Caracteristiques = "{\"avantages\":[\"4 billets\",\"Économie de 80€\",\"Places groupées\",\"Kit famille offert\",\"Programme officiel x4\"]}"
                }
            };

            context.Offres.AddRange(offres);
            await context.SaveChangesAsync();
            logger.LogInformation("Offres créées: {Count}", offres.Count);

            // ============================================
            // SEED D'UN UTILISATEUR TEST
            // ============================================
            var utilisateurTest = new Utilisateur
            {
                Prenom = "Test",
                Nom = "Utilisateur",
                Email = "test@jo2024.fr",
                MotDePasseHash = BCrypt.Net.BCrypt.HashPassword("Test@123"),
                Role = "Utilisateur",
                DateCreation = now,
                EstActif = true
            };

            context.Utilisateurs.Add(utilisateurTest);
            await context.SaveChangesAsync();
            logger.LogInformation("Utilisateur test créé: {Email}", utilisateurTest.Email);

            // ============================================
            // SEED D'UN ADMIN
            // ============================================
            var admin = new Utilisateur
            {
                Prenom = "Admin",
                Nom = "JO2024",
                Email = "admin@jo2024.fr",
                MotDePasseHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = "Admin",
                DateCreation = now,
                EstActif = true
            };

            context.Utilisateurs.Add(admin);
            await context.SaveChangesAsync();
            logger.LogInformation("Administrateur créé: {Email}", admin.Email);

            // ============================================
            // SEED D'UNE COMMANDE EXEMPLE (optionnel)
            // ============================================
            var offreSolo = await context.Offres.FirstAsync(o => o.Type == "solo");
            
            var commandeTest = new Commande
            {
                Numero = $"CMD-{DateTime.UtcNow:yyyyMMdd}-00001",
                UtilisateurId = utilisateurTest.Id,
                DateAchat = now,
                MontantHT = 62.50m,
                MontantTVA = 12.50m,
                MontantTotal = 75.00m,
                Statut = "Payée",
                MethodePaiement = "Carte bancaire"
            };

            context.Commandes.Add(commandeTest);
            await context.SaveChangesAsync();

            var commandeItem = new CommandeItem
            {
                CommandeId = commandeTest.Id,
                OffreId = offreSolo.Id,
                Quantite = 1,
                PrixUnitaire = 75.00m,
                PrixTotal = 75.00m,
                Sport = "Athlétisme"
            };

            context.CommandeItems.Add(commandeItem);
            await context.SaveChangesAsync();

            var billet = new Billet
            {
                Numero = "JO2024-ATHLETISME-00001",
                CommandeId = commandeTest.Id,
                UtilisateurId = utilisateurTest.Id,
                Titre = "Athlétisme - Finale 100m",
                Sport = "Athlétisme",
                Lieu = "Stade de France",
                DateEpreuve = new DateTime(2024, 8, 4, 20, 0, 0, DateTimeKind.Utc),
                Place = "Tribune Nord - Rang 15 - Siège 42",
                Statut = "Actif",
                CodeQR = "QR-JO2024-ATH-00001",
                DateCreation = now
            };

            context.Billets.Add(billet);
            await context.SaveChangesAsync();

            logger.LogInformation("Commande et billet exemples créés");
            logger.LogInformation("Initialisation de la base de données terminée avec succès");
            logger.LogInformation("Statistiques:");
            logger.LogInformation("   - Offres: {OffresCount}", await context.Offres.CountAsync());
            logger.LogInformation("   - Utilisateurs: {UsersCount}", await context.Utilisateurs.CountAsync());
            logger.LogInformation("   - Commandes: {CommandesCount}", await context.Commandes.CountAsync());
            logger.LogInformation("   - Billets: {BilletsCount}", await context.Billets.CountAsync());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de l'initialisation de la base de données");
            throw;
        }
    }
}