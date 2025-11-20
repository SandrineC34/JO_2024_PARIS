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

// ########################################""
// Test 
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

   
    // ############################################
    // Test du catch

    [Fact]
public async Task RegisterAsync_WhenExceptionThrown_ShouldReturnFailure()
{
    // Arrange
    var dto = new RegisterDto
    {
        Prenom = "Test",
        Nom = "User",
        Email = "test@test.com",
        Password = "Password123!"
    };

    _mockUserRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
                 .ThrowsAsync(new Exception("DB error"));

    // Act
    var result = await _authService.RegisterAsync(dto);

    // Assert
    result.Success.Should().BeFalse();
    result.Message.Should().Contain("erreur");
}
    /// #######################################
    // Test echec d'envoi email (Catch interne)

    [Fact]
public async Task RegisterAsync_WhenEmailSendingFails_ShouldStillReturnSuccess()
{
    // Arrange
    _mockUserRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
        .ReturnsAsync((Utilisateur?)null);

    _mockUserRepo.Setup(r => r.AddAsync(It.IsAny<Utilisateur>()))
        .Returns(Task.CompletedTask);

    _mockUserRepo.Setup(r => r.SaveChangesAsync())
        .ReturnsAsync(1);

    _mockEmailService.Setup(e => e.SendEmailAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
        .ThrowsAsync(new Exception("SMTP error"));

    var dto = new RegisterDto
    {
        Prenom = "Jean",
        Nom = "Dupont",
        Email = "test@test.com",
        Password = "Password123!"
    };

    // Act
    var result = await _authService.RegisterAsync(dto);

    // Assert
    result.Success.Should().BeTrue();
}

// ############################""
// Test exception

[Fact]
public async Task LoginAsync_WhenExceptionThrown_ShouldReturnFailure()
{
    var dto = new LoginDto { Email = "test@test.com", Password = "123" };

    _mockUserRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
                 .ThrowsAsync(new Exception("DB error"));

    var result = await _authService.LoginAsync(dto);

    result.Success.Should().BeFalse();
}

// ###############################
// Test user inexistant

[Fact]
public async Task GetCurrentUserAsync_WhenUserNotFound_ShouldThrow()
{
    _mockUserRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                 .ReturnsAsync((Utilisateur?)null);

    await Assert.ThrowsAsync<Exception>(() => _authService.GetCurrentUserAsync(999));
}

// #####################
// Test utilisateur inexistant

[Fact]
public async Task ChangePasswordAsync_WhenUserNotFound_ShouldReturnFalse()
{
    _mockUserRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                 .ReturnsAsync((Utilisateur?)null);

    var dto = new ChangePasswordDto
    {
        CurrentPassword = "Old123!",
        NewPassword = "New123!"
    };

    var result = await _authService.ChangePasswordAsync(1, dto);

    result.Should().BeFalse();
}

[Fact]
public async Task ChangePasswordAsync_WhenExceptionThrown_ShouldReturnFalse()
{
    _mockUserRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                 .ThrowsAsync(new Exception("DB error"));

    var dto = new ChangePasswordDto
    {
        CurrentPassword = "123",
        NewPassword = "456"
    };

    var result = await _authService.ChangePasswordAsync(1, dto);

    result.Should().BeFalse();
}

[Fact]
public async Task ResetPasswordRequestAsync_WhenExceptionThrown_ShouldReturnFalse()
{
    _mockUserRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
                 .ThrowsAsync(new Exception("DB error"));

    var result = await _authService.ResetPasswordRequestAsync("test@test.com");

    result.Should().BeFalse();
}

[Fact]
public async Task ResetPasswordAsync_WhenTokenInvalid_ShouldReturnFalse()
{
    _mockUserRepo.Setup(r => r.GetByResetTokenAsync(It.IsAny<string>()))
                 .ReturnsAsync((Utilisateur?)null);

    var dto = new ResetPasswordDto { Token = "abc", NewPassword = "123" };

    var result = await _authService.ResetPasswordAsync(dto);

    result.Should().BeFalse();
}

[Fact]
public async Task ResetPasswordAsync_WhenTokenExpired_ShouldReturnFalse()
{
    var user = new Utilisateur
    {
        ResetPasswordToken = "abc",
        ResetPasswordExpiry = DateTime.UtcNow.AddMinutes(-10) // expiré
    };

    _mockUserRepo.Setup(r => r.GetByResetTokenAsync("abc"))
                 .ReturnsAsync(user);

    var dto = new ResetPasswordDto { Token = "abc", NewPassword = "123" };

    var result = await _authService.ResetPasswordAsync(dto);

    result.Should().BeFalse();
}

[Fact]
public async Task ResetPasswordAsync_WhenValid_ShouldReturnTrue()
{
    var user = new Utilisateur
    {
        ResetPasswordToken = "abc",
        ResetPasswordExpiry = DateTime.UtcNow.AddMinutes(30)
    };

    _mockUserRepo.Setup(r => r.GetByResetTokenAsync("abc"))
                 .ReturnsAsync(user);

    _mockUserRepo.Setup(r => r.UpdateAsync(It.IsAny<Utilisateur>()))
                 .Returns(Task.CompletedTask);

    _mockUserRepo.Setup(r => r.SaveChangesAsync())
                 .ReturnsAsync(1);

    var dto = new ResetPasswordDto { Token = "abc", NewPassword = "New123!" };

    var result = await _authService.ResetPasswordAsync(dto);

    result.Should().BeTrue();
}

[Fact]
public async Task ResetPasswordAsync_WhenExceptionThrown_ShouldReturnFalse()
{
    _mockUserRepo.Setup(r => r.GetByResetTokenAsync(It.IsAny<string>()))
                 .ThrowsAsync(new Exception("DB error"));

    var dto = new ResetPasswordDto { Token = "abc", NewPassword = "123" };

    var result = await _authService.ResetPasswordAsync(dto);

    result.Should().BeFalse();
}

[Fact]
public void GenerateJwtToken_ShouldProduceValidToken()
{
    var user = new Utilisateur
    {
        Id = 1,
        Email = "test@test.com",
        Role = "User"
    };

    var token = typeof(AuthService)
        .GetMethod("GenerateJwtToken", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
        .Invoke(_authService, new object?[] { user }) as string;

    token.Should().NotBeNullOrEmpty();
}

/// ############################
/// test pour forcer l’exception d’envoi emai
[Fact]
public async Task ChangePasswordAsync_WhenEmailSendingFails_ShouldStillReturnTrue()
{
    // Arrange
    var currentPassword = "OldPassword123!";
    var user = new Utilisateur
    {
        Id = 1,
        Email = "test@example.com",
        MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(currentPassword)
    };

    var dto = new ChangePasswordDto
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

    // ICI : on force l’exception email
    _mockEmailService.Setup(e => e.SendEmailAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
        .ThrowsAsync(new Exception("SMTP error"));

    // Act
    var result = await _authService.ChangePasswordAsync(user.Id, dto);

    // Assert
    result.Should().BeTrue(); // Retourne TRUE malgré erreur email
}




}
    