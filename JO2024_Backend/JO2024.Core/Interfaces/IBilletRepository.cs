// ============================================
// IBilletRepository.cs
// ============================================
using JO2024.Core.Entities;

namespace JO2024.Core.Interfaces;

public interface IBilletRepository : IRepository<Billet>
{
    Task<IEnumerable<Billet>> GetByUtilisateurIdAsync(int utilisateurId);
    Task<IEnumerable<Billet>> GetByCommandeIdAsync(int commandeId);
    Task<Billet?> GetByNumeroAsync(string numero);
    Task<string> GenerateNumeroBilletAsync(string sport);
    Task<bool> ScanBilletAsync(int billetId);
}