using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using JO2024.Core.Interfaces;
using JO2024.Core.DTOs.Newsletter;

namespace JO2024.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NewsletterController : ControllerBase
{
    private readonly INewsletterService _newsletterService;
    private readonly IEmailService _emailService;
    private readonly ILogger<NewsletterController> _logger;

    public NewsletterController(
        INewsletterService newsletterService,
        IEmailService emailService,
        ILogger<NewsletterController> logger)
    {
        _newsletterService = newsletterService;
        _emailService = emailService;
        _logger = logger;
    }

    [Authorize]
    [HttpGet("preferences")]
    public async Task<IActionResult> GetPreferences()
    {
        try
        {
            var userId = GetUserIdFromClaims();
            var preferences = await _newsletterService.GetPreferencesAsync(userId);
            
            if (preferences == null)
            {
                preferences = new NewsletterPreferencesDto
                {
                    UtilisateurId = userId,
                    EstAbonne = false,
                    Categories = new NewsletterCategoriesDto()
                };
            }
            
            return Ok(preferences);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des préférences newsletter");
            return StatusCode(500, new { message = "Une erreur s'est produite" });
        }
    }

    [Authorize]
    [HttpPut("preferences")]
    public async Task<IActionResult> UpdatePreferences([FromBody] UpdateNewsletterPreferencesDto dto)
    {
        try
        {
            var userId = GetUserIdFromClaims();
            
            await _newsletterService.UpdatePreferencesAsync(userId, dto);
            
            if (dto.EstAbonne)
            {
                await _emailService.SendNewsletterConfirmationAsync(userId);
            }
            
            return Ok(new 
            { 
                success = true, 
                message = "Préférences mises à jour avec succès" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise à jour des préférences");
            return StatusCode(500, new { message = "Une erreur s'est produite" });
        }
    }

    [HttpGet("unsubscribe")]
    public async Task<IActionResult> Unsubscribe([FromQuery] string token)
    {
        try
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new { message = "Token invalide" });
            }

            var result = await _newsletterService.UnsubscribeByTokenAsync(token);
            
            if (!result)
            {
                return BadRequest(new { message = "Token invalide ou expiré" });
            }

            var userId = await _newsletterService.GetUserIdByTokenAsync(token);
            if (userId.HasValue)
            {
                await _emailService.SendUnsubscribeConfirmationAsync(userId.Value);
            }

            return Ok(new 
            { 
                success = true, 
                message = "Vous avez été désinscrit de la newsletter avec succès" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la désinscription");
            return StatusCode(500, new { message = "Une erreur s'est produite" });
        }
    }

    [HttpPost("unsubscribe/confirm")]
    public async Task<IActionResult> ConfirmUnsubscribe([FromBody] UnsubscribeConfirmDto dto)
    {
        try
        {
            var result = await _newsletterService.UnsubscribeByTokenAsync(dto.Token);
            
            if (!result)
            {
                return BadRequest(new { message = "Token invalide ou expiré" });
            }

            return Ok(new 
            { 
                success = true, 
                message = "Désinscription confirmée" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la confirmation de désinscription");
            return StatusCode(500, new { message = "Une erreur s'est produite" });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        try
        {
            var stats = await _newsletterService.GetStatsAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des statistiques");
            return StatusCode(500, new { message = "Une erreur s'est produite" });
        }
    }

    private int GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim ?? "0");
    }
}