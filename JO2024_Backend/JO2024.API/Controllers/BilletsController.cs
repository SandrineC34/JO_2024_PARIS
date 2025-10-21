// ============================================
// BilletsController.cs
// ============================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using JO2024.Core.Interfaces;

namespace JO2024.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BilletsController : ControllerBase
{
    private readonly IBilletService _billetService;
    private readonly ILogger<BilletsController> _logger;

    public BilletsController(IBilletService billetService, ILogger<BilletsController> logger)
    {
        _billetService = billetService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyBillets()
    {
        try
        {
            var userId = GetUserIdFromClaims();
            var billets = await _billetService.GetBilletsByUtilisateurAsync(userId);
            return Ok(billets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des billets");
            return StatusCode(500, new { message = "Une erreur s'est produite" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBilletById(int id)
    {
        try
        {
            var userId = GetUserIdFromClaims();
            var billet = await _billetService.GetBilletByIdAsync(id, userId);
            
            if (billet == null)
                return NotFound(new { message = "Billet non trouvé" });
            
            return Ok(billet);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération du billet");
            return StatusCode(500, new { message = "Une erreur s'est produite" });
        }
    }

    [HttpPost("{id}/download")]
    public async Task<IActionResult> DownloadBilletPdf(int id)
    {
        try
        {
            var userId = GetUserIdFromClaims();
            var downloadUrl = await _billetService.GeneratePdfAsync(id, userId);
            
            return Ok(new { 
                success = true, 
                message = "PDF généré avec succès", 
                downloadUrl 
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "Accès non autorisé" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la génération du PDF");
            return StatusCode(500, new { message = "Une erreur s'est produite" });
        }
    }

    [HttpPost("{id}/email")]
    public async Task<IActionResult> SendBilletByEmail(int id)
    {
        try
        {
            var userId = GetUserIdFromClaims();
            var success = await _billetService.SendBilletByEmailAsync(id, userId);
            
            if (!success)
                return NotFound(new { message = "Billet non trouvé" });
            
            return Ok(new { 
                success = true, 
                message = "Billet envoyé par email avec succès" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'envoi du billet par email");
            return StatusCode(500, new { message = "Une erreur s'est produite" });
        }
    }

    [HttpPost("scan")]
    [AllowAnonymous] // Pour permettre aux scanners de valider les billets
    public async Task<IActionResult> ScanBillet([FromBody] ScanBilletDto scanDto)
    {
        try
        {
            var success = await _billetService.ScanBilletAsync(scanDto.NumeroBillet);
            
            if (!success)
                return BadRequest(new { message = "Billet invalide ou déjà utilisé" });
            
            return Ok(new { 
                success = true, 
                message = "Billet validé avec succès" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du scan du billet");
            return StatusCode(500, new { message = "Une erreur s'est produite" });
        }
    }

    private int GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim ?? "0");
    }
}

public class ScanBilletDto
{
    public string NumeroBillet { get; set; } = string.Empty;
}
