// ============================================
// CompteController.cs - Version mise à jour
// ============================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using JO2024.Core.Interfaces;
using JO2024.Core.DTOs.Compte;
using JO2024.Core.DTOs.Auth;

namespace JO2024.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CompteController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICommandeService _commandeService;
    private readonly IBilletService _billetService;
    private readonly IUtilisateurRepository _utilisateurRepository;
    private readonly IEmailService _emailService; // ⭐ NOUVEAU
    private readonly ILogger<CompteController> _logger;

    public CompteController(
        IAuthService authService,
        ICommandeService commandeService,
        IBilletService billetService,
        IUtilisateurRepository utilisateurRepository,
        IEmailService emailService, // ⭐ NOUVEAU
        ILogger<CompteController> logger)
    {
        _authService = authService;
        _commandeService = commandeService;
        _billetService = billetService;
        _utilisateurRepository = utilisateurRepository;
        _emailService = emailService; // ⭐ NOUVEAU
        _logger = logger;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            var userId = GetUserIdFromClaims();
            var user = await _authService.GetCurrentUserAsync(userId);
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération du profil");
            return StatusCode(500, new { message = "Une erreur s'est produite" });
        }
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto updateProfileDto)
    {
        try
        {
            var userId = GetUserIdFromClaims();
            var utilisateur = await _utilisateurRepository.GetByIdAsync(userId);
            
            if (utilisateur == null)
                return NotFound(new { message = "Utilisateur non trouvé" });

            // Vérifier si l'email est déjà utilisé par un autre utilisateur
            if (utilisateur.Email != updateProfileDto.Email.ToLower())
            {
                var emailExists = await _utilisateurRepository.EmailExistsAsync(updateProfileDto.Email);
                if (emailExists)
                    return BadRequest(new { message = "Cette adresse email est déjà utilisée" });
            }

            utilisateur.Prenom = updateProfileDto.Prenom;
            utilisateur.Nom = updateProfileDto.Nom;
            utilisateur.Email = updateProfileDto.Email.ToLower();
            
            await _utilisateurRepository.UpdateAsync(utilisateur);
            
            return Ok(new { 
                success = true, 
                message = "Profil mis à jour avec succès" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise à jour du profil");
            return StatusCode(500, new { message = "Une erreur s'est produite" });
        }
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
    {
        try
        {
            var userId = GetUserIdFromClaims();
            var success = await _authService.ChangePasswordAsync(userId, changePasswordDto);
            
            if (!success)
                return BadRequest(new { message = "Mot de passe actuel incorrect" });
            
            return Ok(new { 
                success = true, 
                message = "Mot de passe modifié avec succès" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du changement de mot de passe");
            return StatusCode(500, new { message = "Une erreur s'est produite" });
        }
    }

    [HttpGet("export-data")]
    public async Task<IActionResult> ExportData()
    {
        try {
            var userId = GetUserIdFromClaims();
            var user = await _authService.GetCurrentUserAsync(userId);
            var commandes = await _commandeService.GetCommandesByUtilisateurAsync(userId);
            var billets = await _billetService.GetBilletsByUtilisateurAsync(userId);

            var exportData = new ExportDataDto
            {
                User = user,
                Commandes = commandes.ToList(),
                Billets = billets.ToList(),
                ExportDate = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });

            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            return File(bytes, "application/json", $"mes-donnees-jo2024-{DateTime.UtcNow:yyyyMMdd}.json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'export des données");
            return StatusCode(500, new { message = "Une erreur s'est produite" });
        }
    }

    /// <summary>
    /// ⭐ MODIFIÉ: Suppression de compte avec envoi d'email de confirmation
    /// </summary>
    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteAccount()
    {
        try
        {
            var userId = GetUserIdFromClaims();
            var utilisateur = await _utilisateurRepository.GetByIdAsync(userId);
            
            if (utilisateur == null)
                return NotFound(new { message = "Utilisateur non trouvé" });

            // Désactiver le compte (soft delete)
            utilisateur.EstActif = false;
            await _utilisateurRepository.UpdateAsync(utilisateur);
            
            // ⭐ NOUVEAU: Envoyer l'email de confirmation de suppression
            try
            {
                await _emailService.SendAccountDeletionConfirmationAsync(userId);
                _logger.LogInformation("Email de confirmation de suppression envoyé pour l'utilisateur {UserId}", userId);
            }
            catch (Exception emailEx)
            {
                _logger.LogWarning(emailEx, "Impossible d'envoyer l'email de confirmation de suppression pour {UserId}", userId);
                // Ne pas échouer la suppression si l'email ne part pas
            }
            
            return Ok(new { 
                success = true, 
                message = "Compte supprimé avec succès" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression du compte");
            return StatusCode(500, new { message = "Une erreur s'est produite" });
        }
    }

    private int GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim ?? "0");
    }
}