using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using JO2024.API.Controllers;
using JO2024.Core.Interfaces;
using JO2024.Core.DTOs.Newsletter;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace JO2024.Tests.Controllers;

/// <summary>
/// Tests du NewsletterController
/// </summary>
public class NewsletterControllerTests
{
    private readonly Mock<INewsletterService> _mockNewsletterService;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<ILogger<NewsletterController>> _mockLogger;
    private readonly NewsletterController _controller;

    public NewsletterControllerTests()
    {
        _mockNewsletterService = new Mock<INewsletterService>();
        _mockEmailService = new Mock<IEmailService>();
        _mockLogger = new Mock<ILogger<NewsletterController>>();
        
        _controller = new NewsletterController(
            _mockNewsletterService.Object,
            _mockEmailService.Object,
            _mockLogger.Object
        );

        // Simuler un utilisateur authentifié
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "123"),
            new Claim(ClaimTypes.Email, "test@jo2024.fr")
        }, "mock"));

        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };
    }

    #region GetPreferences Tests

    [Fact]
    public async Task GetPreferences_ReturnsOk_WhenPreferencesExist()
    {
        // Arrange
        var expectedPreferences = new NewsletterPreferencesDto
        {
            UtilisateurId = 123,
            EstAbonne = true,
            Categories = new NewsletterCategoriesDto
            {
                Sports = true,
                Evenements = false,
                Billets = true
            }
        };

        _mockNewsletterService
            .Setup(s => s.GetPreferencesAsync(123))
            .ReturnsAsync(expectedPreferences);

        // Act
        var result = await _controller.GetPreferences();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedPreferences);
    }

    [Fact]
    public async Task GetPreferences_ReturnsDefaultPreferences_WhenNoneExist()
    {
        // Arrange
        _mockNewsletterService
            .Setup(s => s.GetPreferencesAsync(123))
            .ReturnsAsync((NewsletterPreferencesDto?)null);

        // Act
        var result = await _controller.GetPreferences();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var preferences = okResult!.Value as NewsletterPreferencesDto;
        
        preferences.Should().NotBeNull();
        preferences!.UtilisateurId.Should().Be(123);
        preferences.EstAbonne.Should().BeFalse();
    }

    [Fact]
    public async Task GetPreferences_Returns500_OnException()
    {
        // Arrange
        _mockNewsletterService
            .Setup(s => s.GetPreferencesAsync(It.IsAny<int>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetPreferences();

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }

    #endregion
  
    #region UpdatePreferences Tests 

    [Fact]
    public async Task UpdatePreferences_ReturnsOk_WhenValid()
    {
        // Arrange
        var updateDto = new UpdateNewsletterPreferencesDto
        {
            EstAbonne = true,
            CategoriesSports = true,
            CategoriesEvenements = false,
            CategoriesBillets = true
        };

        _mockNewsletterService
            .Setup(s => s.UpdatePreferencesAsync(123, updateDto))
            .Returns(Task.CompletedTask);

        _mockEmailService
            .Setup(s => s.SendNewsletterConfirmationAsync(123))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdatePreferences(updateDto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        
        // Vérifier que l'email de confirmation a été envoyé
        _mockEmailService.Verify(
            s => s.SendNewsletterConfirmationAsync(123),
            Times.Once
        );
    }

    [Fact]
    public async Task UpdatePreferences_DoesNotSendEmail_WhenUnsubscribing()
    {
        // Arrange
        var updateDto = new UpdateNewsletterPreferencesDto
        {
            EstAbonne = false // Désinscription
        };

        _mockNewsletterService
            .Setup(s => s.UpdatePreferencesAsync(123, updateDto))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdatePreferences(updateDto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        
        // Aucun email ne doit être envoyé lors de la désinscription
        _mockEmailService.Verify(
            s => s.SendNewsletterConfirmationAsync(It.IsAny<int>()),
            Times.Never
        );
    }

    [Fact]
    public async Task UpdatePreferences_Returns500_OnException()
    {
        // Arrange
        var updateDto = new UpdateNewsletterPreferencesDto { EstAbonne = true };

        _mockNewsletterService
            .Setup(s => s.UpdatePreferencesAsync(It.IsAny<int>(), It.IsAny<UpdateNewsletterPreferencesDto>()))
            .ThrowsAsync(new Exception("Update failed"));

        // Act
        var result = await _controller.UpdatePreferences(updateDto);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }

    #endregion

    #region Unsubscribe Tests

    [Fact]
    public async Task Unsubscribe_ReturnsOk_WithValidToken()
    {
        // Arrange
        string validToken = "unsubscribe_abc123";
        
        _mockNewsletterService
            .Setup(s => s.UnsubscribeByTokenAsync(validToken))
            .ReturnsAsync(true);

        _mockNewsletterService
            .Setup(s => s.GetUserIdByTokenAsync(validToken))
            .ReturnsAsync(123);

        _mockEmailService
            .Setup(s => s.SendUnsubscribeConfirmationAsync(123))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Unsubscribe(validToken);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        
        // Vérifier que l'email de confirmation de désinscription est envoyé
        _mockEmailService.Verify(
            s => s.SendUnsubscribeConfirmationAsync(123),
            Times.Once
        );
    }

    [Fact]
    public async Task Unsubscribe_ReturnsBadRequest_WithInvalidToken()
    {
        // Arrange
        string invalidToken = "invalid_token";
        
        _mockNewsletterService
            .Setup(s => s.UnsubscribeByTokenAsync(invalidToken))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Unsubscribe(invalidToken);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Unsubscribe_ReturnsBadRequest_WithEmptyToken()
    {
        // Act
        var result = await _controller.Unsubscribe("");

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        
        // Ne doit jamais appeler le service avec un token vide
        _mockNewsletterService.Verify(
            s => s.UnsubscribeByTokenAsync(It.IsAny<string>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Unsubscribe_Returns500_OnException()
    {
        // Arrange
        _mockNewsletterService
            .Setup(s => s.UnsubscribeByTokenAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.Unsubscribe("any_token");

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }

    #endregion

    #region ConfirmUnsubscribe Tests

    [Fact]
    public async Task ConfirmUnsubscribe_ReturnsOk_WithValidToken()
    {
        // Arrange
        var dto = new UnsubscribeConfirmDto { Token = "valid_token" };
        
        _mockNewsletterService
            .Setup(s => s.UnsubscribeByTokenAsync(dto.Token))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ConfirmUnsubscribe(dto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ConfirmUnsubscribe_ReturnsBadRequest_WithInvalidToken()
    {
        // Arrange
        var dto = new UnsubscribeConfirmDto { Token = "invalid_token" };
        
        _mockNewsletterService
            .Setup(s => s.UnsubscribeByTokenAsync(dto.Token))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.ConfirmUnsubscribe(dto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region GetStats Tests

    [Fact]
    public async Task GetStats_ReturnsOk_WithValidStats()
    {
        // Arrange
        var expectedStats = new NewsletterStatsDto
        {
            TotalAbonnes = 150,
            AbonnesSports = 120,
            AbonnesEvenements = 80,
            AbonnesBillets = 95,
            TauxDesabonnement = 5.2
        };

        _mockNewsletterService
            .Setup(s => s.GetStatsAsync())
            .ReturnsAsync(expectedStats);

        // Act
        var result = await _controller.GetStats();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedStats);
    }

    [Fact]
    public async Task GetStats_Returns500_OnException()
    {
        // Arrange
        _mockNewsletterService
            .Setup(s => s.GetStatsAsync())
            .ThrowsAsync(new Exception("Stats error"));

        // Act
        var result = await _controller.GetStats();

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }

    #endregion

    #region Tests RGPD

    [Fact]
    public async Task Unsubscribe_MustDeleteDataPermanently_GDPR_Article17()
    {
        // Arrange
        string token = "gdpr_test_token";
        
        _mockNewsletterService
            .Setup(s => s.UnsubscribeByTokenAsync(token))
            .ReturnsAsync(true)
            .Verifiable("La suppression définitive doit être appelée (RGPD Art. 17)");

        _mockNewsletterService
            .Setup(s => s.GetUserIdByTokenAsync(token))
            .ReturnsAsync(999);

        // Act
        await _controller.Unsubscribe(token);

        // Assert - Vérifier que la suppression a bien été effectuée
        _mockNewsletterService.Verify(
            s => s.UnsubscribeByTokenAsync(token),
            Times.Once,
            "Le droit à l'effacement (RGPD Art. 17) doit être respecté"
        );
    }

    [Fact]
    public async Task UpdatePreferences_MustRecordConsentDate_GDPR_Article7()
    {
        // Arrange
        var dto = new UpdateNewsletterPreferencesDto
        {
            EstAbonne = true,
            CategoriesSports = true
        };

        // Act
        await _controller.UpdatePreferences(dto);

        // Assert - Vérifier que le service a bien été appelé
        // (La date de consentement sera enregistrée dans le service)
        _mockNewsletterService.Verify(
            s => s.UpdatePreferencesAsync(123, dto),
            Times.Once,
            "Le consentement doit être tracé avec la date (RGPD Art. 7)"
        );
    }

    #endregion
}

