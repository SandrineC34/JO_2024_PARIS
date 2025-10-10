// ============================================
// ICommandeRepository.cs
// ============================================
using JO2024.Core.Entities;

namespace JO2024.Core.Interfaces;

public interface ICommandeRepository : IRepository<Commande>
{
    Task<IEnumerable<Commande>> GetByUtilisateurIdAsync(int utilisateurId);
    Task<Commande?> GetByNumeroAsync(string numero);
    Task<Commande?> GetWithDetailsAsync(int id);
    Task<string> GenerateNumeroCommandeAsync();
}