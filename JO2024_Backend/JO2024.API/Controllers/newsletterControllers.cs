// ============================================
// NewsletterController.cs
// JO2024.API/Controllers/NewsletterController.cs
// ============================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using JO2024.Infrastructure.Data;
using JO2024.Core.Entities;
using JO2024.Core.DTOs.Newsletter;

namespace JO2024.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NewsletterController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<NewsletterController> _logger;

    public NewsletterController(
        ApplicationDbContext context,
        ILogger<NewsletterController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Récupérer les préférences newsletter de l'utilisateur connecté
    /// </summary>
    [Authorize]
    [HttpGet("preferences")]
    public async Task<IActionResult> GetPreferences()
    {
        try
        {
            var userId = GetUserIdFromClaims();
            var preference = await _context.NewsletterPreferences
                .FirstOrDefaultAsync(n => n.UtilisateurId == userId);

            if (preference == null)
            {
                return Ok(new NewsletterPreferenceResponseDto
                {
                    EstAbonne = false,
                    Categories = new List<string>()
                });
            }

            var categories = JsonSerializer.Deserialize<List<string>>(
                preference.CategoriesSelectionnees
            ) ?? new List<string>();

            return Ok(new NewsletterPreferenceResponseDto
            {
                EstAbonne = preference.EstAbonne,
                Categories = categories,
                DateConsentement = preference.DateConsentement
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des préférences");
            return StatusCode(500, new { message = "Une erreur s'est produite" });
        }
    }

    /// <summary>
    /// S'abonner ou mettre à jour ses préférences newsletter
    /// </summary>
    [Authorize]
    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] NewsletterSubscribeDto dto)
    {
        try
        {
            var userId = GetUserIdFromClaims();
            var preference = await _context.NewsletterPreferences
                .FirstOrDefaultAsync(n => n.UtilisateurId == userId);

            if (preference == null)
            {
                // Créer une nouvelle préférence
                preference = new NewsletterPreference
                {
                    UtilisateurId = userId,
                    EstAbonne = dto.EstAbonne,
                    DateConsentement = dto.EstAbonne ? DateTime.UtcNow : null,
                    CategoriesSelectionnees = JsonSerializer.Serialize(dto.Categories),
                    TokenDesabonnement = Guid.NewGuid().ToString(),
                    SourceConsentement = "compte"
                };
                _context.NewsletterPreferences.Add(preference);
            }
            else
            {
                // Mettre à jour
                var etaitAbonne = preference.EstAbonne;
                preference.EstAbonne = dto.EstAbonne;
                preference.CategoriesSelectionnees = JsonSerializer.Serialize(dto.Categories);

                if (dto.EstAbonne && !etaitAbonne)
                {
                    // Réabonnement
                    preference.DateConsentement = DateTime.UtcNow;
                    preference.DateDesabonnement = null;
                    preference.TokenDesabonnement = Guid.NewGuid().ToString();
                }
                else if (!dto.EstAbonne && etaitAbonne)
                {
                    // Désabonnement
                    preference.DateDesabonnement = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();

            var message = dto.EstAbonne 
                ? "✅ Vous êtes maintenant abonné à la newsletter !" 
                : "Vous avez été désabonné de la newsletter";

            return Ok(new { success = true, message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'abonnement");
            return StatusCode(500, new { message = "Une erreur s'est produite" });
        }
    }

    /// <summary>
    /// Se désabonner via le lien dans l'email (sans authentification)
    /// </summary>
    [AllowAnonymous]
    [HttpGet("unsubscribe/{token}")]
    public async Task<IActionResult> UnsubscribeByToken(string token)
    {
        try
        {
            var preference = await _context.NewsletterPreferences
                .FirstOrDefaultAsync(n => n.TokenDesabonnement == token);

            if (preference == null)
            {
                return BadRequest(new { message = "Token invalide" });
            }

            preference.EstAbonne = false;
            preference.DateDesabonnement = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Retourner une page HTML simple
            return Content(@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Désabonnement réussi</title>
    <style>
        body {
            font-family: Arial, sans-serif;
            display: flex;
            justify-content: center;
            align-items: center;
            min-height: 100vh;
            margin: 0;
            background: linear-gradient(135deg, #004e92 0%, #000428 100%);
        }
        .card {
            background: white;
            padding: 40px;
            border-radius: 10px;
            text-align: center;
            max-width: 500px;
        }
        h1 { color: #004e92; }
        .icon { font-size: 48px; margin-bottom: 20px; }
        .btn {
            display: inline-block;
            background: #004e92;
            color: white;
            padding: 12px 30px;
            text-decoration: none;
            border-radius: 5px;
            margin-top: 20px;
        }
    </style>
</head>
<body>
    <div class='card'>
        <div class='icon'>✅</div>
        <h1>Désabonnement réussi</h1>
        <p>Vous avez été désabonné de notre newsletter.</p>
        <p>Vous pouvez toujours vous réabonner depuis votre compte.</p>
        <a href='/compte.html' class='btn'>Retour à mon compte</a>
    </div>
</body>
</html>
            ", "text/html");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du désabonnement");
            return StatusCode(500, new { message = "Une erreur s'est produite" });
        }
    }

    /// <summary>
    /// Liste des catégories disponibles
    /// </summary>
    [HttpGet("categories")]
    public IActionResult GetCategories()
    {
        return Ok(new { categories = CategoriesEpreuves.Categories });
    }

    /// <summary>
    /// Statistiques (réservé aux admins)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        try
        {
            var totalAbonnes = await _context.NewsletterPreferences
                .CountAsync(n => n.EstAbonne);

            // Stats par catégorie (simplifiée)
            var statsCategories = new Dictionary<string, int>();
            foreach (var cat in CategoriesEpreuves.Categories)
            {
                var count = await _context.NewsletterPreferences
                    .Where(n => n.EstAbonne && n.CategoriesSelectionnees.Contains(cat))
                    .CountAsync();
                statsCategories[cat] = count;
            }

            return Ok(new NewsletterStatsDto
            {
                TotalAbonnes = totalAbonnes,
                AbonnesParCategorie = statsCategories,
                DernierEnvoi = DateTime.UtcNow // À améliorer avec table d'historique
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des stats");
            return StatusCode(500, new { message = "Une erreur s'est produite" });
        }
    }

    private int GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim ?? "0");
    }
}