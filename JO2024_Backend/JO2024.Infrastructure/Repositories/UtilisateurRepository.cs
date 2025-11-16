using Microsoft.EntityFrameworkCore;
using JO2024.Core.Entities;
using JO2024.Core.Interfaces;
using JO2024.Infrastructure.Data;

// ⚠️ CE FICHIER DOIT ÊTRE DANS : JO2024.Infrastructure/Repositories/UtilisateurRepository.cs
// PAS DANS JO2024.Core/Services/ !

namespace JO2024.Infrastructure.Repositories
{
    public class UtilisateurRepository : IUtilisateurRepository
    {
        private readonly ApplicationDbContext _context;

        public UtilisateurRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Utilisateur?> GetByIdAsync(int id)
        {
            return await _context.Utilisateurs
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<Utilisateur?> GetByEmailAsync(string email)
        {
            return await _context.Utilisateurs
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<Utilisateur?> GetByResetTokenAsync(string token)
        {
            return await _context.Utilisateurs
                .FirstOrDefaultAsync(u => u.ResetPasswordToken == token);
        }

        public async Task<IEnumerable<Utilisateur>> GetAllAsync()
        {
            return await _context.Utilisateurs.ToListAsync();
        }

        public async Task AddAsync(Utilisateur utilisateur)
        {
            await _context.Utilisateurs.AddAsync(utilisateur);
        }

        public async Task UpdateAsync(Utilisateur utilisateur)
        {
            _context.Utilisateurs.Update(utilisateur);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(int id)
        {
            var utilisateur = await GetByIdAsync(id);
            if (utilisateur != null)
            {
                _context.Utilisateurs.Remove(utilisateur);
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        // Méthodes supplémentaires pour la newsletter
        public async Task<IEnumerable<Utilisateur>> GetNewsletterSubscribersAsync()
        {
            return await _context.Utilisateurs
                .Where(u => u.NewsletterAbonne == true && u.EstActif == true)
                .ToListAsync();
        }

        public async Task<Utilisateur?> GetByUnsubscribeTokenAsync(string token)
        {
            return await _context.Utilisateurs
                .FirstOrDefaultAsync(u => u.NewsletterUnsubscribeToken == token);
        }
        
        // Méthode pour AdminService - Récupérer utilisateur avec ses relations
        public async Task<Utilisateur?> GetWithCommandesAsync(int userId)
        {
            return await _context.Utilisateurs
                .Include(u => u.Commandes)
                .Include(u => u.Billets)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }
        
        // Méthode pour CompteController - Vérifier si email existe
        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Utilisateurs
                .AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }
    }
}