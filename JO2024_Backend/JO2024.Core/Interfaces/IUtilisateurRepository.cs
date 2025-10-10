// ============================================
// IUtilisateurRepository.cs
// ============================================
using JO2024.Core.Entities;

namespace JO2024.Core.Interfaces;

public interface IUtilisateurRepository : IRepository<Utilisateur>
{
    Task<Utilisateur?> GetByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
    Task<Utilisateur?> GetWithCommandesAsync(int id);
    Task<Utilisateur?> GetWithBilletsAsync(int id);
}