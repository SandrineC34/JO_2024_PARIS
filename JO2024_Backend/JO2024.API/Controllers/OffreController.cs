// ============================================
// OffresController.cs
// ============================================
using Microsoft.AspNetCore.Mvc;
using JO2024.Core.Interfaces;

namespace JO2024.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OffresController : ControllerBase
{
    private readonly IOffreService _offreService;
    private readonly ILogger<OffresController> _logger;

    public OffresController(IOffreService offreService, ILogger<OffresController> logger)
    {
        _offreService = offreService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllOffres()
    {
        try
        {
            var offres = await _offreService.GetAllOffresAsync();
            return Ok(offres);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des offres");
            return StatusCode(500, new { message = "Une erreur s'est produite" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOffreById(int id)
    {
        try
        {
            var offre = await _offreService.GetOffreByIdAsync(id);
            
            if (offre == null)
                return NotFound(new { message = "Offre non trouvée" });
            
            return Ok(offre);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de l'offre");
            return StatusCode(500, new { message = "Une erreur s'est produite" });
        }
    }

    [HttpGet("type/{type}")]
    public async Task<IActionResult> GetOffreByType(string type)
    {
        try
        {
            var offre = await _offreService.GetOffreByTypeAsync(type);
            
            if (offre == null)
                return NotFound(new { message = "Offre non trouvée" });
            
            return Ok(offre);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de l'offre");
            return StatusCode(500, new { message = "Une erreur s'est produite" });
        }
    }
}
