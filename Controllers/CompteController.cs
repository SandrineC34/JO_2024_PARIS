using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JO2024API.Data;
using JO2024API.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BCrypt.Net;

namespace JO2024API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Toutes les actions nécessitent une authentification
    public class CompteController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CompteController(AppDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }

        // GET: api/compte/billets - Pour la section "Mes Billets"
        [HttpGet("billets")]
        public async Task<ActionResult<IEnumerable<BilletDetailsDto>>> GetMesBillets()
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var billets = await _context.Billets
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.DateEvenement)
                .Select(b => new BilletDetailsDto
                {
                    Id = b.Id,
                    NumeroBillet = b.NumeroBillet,
                    Titre = b.Titre,
                    Sport = b.Sport,
                    DateEvenement = b.DateEvenement,
                    Lieu = b.Lieu,
                    Place = FormatPlace(b.Secteur, b.Rangee, b.Sieges),
                    Statut = b.Statut.ToString(),
                    StatutDescription = GetStatutDescription(b.Statut, b.DateScan),
                    DateScan = b.DateScan,
                    PeutVoirQR = b.Statut == BilletStatut.Actif,
                    PeutTelecharger = b.Statut != BilletStatut.Annule
                })
                .ToListAsync();

            return billets;
        }

        // GET: api/compte/commandes - Pour la section "Mes Commandes" 
        [HttpGet("commandes")]
        public async Task<ActionResult<IEnumerable<CommandeHistoriqueDto>>> GetMesCommandes()
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var commandes = await _context.Commandes
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.DateCommande)
                .Select(c => new CommandeHistoriqueDto
                {
                    NumeroCommande = c.NumeroCommande,
                    DateCommande = c.DateCommande,
                    Description = c.Description,
                    MontantTotal = c.MontantTotal,
                    Statut = c.Statut.ToString()
                })
                .ToListAsync();

            return commandes;
        }

        // GET: api/compte/profil - Pour les paramètres du compte
        [HttpGet("profil")]
        public async Task<ActionResult<User>> GetProfil()
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            // Ne pas retourner le mot de passe
            user.MotDePasse = null;
            return user;
        }

        // PUT: api/compte/profil - Mise à jour des informations
        [HttpPut("profil")]
        public async Task<IActionResult> UpdateProfil(UpdateUserInfoDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            // Vérifier si l'email n'est pas déjà utilisé par un autre utilisateur
            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == dto.Email && u.Id != userId);

            if (emailExists)
            {
                return BadRequest("Cette adresse email est déjà utilisée par un autre compte");
            }

            // Mettre à jour les informations
            user.Prenom = dto.Prenom;
            user.Nom = dto.Nom;
            user.Email = dto.Email;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Informations mises à jour avec succès" });
            }
            catch (Exception)
            {
                return StatusCode(500, "Erreur lors de la mise à jour");
            }
        }

        // PUT: api/compte/motdepasse - Changement de mot de passe
        [HttpPut("motdepasse")]
        public async Task<IActionResult> ChangerMotDePasse(ChangePasswordDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            // Vérifier le mot de passe actuel
            if (!BCrypt.Verify(dto.MotDePasseActuel, user.MotDePasse))
            {
                return BadRequest("Mot de passe actuel incorrect");
            }

            // Valider le nouveau mot de passe
            if (!IsValidPassword(dto.NouveauMotDePasse))
            {
                return BadRequest("Le nouveau mot de passe ne respecte pas les critères de sécurité");
            }

            // Hasher et sauvegarder le nouveau mot de passe
            user.MotDePasse = BCrypt.HashPassword(dto.NouveauMotDePasse);

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Mot de passe changé avec succès" });
            }
            catch (Exception)
            {
                return StatusCode(500, "Erreur lors du changement de mot de passe");
            }
        }

        // GET: api/compte/billet/{id}/qr - Génération du QR code
        [HttpGet("billet/{id}/qr")]
        public async Task<IActionResult> GetQRCode(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var billet = await _context.Billets
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (billet == null) return NotFound();

            if (billet.Statut != BilletStatut.Actif)
            {
                return BadRequest("Ce billet n'est plus valide pour affichage du QR code");
            }

            // Générer ou récupérer le QR code
            if (string.IsNullOrEmpty(billet.QRCode))
            {
                billet.QRCode = GenerateQRCodeData(billet);
                await _context.SaveChangesAsync();
            }

            return Ok(new { qrCode = billet.QRCode, numeroBillet = billet.NumeroBillet });
        }

        // GET: api/compte/billet/{id}/pdf - Téléchargement PDF
        [HttpGet("billet/{id}/pdf")]
        public async Task<IActionResult> TelechargerPDF(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var billet = await _context.Billets
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (billet == null) return NotFound();

            // Ici vous pourriez générer un vrai PDF
            // Pour l'exemple, on retourne juste les informations
            var pdfData = new
            {
                numeroBillet = billet.NumeroBillet,
                titre = billet.Titre,
                dateEvenement = billet.DateEvenement,
                lieu = billet.Lieu,
                place = FormatPlace(billet.Secteur, billet.Rangee, billet.Sieges),
                proprietaire = $"{billet.User.Prenom} {billet.User.Nom}",
                qrCode = billet.QRCode ?? GenerateQRCodeData(billet)
            };

            return Ok(pdfData);
        }

        // POST: api/compte/billet/{id}/email - Envoi par email
        [HttpPost("billet/{id}/email")]
        public async Task<IActionResult> EnvoyerParEmail(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var billet = await _context.Billets
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (billet == null) return NotFound();

            // Ici vous implémenteriez l'envoi d'email
            // Pour l'exemple, on simule juste
            
            return Ok(new { message = $"Billet {billet.NumeroBillet} envoyé par email à {billet.User.Email}" });
        }

        // GET: api/compte/donnees - Export des données (RGPD)
        [HttpGet("donnees")]
        public async Task<IActionResult> ExporterDonnees()
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var user = await _context.Users
                .Include(u => u.Commandes)
                .ThenInclude(c => c.Billets)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound();

            var exportData = new
            {
                informationsPersonnelles = new
                {
                    prenom = user.Prenom,
                    nom = user.Nom,
                    email = user.Email,
                    dateCreation = user.DateCreation
                },
                commandes = user.Commandes.Select(c => new
                {
                    numeroCommande = c.NumeroCommande,
                    dateCommande = c.DateCommande,
                    montant = c.MontantTotal,
                    statut = c.Statut.ToString()
                }),
                billets = user.Commandes.SelectMany(c => c.Billets).Select(b => new
                {
                    numeroBillet = b.NumeroBillet,
                    titre = b.Titre,
                    dateEvenement = b.DateEvenement,
                    statut = b.Statut.ToString()
                })
            };

            return Ok(exportData);
        }

        // DELETE: api/compte - Suppression du compte
        [HttpDelete]
        public async Task<IActionResult> SupprimerCompte()
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var user = await _context.Users
                .Include(u => u.Commandes)
                .ThenInclude(c => c.Billets)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound();

            // Vérifier s'il y a des billets actifs
            var billetsActifs = user.Commandes
                .SelectMany(c => c.Billets)
                .Any(b => b.Statut == BilletStatut.Actif && b.DateEvenement > DateTime.Now);

            if (billetsActifs)
            {
                return BadRequest("Impossible de supprimer le compte : vous avez des billets actifs pour des événements à venir");
            }

            try
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Compte supprimé avec succès" });
            }
            catch (Exception)
            {
                return StatusCode(500, "Erreur lors de la suppression du compte");
            }
        }

        // Méthodes utilitaires privées
        private string FormatPlace(string secteur, string rangee, string sieges)
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(secteur)) parts.Add(secteur);
            if (!string.IsNullOrEmpty(rangee)) parts.Add(rangee);
            if (!string.IsNullOrEmpty(sieges)) parts.Add(sieges);
            
            return string.Join(" - ", parts);
        }

        private string GetStatutDescription(BilletStatut statut, DateTime? dateScan)
        {
            return statut switch
            {
                BilletStatut.Actif => "Actif - Prêt à être utilisé",
                BilletStatut.Scanne => $"Scanné le {dateScan?.ToString("dd/MM/yyyy à HH:mm")}",
                BilletStatut.Expire => "Expiré",
                BilletStatut.Annule => "Annulé",
                _ => "Statut inconnu"
            };
        }

        private string GenerateQRCodeData(Billet billet)
        {
            // Générer les données du QR code (à adapter selon votre format)
            return $"JO2024|{billet.NumeroBillet}|{billet.UserId}|{billet.DateEvenement:yyyyMMdd}|{billet.Sport}";
        }

        private bool IsValidPassword(string password)
        {
            // Validation du mot de passe selon vos critères HTML
            if (password.Length < 8) return false;
            if (!password.Any(char.IsUpper)) return false;
            if (!password.Any(char.IsLower)) return false;
            if (!password.Any(char.IsDigit)) return false;
            if (!password.Any(c => "!@#$%^&*".Contains(c))) return false;

            return true;
        }
    }
}