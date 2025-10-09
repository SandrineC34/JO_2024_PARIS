using System;
using System.Text;
using System.Threading.Tasks;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // ✅ nécessite une session authentifiée (cookies ou JWT)
    public class CompteController : ControllerBase
    {
        private readonly CompteService _compteService;

        public CompteController(CompteService compteService)
        {
            _compteService = compteService;
        }

        // 🔹 GET /api/Compte/profile
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetUserId();
            var profile = await _compteService.GetProfileAsync(userId);
            if (profile == null) return NotFound("Utilisateur non trouvé");

            return Ok(new
            {
                prenom = profile.FirstName,
                nom = profile.LastName,
                email = profile.Email
            });
        }

        // 🔹 PUT /api/Compte/profile
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] dynamic data)
        {
            var userId = GetUserId();
            string prenom = data.prenom;
            string nom = data.nom;
            string email = data.email;

            var success = await _compteService.UpdateProfileAsync(userId, prenom, nom, email);
            if (!success) return BadRequest("Mise à jour impossible");

            return Ok(new { message = "Profil mis à jour" });
        }

        // 🔹 POST /api/Compte/change-password
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] dynamic data)
        {
            var userId = GetUserId();
            string currentPassword = data.currentPassword;
            string newPassword = data.newPassword;

            var (success, message) = await _compteService.ChangePasswordAsync(userId, currentPassword, newPassword);
            if (!success) return BadRequest(new { message });

            return Ok(new { message });
        }

        // 🔹 GET /api/Compte/export-data
        [HttpGet("export-data")]
        public async Task<IActionResult> ExportData()
        {
            var userId = GetUserId();
            var json = await _compteService.ExportUserDataAsync(userId);

            var bytes = Encoding.UTF8.GetBytes(json);
            return File(bytes, "application/json", $"mes-donnees-jo2024-{DateTime.UtcNow:yyyyMMddHHmmss}.json");
        }

        // 🔹 DELETE /api/Compte/delete
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteAccount()
        {
            var userId = GetUserId();
            var success = await _compteService.DeleteAccountAsync(userId);
            if (!success) return BadRequest("Suppression impossible");

            return Ok(new { message = "Compte supprimé" });
        }

        // 🔹 Méthode utilitaire pour extraire l'ID utilisateur depuis le token/session
        private int GetUserId()
        {
            // Exemple simple, à adapter selon ton système d’authentification
            if (User.Identity?.IsAuthenticated == true)
            {
                var idClaim = User.FindFirst("userId")?.Value;
                if (int.TryParse(idClaim, out int userId))
                    return userId;
            }

            throw new UnauthorizedAccessException("Utilisateur non authentifié");
        }
    }
}
