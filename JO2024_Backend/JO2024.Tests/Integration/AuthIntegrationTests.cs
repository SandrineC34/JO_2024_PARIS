// ============================================
// Tests d'Intégration pour AuthService
// JO2024.Tests/Integration/AuthIntegrationTests.cs
// ============================================

using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using JO2024.Core.Services;
using JO2024.Core.Interfaces;
using JO2024.Core.DTOs.Auth;
using JO2024.Core.Entities;
using JO2024.Infrastructure.Data;
using JO2024.Infrastructure.Repositories;

namespace JO2024.Tests.Integration;

/// <summary>
/// Tests d'intégration avec une vraie base de données InMemory
/// Teste les interactions réelles entre AuthService, Repository et DbContext
/// </summary>
public class AuthIntegrationTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IUtilisateurRepository _userRepository;
    private readonly AuthService _authService;
    private readonly Mock<IEmailService> _mockEmailService;

    public AuthIntegrationTests()
    {
        // Configuration de la base de données InMemory
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _userRepository = new UtilisateurRepository(_context);

        // Configuration mock
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["Jwt:Key"])
            .Returns("TestKeyForJWT123456789012345678901234567890");

        _mockEmailService = new Mock<IEmailService>();
        _mockEmailService.Setup(e => e.SendEmailAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var mockLogger = new Mock<ILogger<AuthService>>();

        _authService = new AuthService(
            _userRepository,
            mockConfig.Object,
            _mockEmailService.Object,
            mockLogger.Object
        );
    }

    // ============================================
    // TESTS D'INSCRIPTION COMPLÈTE
    // ============================================

    [Fact]
    public async Task RegisterAsync_Integration_ShouldCreateUserInDatabase()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Prenom = "Jean",
            Nom = "Dupont",
            Email = "jean.dupont@integration.test",
            Password = "Password123!",
            NewsletterPreferences = null
        };

        // Act
        var result = await _authService.RegisterAsync(registerDto);

        // Assert - Vérifier la réponse
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Token.Should().NotBeNullOrEmpty();
        result.User.Should().NotBeNull();

        // Assert - Vérifier en base de données
        var userInDb = await _context.Utilisateurs
            .FirstOrDefaultAsync(u => u.Email == registerDto.Email.ToLower());
        
        userInDb.Should().NotBeNull();
        userInDb!.Prenom.Should().Be(registerDto.Prenom);
        userInDb.Nom.Should().Be(registerDto.Nom);
        userInDb.Email.Should().Be(registerDto.Email.ToLower());
        userInDb.EstActif.Should().BeTrue();
        userInDb.Role.Should().Be("Utilisateur");
        userInDb.NewsletterAbonne.Should().BeFalse();

        // Vérifier que le mot de passe est bien hashé
        BCrypt.Net.BCrypt.Verify(registerDto.Password, userInDb.MotDePasseHash)
            .Should().BeTrue();
    }

    [Fact]
    public async Task RegisterAsync_WithNewsletterSubscription_ShouldSavePreferencesInDatabase()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Prenom = "Marie",
            Nom = "Martin",
            Email = "marie.martin@integration.test",
            Password = "Password123!",
            NewsletterPreferences = new NewsletterPreferencesDto
            {
                Subscribed = true,
                Categories = new NewsletterCategoriesDto
                {
                    Sport = true,
                    Evenements = false,
                    Billets = true
                },
                Sports = new List<SportPreferenceDto>
                {
                    new SportPreferenceDto { Id = "natation", Name = "Natation" },
                    new SportPreferenceDto { Id = "athletisme", Name = "Athlétisme" }
                }
            }
        };

        // Act
        var result = await _authService.RegisterAsync(registerDto);

        // Assert
        result.Success.Should().BeTrue();

        var userInDb = await _context.Utilisateurs
            .FirstOrDefaultAsync(u => u.Email == registerDto.Email.ToLower());

        userInDb.Should().NotBeNull();
        userInDb!.NewsletterAbonne.Should().BeTrue();
        userInDb.NewsletterCategories.Should().NotBeNullOrEmpty();
        userInDb.NewsletterSports.Should().NotBeNullOrEmpty();

        // Vérifier le contenu JSON
        userInDb.NewsletterCategories.Should().Contain("Sport");
        userInDb.NewsletterCategories.Should().Contain("Billets");
        userInDb.NewsletterSports.Should().Contain("Natation");        
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateEmail_ShouldFailAndNotCreateUser()
    {
        // Arrange - Créer un premier utilisateur
        var firstUser = new RegisterDto
        {
            Prenom = "Premier",
            Nom = "Utilisateur",
            Email = "duplicate@test.com",
            Password = "Password123!"
        };

        await _authService.RegisterAsync(firstUser);

        // Tenter de créer un deuxième utilisateur avec le même email
        var duplicateUser = new RegisterDto
        {
            Prenom = "Deuxieme",
            Nom = "Utilisateur",
            Email = "duplicate@test.com",
            Password = "DifferentPassword123!"
        };

        // Act
        var result = await _authService.RegisterAsync(duplicateUser);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("déjà utilisée");

        // Vérifier qu'il n'y a qu'un seul utilisateur en base
        var usersCount = await _context.Utilisateurs
            .CountAsync(u => u.Email == "duplicate@test.com");
        
        usersCount.Should().Be(1);
    }

    // ============================================
    // TESTS DE CONNEXION COMPLÈTE
    // ============================================

    [Fact]
    public async Task LoginAsync_Integration_WithValidCredentials_ShouldUpdateLastConnection()
    {
        // Arrange - Créer un utilisateur
        var registerDto = new RegisterDto
        {
            Prenom = "Test",
            Nom = "Login",
            Email = "login.test@integration.test",
            Password = "Password123!"
        };

        await _authService.RegisterAsync(registerDto);

        var loginDto = new LoginDto
        {
            Email = registerDto.Email,
            Password = registerDto.Password
        };

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        result.Success.Should().BeTrue();
        result.Token.Should().NotBeNullOrEmpty();

        // Vérifier que DerniereConnexion a été mise à jour
        var userInDb = await _context.Utilisateurs
            .FirstOrDefaultAsync(u => u.Email == registerDto.Email.ToLower());

        userInDb.Should().NotBeNull();
        userInDb!.DerniereConnexion.Should().NotBeNull();
        userInDb.DerniereConnexion.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task LoginAsync_Integration_WithInvalidPassword_ShouldFail()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Prenom = "Test",
            Nom = "InvalidPassword",
            Email = "invalid.password@test.com",
            Password = "CorrectPassword123!"
        };

        await _authService.RegisterAsync(registerDto);

        var loginDto = new LoginDto
        {
            Email = registerDto.Email,
            Password = "WrongPassword123!"
        };

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("incorrect");
        result.Token.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task LoginAsync_Integration_WithInactiveAccount_ShouldFail()
    {
        // Arrange - Créer et désactiver un compte
        var registerDto = new RegisterDto
        {
            Prenom = "Test",
            Nom = "Inactive",
            Email = "inactive@test.com",
            Password = "Password123!"
        };

        await _authService.RegisterAsync(registerDto);

        // Désactiver le compte
        var user = await _context.Utilisateurs
            .FirstAsync(u => u.Email == registerDto.Email.ToLower());
        user.EstActif = false;
        await _context.SaveChangesAsync();

        var loginDto = new LoginDto
        {
            Email = registerDto.Email,
            Password = registerDto.Password
        };

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("désactivé");
    }

    // ============================================
    // TESTS DE CHANGEMENT DE MOT DE PASSE
    // ============================================

    [Fact]
    public async Task ChangePasswordAsync_Integration_ShouldUpdatePasswordInDatabase()
    {
        // Arrange - Créer un utilisateur
        var registerDto = new RegisterDto
        {
            Prenom = "Test",
            Nom = "ChangePassword",
            Email = "change.password@test.com",
            Password = "OldPassword123!"
        };

        var registerResult = await _authService.RegisterAsync(registerDto);
        var userId = registerResult.User!.Id;

        var changePasswordDto = new ChangePasswordDto
        {
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword123!"
        };

        // Act
        var result = await _authService.ChangePasswordAsync(userId, changePasswordDto);

        // Assert
        result.Should().BeTrue();

        // Vérifier que l'ancien mot de passe ne fonctionne plus
        var loginWithOld = await _authService.LoginAsync(new LoginDto
        {
            Email = registerDto.Email,
            Password = "OldPassword123!"
        });
        loginWithOld.Success.Should().BeFalse();

        // Vérifier que le nouveau mot de passe fonctionne
        var loginWithNew = await _authService.LoginAsync(new LoginDto
        {
            Email = registerDto.Email,
            Password = "NewPassword123!"
        });
        loginWithNew.Success.Should().BeTrue();
    }

    // ============================================
    // TESTS DE RÉINITIALISATION DE MOT DE PASSE
    // ============================================

    [Fact]
    public async Task ResetPassword_Integration_CompletFlow_ShouldWork()
    {
        // Arrange - Créer un utilisateur
        var registerDto = new RegisterDto
        {
            Prenom = "Test",
            Nom = "ResetPassword",
            Email = "reset.password@test.com",
            Password = "OldPassword123!"
        };

        await _authService.RegisterAsync(registerDto);

        // Étape 1 : Demander la réinitialisation
        var requestResult = await _authService.ResetPasswordRequestAsync(registerDto.Email);
        requestResult.Should().BeTrue();

        // Récupérer le token en base
        var user = await _context.Utilisateurs
            .FirstAsync(u => u.Email == registerDto.Email.ToLower());
        
        user.ResetPasswordToken.Should().NotBeNullOrEmpty();
        user.ResetPasswordExpiry.Should().NotBeNull();

        // Étape 2 : Réinitialiser avec le token
        var resetDto = new ResetPasswordDto
        {
            Token = user.ResetPasswordToken!,
            Email = registerDto.Email,
            NewPassword = "NewResetPassword123!"
        };

        var resetResult = await _authService.ResetPasswordAsync(resetDto);
        resetResult.Should().BeTrue();

        // Vérifier que le token a été supprimé
        await _context.Entry(user).ReloadAsync();
        user.ResetPasswordToken.Should().BeNull();
        user.ResetPasswordExpiry.Should().BeNull();

        // Vérifier que la connexion fonctionne avec le nouveau mot de passe
        var loginResult = await _authService.LoginAsync(new LoginDto
        {
            Email = registerDto.Email,
            Password = "NewResetPassword123!"
        });

        loginResult.Success.Should().BeTrue();
    }

    // ============================================
    // TESTS DE SCÉNARIOS MULTIPLES
    // ============================================

    [Fact]
    public async Task Integration_MultipleUsersRegistration_ShouldWorkIndependently()
    {
        // Arrange & Act - Créer plusieurs utilisateurs
        var users = new[]
        {
            new RegisterDto { Prenom = "User1", Nom = "Test", Email = "user1@test.com", Password = "Pass1234!" },
            new RegisterDto { Prenom = "User2", Nom = "Test", Email = "user2@test.com", Password = "Pass1234!" },
            new RegisterDto { Prenom = "User3", Nom = "Test", Email = "user3@test.com", Password = "Pass1234!" }
        };

        var results = new List<AuthResponseDto>();
        foreach (var user in users)
        {
            var result = await _authService.RegisterAsync(user);
            results.Add(result);
        }

        // Assert
        results.Should().AllSatisfy(r => r.Success.Should().BeTrue());

        var dbUsers = await _context.Utilisateurs.ToListAsync();
        dbUsers.Should().HaveCount(3);
        dbUsers.Select(u => u.Email).Should().Contain("user1@test.com");
        dbUsers.Select(u => u.Email).Should().Contain("user2@test.com");
        dbUsers.Select(u => u.Email).Should().Contain("user3@test.com");
    }

    [Fact]
    public async Task Integration_RegisterLoginAndChangePassword_CompleteUserFlow()
    {
        // Scénario complet d'un utilisateur

        // 1. Inscription
        var registerDto = new RegisterDto
        {
            Prenom = "Complete",
            Nom = "Flow",
            Email = "complete.flow@test.com",
            Password = "Initial123!",
            NewsletterPreferences = new NewsletterPreferencesDto
            {
                Subscribed = true,
                Categories = new NewsletterCategoriesDto { Sport = true }
            }
        };

        var registerResult = await _authService.RegisterAsync(registerDto);
        registerResult.Success.Should().BeTrue();

        // 2. Connexion
        var loginResult = await _authService.LoginAsync(new LoginDto
        {
            Email = registerDto.Email,
            Password = registerDto.Password
        });
        loginResult.Success.Should().BeTrue();

        // 3. Changement de mot de passe
        var changeResult = await _authService.ChangePasswordAsync(
            registerResult.User!.Id,
            new ChangePasswordDto
            {
                CurrentPassword = "Initial123!",
                NewPassword = "Updated123!"
            });
        changeResult.Should().BeTrue();

        // 4. Connexion avec nouveau mot de passe
        var loginWithNewPassword = await _authService.LoginAsync(new LoginDto
        {
            Email = registerDto.Email,
            Password = "Updated123!"
        });
        loginWithNewPassword.Success.Should().BeTrue();

        // Vérifier l'état final en base
        var finalUser = await _context.Utilisateurs
            .FirstAsync(u => u.Email == registerDto.Email.ToLower());

        finalUser.NewsletterAbonne.Should().BeTrue();
        finalUser.DerniereConnexion.Should().NotBeNull();
        BCrypt.Net.BCrypt.Verify("Updated123!", finalUser.MotDePasseHash).Should().BeTrue();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}