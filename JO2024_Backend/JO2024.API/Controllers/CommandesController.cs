// ============================================
// CommandesController.cs
// ============================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using JO2024.Core.Interfaces;
using JO2024.Core.DTOs.Commandes;

namespace JO2024.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CommandesController : ControllerBase
{
    private readonly ICommandeService _commandeService;
    private readonly ILogger<CommandesController> _logger;

    public CommandesController(ICommandeService commandeService, ILogger<CommandesController> logger)
    {
        _commandeService = commandeService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateCommande([FromBody] CreateCommandeDto createCommandeDto)
    {
        try
        {
            var userId = GetUserIdFromClaims();
            var result = await _commandeService.CreateCommandeAsync(userId, createCommandeDto);
            
            if (!result.Success)
                return BadRequest(result);
            
            return CreatedAtAction(nameof(GetCommandeById), new { id = result.Commande?.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création de la commande");
            return StatusCode(500, new { message = "Une erreur s'est produite lors de la création de la commande" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetMyCommandes()
    {
        try
        {
            var userId = GetUserIdFromClaims();
            var commandes = await _commandeService.GetCommandesByUtilisateurAsync(userId);
            return Ok(commandes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des commandes");
            return StatusCode(500, new { message = "Une erreur s'est produite" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCommandeById(int id)
    {
        try
        {
            var userId = GetUserIdFromClaims();
            var commande = await _commandeService.GetCommandeByIdAsync(id, userId);
            
            if (commande == null)
                return NotFound(new { message = "Commande non trouvée" });
            
            return Ok(commande);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de la commande");
            return StatusCode(500, new { message = "Une erreur s'est produite" });
        }
    }

    private int GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim ?? "0");
    }
}