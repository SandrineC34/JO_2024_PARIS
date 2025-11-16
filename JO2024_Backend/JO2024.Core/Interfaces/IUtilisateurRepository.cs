using JO2024.Core.Entities;

namespace JO2024.Core.Interfaces
{
    public interface IUtilisateurRepository
    {
        // Opérations CRUD de base
        Task<Utilisateur?> GetByIdAsync(int id);
        Task<Utilisateur?> GetByEmailAsync(string email);
        Task<IEnumerable<Utilisateur>> GetAllAsync();
        Task AddAsync(Utilisateur utilisateur);
        Task UpdateAsync(Utilisateur utilisateur);
        Task DeleteAsync(int id);
        Task<int> SaveChangesAsync();

        // Méthodes spécifiques pour l'authentification
        Task<Utilisateur?> GetByResetTokenAsync(string token);

        // Méthodes pour la newsletter
        Task<IEnumerable<Utilisateur>> GetNewsletterSubscribersAsync();
        Task<Utilisateur?> GetByUnsubscribeTokenAsync(string token);
        
        // Méthode pour AdminService
        Task<Utilisateur?> GetWithCommandesAsync(int userId);
        
        // Méthode pour CompteController
        Task<bool> EmailExistsAsync(string email);
    }
}