using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace backend.Services
{
    public class CompteService
    {
        private readonly ApplicationDbContext _context;

        public CompteService(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🔹 Récupération du profil utilisateur
        public async Task<Compte?> GetProfileAsync(int userId)
        {
            return await _context.Comptes
                .Include(c => c.Billets)
                .Include(c => c.Commandes)
                .FirstOrDefaultAsync(c => c.Id == userId && c.IsActive);
        }

        // 🔹 Mise à jour du profil
        public async Task<bool> UpdateProfileAsync(int userId, string prenom, string nom, string email)
        {
            var compte = await _context.Comptes.FindAsync(userId);
            if (compte == null || !compte.IsActive) return false;

            compte.FirstName = prenom;
            compte.LastName = nom;
            compte.Email = email;
            compte.LastAction = "Profil modifié";
            compte.LastUpdate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // 🔹 Vérifie le mot de passe actuel
        private bool VerifyPassword(string inputPassword, string storedHash)
        {
            using var sha256 = SHA256.Create();
            var hash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(inputPassword)));
            return hash == storedHash;
        }

        // 🔹 Hache le nouveau mot de passe
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            return Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }

        // 🔹 Changer le mot de passe
        public async Task<(bool Success, string Message)> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var compte = await _context.Comptes.FindAsync(userId);
            if (compte == null) return (false, "Utilisateur introuvable");

            if (!VerifyPassword(currentPassword, compte.PasswordHash))
                return (false, "Mot de passe actuel incorrect");

            compte.PasswordHash = HashPassword(newPassword);
            compte.LastAction = "Mot de passe changé";
            compte.LastUpdate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return (true, "Mot de passe mis à jour avec succès");
        }

        // 🔹 Exporter les données (RGPD)
        public async Task<string> ExportUserDataAsync(int userId)
        {
            var compte = await _context.Comptes
                .Include(c => c.Billets)
                .Include(c => c.Commandes)
                .FirstOrDefaultAsync(c => c.Id == userId);

            if (compte == null) throw new Exception("Utilisateur introuvable");

            var exportData = new
            {
                compte.Id,
                compte.FirstName,
                compte.LastName,
                compte.Email,
                compte.CreatedAt,
                Billets = compte.Billets.Select(b => new
                {
                    b.Id,
                    b.Titre,
                    b.Statut,
                    b.DateEpreuve,
                    b.Lieu,
                    b.Numero
                }),
                Commandes = compte.Commandes.Select(o => new
                {
                    o.Id,
                    o.Numero,
                    o.DateAchat,
                    o.MontantTotal,
                    o.Statut
                })
            };

            return JsonConvert.SerializeObject(exportData, Formatting.Indented);
        }

        // 🔹 Supprimer le compte
        public async Task<bool> DeleteAccountAsync(int userId)
        {
            var compte = await _context.Comptes
                .Include(c => c.Billets)
                .Include(c => c.Commandes)
                .FirstOrDefaultAsync(c => c.Id == userId);

            if (compte == null) return false;

            // Supprime tout ce qui est lié
            _context.Billets.RemoveRange(compte.Billets);
            _context.Commandes.RemoveRange(compte.Commandes);
            _context.Comptes.Remove(compte);

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
