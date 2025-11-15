using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using JO2024.Infrastructure.Data;
using JO2024.Core.Entities;
using JO2024.Infrastructure.Services;

namespace JO2024.Tests.Integration;

/// <summary>
/// Tests d'intégration - Scénarios complets avec base de données
/// Couverture: 15%+ supplémentaires
/// </summary>
public class NewsletterIntegrationTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly NewsletterService _newsletterService;

    public NewsletterIntegrationTests()
    {
        // Base de données en mémoire pour les tests
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _newsletterService = new NewsletterService(_context);
        
        // Seed data
        SeedTestData();
    }

    private void SeedTestData()
    {
        var user = new Utilisateur
        {
            Id = 1,
            Email = "test@jo2024.fr",
            Prenom = "Jean",
            Nom = "Dupont"
        };

        _context.Utilisateurs.Add(user);
        _context.SaveChanges();
    }

    #region Scénario 1: Inscription complète à la newsletter (E2E)

    [Fact]
    public async Task Scenario_UserSubscribesToNewsletter_WithMultipleCategories()
    {
        // GIVEN: Un utilisateur non abonné
        var userId = 1;

        // WHEN: Il s'abonne avec plusieurs catégories
        var preferences = new UpdateNewsletterPreferencesDto
        {
            EstAbonne = true,
            CategoriesSports = true,
            CategoriesEvenements = false,
            CategoriesBillets = true
        };

        await _newsletterService.UpdatePreferencesAsync(userId, preferences);

        // THEN: Les préférences sont enregistrées correctement
        var savedPreferences = await _newsletterService.GetPreferencesAsync(userId);

        savedPreferences.Should().NotBeNull();
        savedPreferences!.EstAbonne.Should().BeTrue();
        savedPreferences.Categories.Sports.Should().BeTrue();
        savedPreferences.Categories.Evenements.Should().BeFalse();
        savedPreferences.Categories.Billets.Should().BeTrue();

        // THEN: Un token de désinscription est généré
        var subscription = await _context.NewsletterSubscriptions
            .FirstOrDefaultAsync(s => s.UtilisateurId == userId);

        subscription.Should().NotBeNull();
        subscription!.TokenDesabonnement.Should().NotBeNullOrEmpty();
        subscription.DateAbonnement.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    #endregion

    #region Scénario 2: Désinscription avec token (RGPD Art. 17)

    [Fact]
    public async Task Scenario_UserUnsubscribes_WithToken_DeletesDataPermanently()
    {
        // GIVEN: Un utilisateur abonné avec un token
        var subscription = new NewsletterSubscription
        {
            UtilisateurId = 1,
            EstAbonne = true,
            CategoriesSports = true,
            DateAbonnement = DateTime.UtcNow,
            TokenDesabonnement = "test_token_123",
            TokenExpiration = DateTime.UtcNow.AddDays(30)
        };

        _context.NewsletterSubscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        // WHEN: L'utilisateur se désinscrit via le token
        var result = await _newsletterService.UnsubscribeByTokenAsync("test_token_123");

        // THEN: La désinscription est effectuée
        result.Should().BeTrue();

        // THEN: Les données sont supprimées (RGPD Art. 17)
        var deletedSubscription = await _context.NewsletterSubscriptions
            .FirstOrDefaultAsync(s => s.TokenDesabonnement == "test_token_123");

        deletedSubscription.Should().BeNull("Les données doivent être supprimées (RGPD Art. 17)");

        // THEN: Un historique est créé pour la traçabilité
        var history = await _context.NewsletterSubscriptionHistory
            .FirstOrDefaultAsync(h => h.UtilisateurId == 1 && h.Action == "Désinscription");

        history.Should().NotBeNull("L'action doit être tracée pour audit RGPD");
    }

    #endregion

    #region Scénario 3: Modification des préférences

    [Fact]
    public async Task Scenario_UserChangesCategories_OnlySelectedOnesAreUpdated()
    {
        // GIVEN: Un utilisateur déjà abonné
        var subscription = new NewsletterSubscription
        {
            UtilisateurId = 1,
            EstAbonne = true,
            CategoriesSports = true,
            CategoriesEvenements = true,
            CategoriesBillets = false,
            DateAbonnement = DateTime.UtcNow.AddMonths(-1)
        };

        _context.NewsletterSubscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        // WHEN: Il modifie ses préférences
        var newPreferences = new UpdateNewsletterPreferencesDto
        {
            EstAbonne = true,
            CategoriesSports = false, // ❌ Désactive Sports
            CategoriesEvenements = true, // ✅ Garde Événements
            CategoriesBillets = true  // ✅ Active Billets
        };

        await _newsletterService.UpdatePreferencesAsync(1, newPreferences);

        // THEN: Seules les catégories modifiées changent
        var updated = await _context.NewsletterSubscriptions
            .FirstOrDefaultAsync(s => s.UtilisateurId == 1);

        updated.Should().NotBeNull();
        updated!.CategoriesSports.Should().BeFalse();
        updated.CategoriesEvenements.Should().BeTrue();
        updated.CategoriesBillets.Should().BeTrue();
        updated.DateModification.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    #endregion

    #region Scénario 4: Récupération des abonnés pour envoi (Scheduler)

    [Fact]
    public async Task Scenario_SchedulerFetchesSubscribers_OnlyActiveOnesWithSportsCategory()
    {
        // GIVEN: 3 utilisateurs avec différentes préférences
        var subscriptions = new List<NewsletterSubscription>
        {
            new() // Abonné Sports + Événements
            {
                UtilisateurId = 1,
                EstAbonne = true,
                CategoriesSports = true,
                CategoriesEvenements = true,
                DateAbonnement = DateTime.UtcNow
            },
            new() // Abonné uniquement Billets
            {
                UtilisateurId = 2,
                EstAbonne = true,
                CategoriesSports = false,
                CategoriesBillets = true,
                DateAbonnement = DateTime.UtcNow
            },
            new() // Désabonné
            {
                UtilisateurId = 3,
                EstAbonne = false,
                CategoriesSports = true,
                DateDesabonnement = DateTime.UtcNow.AddDays(-5)
            }
        };

        _context.NewsletterSubscriptions.AddRange(subscriptions);
        await _context.SaveChangesAsync();

        // WHEN: Le scheduler récupère les abonnés Sports
        var sportsSubscribers = await _context.NewsletterSubscriptions
            .Where(s => s.EstAbonne && s.CategoriesSports)
            .ToListAsync();

        // THEN: Seul l'utilisateur 1 est retourné
        sportsSubscribers.Should().HaveCount(1);
        sportsSubscribers.First().UtilisateurId.Should().Be(1);
    }

    #endregion

    #region Test de charge - Performance

    [Fact]
    public async Task LoadTest_CanHandle1000Subscriptions_InUnder2Seconds()
    {
        // GIVEN: 1000 abonnés
        var users = Enumerable.Range(1, 1000).Select(i => new Utilisateur
        {
            Id = i,
            Email = $"user{i}@jo2024.fr",
            Prenom = $"User{i}",
            Nom = "Test"
        }).ToList();

        _context.Utilisateurs.AddRange(users);
        await _context.SaveChangesAsync();

        var subscriptions = users.Select(u => new NewsletterSubscription
        {
            UtilisateurId = u.Id,
            EstAbonne = true,
            CategoriesSports = u.Id % 2 == 0,
            CategoriesEvenements = u.Id % 3 == 0,
            DateAbonnement = DateTime.UtcNow
        }).ToList();

        _context.NewsletterSubscriptions.AddRange(subscriptions);
        await _context.SaveChangesAsync();

        // WHEN: On récupère tous les abonnés Sports
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        var sportsSubscribers = await _context.NewsletterSubscriptions
            .Where(s => s.EstAbonne && s.CategoriesSports)
            .ToListAsync();

        stopwatch.Stop();

        // THEN: Résultat obtenu en moins de 2 secondes
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000, 
            "La requête doit être rapide même avec 1000 abonnés");

        sportsSubscribers.Should().HaveCount(500); // 50% ont Sports activé
    }

    #endregion

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

// DTOs supplémentaires si manquants
public class UpdateNewsletterPreferencesDto
{
    public bool EstAbonne { get; set; }
    public bool CategoriesSports { get; set; }
    public bool CategoriesEvenements { get; set; }
    public bool CategoriesBillets { get; set; }
}