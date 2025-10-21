// ============================================
// AdminController.cs
// JO2024.API/Controllers/AdminController.cs
// ============================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using JO2024.Core.Interfaces;
using JO2024.Core.DTOs.Admin;

namespace JO2024.API.Controllers;

[Authorize(Roles = "Admin,SuperAdmin")]
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IAdminService adminService, ILogger<AdminController> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    // ============================================
    // Gestion des Utilisateurs
    // ============================================

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var users = await _adminService.GetAllUsersAsync(page, pageSize);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des utilisateurs");
            return StatusCode(500, new { message = "Une erreur s'est produite" });
        }
    }

    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUserDetails(int id)
    {
        try
        {
            var user = await _adminService.GetUserDetailsAsync(id);
            
            if (user == null)
                return NotFound(new { message = "Utilisateur non trouvé" });
            
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de l'utilisateur");
            return StatusCode(500, new { message = "Une erreur s'est produite" });
        }
    }

    [HttpPut("users/{id}/toggle-status")]
    public async Task<IActionResult> ToggleUserStatus(int id)
    {
        try
        {
            var success = await _adminService.ToggleUserStatusAsync(id);
            
            if (!success)
                return NotFound(new { message = "Utilisateur non trouvé" });
            
            return Ok(new { message = "Statut de l'utilisateur modifié" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la modification du statut");
            return StatusCode(500, new { message = "Une erreur s'est produite" });
        }
    }

    [HttpPut("users/{id}/role")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UpdateRoleDto updateRoleDto)
    {
        try
        {
            var success = await _adminService.UpdateUserRoleAsync(id, updateRoleDto.Role);
            
            if (!success)
                return BadRequest(new { message = "Impossible de modifier le rôle" });
            
            return Ok(new { message = "Rôle modifié avec succès" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la modification du rôle");
            return StatusCode(500, new { message = "Une erreur s'est produite" });
        }
    }

    // ============================================
    // Gestion des Offres
    // ============================================

    [HttpPost("offres")]
    public async Task<IActionResult> CreateOffre([FromBody] CreateOffreDto createOffreDto)
    {
        try
        {
            var offre = await _adminService.CreateOffreAsync(createOffreDto);
            return CreatedAtAction(nameof(GetUserDetails), new { id = offre.Id }, offre);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création de l'offre");
            return StatusCode(500, new { message = "Une erreur s'est produite" });
        }
    }

    [HttpPut("offres/{id}")]
    public async Task<IActionResult> UpdateOffre(int id, [FromBody] UpdateOffreDto updateOffreDto)
    {
        try
        {
            var success = await _adminService.UpdateOffreAsync(id, updateOffreDto);
            
            if (!success)
                return NotFound(new { message = "Offre non trouvée" });
            
            return Ok(new { message = "Offre mise à jour" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise à jour de l'offre");
            return StatusCode(500, new { message = "Une erreur s'est produite" });
        }
    }

    [HttpDelete("offres/{id}")]
    public async Task<IActionResult> DeleteOffre(int id)
    {
        try
        {
            var success = await _adminService.DeleteOffreAsync(id);
            
            if (!success)
                return NotFound(new { message = "Offre non trouvée" });
            
            return Ok(new { message = "Offre supprimée" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression de l'offre");
            return StatusCode(500, new { message = "Une erreur s'est produite" });
        }
    }

    // ============================================
    // Statistiques
    // ============================================

    [HttpGet("stats/dashboard")]
    public async Task<IActionResult> GetDashboardStats()
    {
        try
        {
            var stats = await _adminService.GetDashboardStatsAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des statistiques");
            return StatusCode(500, new { message = "Une erreur s'est produite" });
        }
    }

    [HttpGet("stats/sales")]
    public async Task<IActionResult> GetSalesStats([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        try
        {
            var stats = await _adminService.GetSalesStatsAsync(startDate, endDate);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des ventes");
            return StatusCode(500, new { message = "Une erreur s'est produite" });
        }
    }

    // ============================================
    // Commandes
    // ============================================

    [HttpGet("commandes")]
    public async Task<IActionResult> GetAllCommandes([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var commandes = await _adminService.GetAllCommandesAsync(page, pageSize);
            return Ok(commandes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des commandes");
            return StatusCode(500, new { message = "Une erreur s'est produite" });
        }
    }

    [HttpPut("commandes/{id}/status")]
    public async Task<IActionResult> UpdateCommandeStatus(int id, [FromBody] UpdateStatusDto updateStatusDto)
    {
        try
        {
            var success = await _adminService.UpdateCommandeStatusAsync(id, updateStatusDto.Status);
            
            if (!success)
                return NotFound(new { message = "Commande non trouvée" });
            
            return Ok(new { message = "Statut mis à jour" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise à jour du statut");
            return StatusCode(500, new { message = "Une erreur s'est produite" });
        }
    }

    // ============================================
    // Billets
    // ============================================

    [HttpGet("billets")]
    public async Task<IActionResult> GetAllBillets([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var billets = await _adminService.GetAllBilletsAsync(page, pageSize);
            return Ok(billets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des billets");
            return StatusCode(500, new { message = "Une erreur s'est produite" });
        }
    }

    [HttpPut("billets/{id}/cancel")]
    public async Task<IActionResult> CancelBillet(int id)
    {
        try
        {
            var success = await _adminService.CancelBilletAsync(id);
            
            if (!success)
                return NotFound(new { message = "Billet non trouvé" });
            
            return Ok(new { message = "Billet annulé" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'annulation du billet");
            return StatusCode(500, new { message = "Une erreur s'est produite" });
        }
    }

    // ============================================
    // Exports
    // ============================================

    [HttpGet("export/users")]
    public async Task<IActionResult> ExportUsers()
    {
        try
        {
            var csv = await _adminService.ExportUsersToCSVAsync();
            return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", $"users_{DateTime.UtcNow:yyyyMMdd}.csv");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'export des utilisateurs");
            return StatusCode(500, new { message = "Une erreur s'est produite" });
        }
    }

    [HttpGet("export/commandes")]
    public async Task<IActionResult> ExportCommandes([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        try
        {
            var csv = await _adminService.ExportCommandesToCSVAsync(startDate, endDate);
            return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", $"commandes_{DateTime.UtcNow:yyyyMMdd}.csv");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'export des commandes");
            return StatusCode(500, new { message = "Une erreur s'est produite" });
        }
    }
}
