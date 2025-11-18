// ============================================
// Tests Unitaires pour AuthService
// ============================================

using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using JO2024.Core.Services;
using JO2024.Core.Interfaces;
using JO2024.Core.DTOs.Auth;
using JO2024.Core.Entities;

namespace JO2024.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUtilisateurRepository> _mockUserRepo;
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<ILogger<AuthService>> _mockLogger;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mockUserRepo = new Mock<IUtilisateurRepository>();
        _mockConfig = new Mock<IConfiguration>();
        _mockEmailService = new Mock<IEmailService>();
        _mockLogger = new Mock<ILogger<AuthService>>();

        // Configuration JWT
        _mockConfig.Setup(c => c["Jwt:Key"]).Returns("TestKeyForJWT123456789012345678901234567890");

        _authService = new AuthService(
            _mockUserRepo.Object,
            _mockConfig.Object,
            _mockEmailService.Object,
            _mockLogger.Object
        );
    }

    // ============================================
    // TESTS D'INSCRIPTION (RegisterAsync)
    // ============================================

    [Fact]
    public async Task RegisterAsync_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Prenom = "Jean",
            Nom = "Dupont",
            Email = "jean.dupont@test.com",
            Password = "Password123!",
            NewsletterPreferences = null
        };

        _mockUserRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((Utilisateur?)null);

        _mockUserRepo.Setup(r => r.AddAsync(It.IsAny<Utilisateur>()))
            .Returns(Task.CompletedTask);

        _mockUserRepo.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        _mockEmailService.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _authService.RegisterAsync(registerDto);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Token.Should().NotBeNullOrEmpty();
        result.User.Should().NotBeNull();
        result.User!.Email.Should().Be(registerDto.Email.ToLower());
        result.User.Prenom.Should().Be(registerDto.Prenom);
        result.User.Nom.Should().Be(registerDto.Nom);

        _mockUserRepo.Verify(r => r.AddAsync(It.IsAny<Utilisateur>()), Times.Once);
        _mockUserRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        _mockEmailService.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ShouldReturnFailure()
    {
        // Arrange
        var existingUser = new Utilisateur
        {
            Id = 1,
            Email = "existing@test.com",
            Prenom = "Existing",
            Nom = "User",
            MotDePasseHash = BCrypt.Net.BCrypt.HashPassword("Password123!")
        };

        var registerDto = new RegisterDto
        {
            Prenom = "Jean",
            Nom = "Dupont",
            Email = "existing@test.com",
            Password = "Password123!"
        };

        _mockUserRepo.Setup(r => r.GetByEmailAsync(registerDto.Email))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _authService.RegisterAsync(registerDto);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("déjà utilisée");
        result.Token.Should().BeNullOrEmpty();
        result.User.Should().BeNull();

        _mockUserRepo.Verify(r => r.AddAsync(It.IsAny<Utilisateur>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WithNewsletterSubscription_ShouldSavePreferences()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Prenom = "Marie",
            Nom = "Martin",
            Email = "marie.martin@test.com",
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

        _mockUserRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((Utilisateur?)null);

        Utilisateur? capturedUser = null;
        _mockUserRepo.Setup(r => r.AddAsync(It.IsAny<Utilisateur>()))
            .Callback<Utilisateur>(u => capturedUser = u)
            .Returns(Task.CompletedTask);

        _mockUserRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _authService.RegisterAsync(registerDto);

        // Assert
        result.Success.Should().BeTrue();
        capturedUser.Should().NotBeNull();
        capturedUser!.NewsletterAbonne.Should().BeTrue();
        capturedUser.NewsletterCategories.Should().NotBeNullOrEmpty();
        capturedUser.NewsletterSports.Should().NotBeNullOrEmpty();
    }

    // ============================================
    // TESTS DE CONNEXION (LoginAsync)
    // ============================================

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnSuccess()
    {
        // Arrange
        var password = "Password123!";
        var user = new Utilisateur
        {
            Id = 1,
            Email = "test@example.com",
            Prenom = "Test",
            Nom = "User",
            MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(password),
            EstActif = true
        };

        var loginDto = new LoginDto
        {
            Email = "test@example.com",
            Password = password
        };

        _mockUserRepo.Setup(r => r.GetByEmailAsync(loginDto.Email))
            .ReturnsAsync(user);

        _mockUserRepo.Setup(r => r.UpdateAsync(It.IsAny<Utilisateur>()))
            .Returns(Task.CompletedTask);

        _mockUserRepo.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Token.Should().NotBeNullOrEmpty();
        result.User.Should().NotBeNull();
        result.User!.Email.Should().Be(user.Email);

        _mockUserRepo.Verify(r => r.UpdateAsync(It.IsAny<Utilisateur>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidEmail_ShouldReturnFailure()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "nonexistent@test.com",
            Password = "Password123!"
        };

        _mockUserRepo.Setup(r => r.GetByEmailAsync(loginDto.Email))
            .ReturnsAsync((Utilisateur?)null);

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("incorrect");
        result.Token.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldReturnFailure()
    {
        // Arrange
        var user = new Utilisateur
        {
            Id = 1,
            Email = "test@example.com",
            Prenom = "Test",
            Nom = "User",
            MotDePasseHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword123!"),
            EstActif = true
        };

        var loginDto = new LoginDto
        {
            Email = "test@example.com",
            Password = "WrongPassword123!"
        };

        _mockUserRepo.Setup(r => r.GetByEmailAsync(loginDto.Email))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("incorrect");
        _mockUserRepo.Verify(r => r.UpdateAsync(It.IsAny<Utilisateur>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WithInactiveAccount_ShouldReturnFailure()
    {
        // Arrange
        var user = new Utilisateur
        {
            Id = 1,
            Email = "test@example.com",
            Prenom = "Test",
            Nom = "User",
            MotDePasseHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            EstActif = false
        };

        var loginDto = new LoginDto
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        _mockUserRepo.Setup(r => r.GetByEmailAsync(loginDto.Email))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("désactivé");
    }

    // ============================================
    // TESTS DE CHANGEMENT DE MOT DE PASSE
    // ============================================

    [Fact]
    public async Task ChangePasswordAsync_WithValidCurrentPassword_ShouldReturnTrue()
    {
        // Arrange
        var currentPassword = "OldPassword123!";
        var user = new Utilisateur
        {
            Id = 1,
            Email = "test@example.com",
            MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(currentPassword)
        };

        var changePasswordDto = new ChangePasswordDto
        {
            CurrentPassword = currentPassword,
            NewPassword = "NewPassword123!"
        };

        _mockUserRepo.Setup(r => r.GetByIdAsync(user.Id))
            .ReturnsAsync(user);

        _mockUserRepo.Setup(r => r.UpdateAsync(It.IsAny<Utilisateur>()))
            .Returns(Task.CompletedTask);

        _mockUserRepo.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _authService.ChangePasswordAsync(user.Id, changePasswordDto);

        // Assert
        result.Should().BeTrue();
        _mockUserRepo.Verify(r => r.UpdateAsync(It.IsAny<Utilisateur>()), Times.Once);
        _mockEmailService.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ChangePasswordAsync_WithInvalidCurrentPassword_ShouldReturnFalse()
    {
        // Arrange
        var user = new Utilisateur
        {
            Id = 1,
            Email = "test@example.com",
            MotDePasseHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword123!")
        };

        var changePasswordDto = new ChangePasswordDto
        {
            CurrentPassword = "WrongPassword123!",
            NewPassword = "NewPassword123!"
        };

        _mockUserRepo.Setup(r => r.GetByIdAsync(user.Id))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.ChangePasswordAsync(user.Id, changePasswordDto);

        // Assert
        result.Should().BeFalse();
        _mockUserRepo.Verify(r => r.UpdateAsync(It.IsAny<Utilisateur>()), Times.Never);
    }

    // ============================================
    // TESTS DE RÉINITIALISATION DE MOT DE PASSE
    // ============================================

    [Fact]
    public async Task ResetPasswordRequestAsync_WithExistingEmail_ShouldSendEmailAndReturnTrue()
    {
        // Arrange
        var user = new Utilisateur
        {
            Id = 1,
            Email = "test@example.com",
            Prenom = "Test",
            Nom = "User"
        };

        _mockUserRepo.Setup(r => r.GetByEmailAsync(user.Email))
            .ReturnsAsync(user);

        _mockUserRepo.Setup(r => r.UpdateAsync(It.IsAny<Utilisateur>()))
            .Returns(Task.CompletedTask);

        _mockUserRepo.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _authService.ResetPasswordRequestAsync(user.Email);

        // Assert
        result.Should().BeTrue();
        _mockUserRepo.Verify(r => r.UpdateAsync(It.IsAny<Utilisateur>()), Times.Once);
        _mockEmailService.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ResetPasswordRequestAsync_WithNonExistingEmail_ShouldReturnTrueWithoutSendingEmail()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((Utilisateur?)null);

        // Act
        var result = await _authService.ResetPasswordRequestAsync("nonexistent@test.com");

        // Assert
        result.Should().BeTrue(); // Pour ne pas révéler si l'email existe
        _mockEmailService.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ResetPasswordAsync_WithValidToken_ShouldReturnTrue()
    {
        // Arrange
        var token = "valid-reset-token";
        var user = new Utilisateur
        {
            Id = 1,
            Email = "test@example.com",
            Prenom = "Test",
            Nom = "User",
            ResetPasswordToken = token,
            ResetPasswordExpiry = DateTime.UtcNow.AddHours(1)
        };

        var resetPasswordDto = new ResetPasswordDto
        {
            Token = token,
            Email = user.Email,
            NewPassword = "NewPassword123!"
        };

        _mockUserRepo.Setup(r => r.GetByResetTokenAsync(token))
            .ReturnsAsync(user);

        _mockUserRepo.Setup(r => r.UpdateAsync(It.IsAny<Utilisateur>()))
            .Returns(Task.CompletedTask);

        _mockUserRepo.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _authService.ResetPasswordAsync(resetPasswordDto);

        // Assert
        result.Should().BeTrue();
        _mockUserRepo.Verify(r => r.UpdateAsync(It.IsAny<Utilisateur>()), Times.Once);
    }

    [Fact]
    public async Task ResetPasswordAsync_WithExpiredToken_ShouldReturnFalse()
    {
        // Arrange
        var token = "expired-token";
        var user = new Utilisateur
        {
            Id = 1,
            Email = "test@example.com",
            ResetPasswordToken = token,
            ResetPasswordExpiry = DateTime.UtcNow.AddHours(-1) // Expiré
        };

        var resetPasswordDto = new ResetPasswordDto
        {
            Token = token,
            Email = user.Email,
            NewPassword = "NewPassword123!"
        };

        _mockUserRepo.Setup(r => r.GetByResetTokenAsync(token))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.ResetPasswordAsync(resetPasswordDto);

        // Assert
        result.Should().BeFalse();
        _mockUserRepo.Verify(r => r.UpdateAsync(It.IsAny<Utilisateur>()), Times.Never);
    }

    // ============================================
    // TESTS DE RÉCUPÉRATION D'UTILISATEUR
    // ============================================

    [Fact]
    public async Task GetCurrentUserAsync_WithValidUserId_ShouldReturnUser()
    {
        // Arrange
        var user = new Utilisateur
        {
            Id = 1,
            Email = "test@example.com",
            Prenom = "Test",
            Nom = "User"
        };

        _mockUserRepo.Setup(r => r.GetByIdAsync(user.Id))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.GetCurrentUserAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
        result.Email.Should().Be(user.Email);
        result.Prenom.Should().Be(user.Prenom);
        result.Nom.Should().Be(user.Nom);
    }

    [Fact]
    public async Task GetCurrentUserAsync_WithInvalidUserId_ShouldThrowException()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Utilisateur?)null);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(async () => 
            await _authService.GetCurrentUserAsync(999));
    }
}