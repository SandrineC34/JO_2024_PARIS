// ============================================
// BilletRepository.cs
// ============================================
using Microsoft.EntityFrameworkCore;
using JO2024.Core.Entities;
using JO2024.Core.Interfaces;
using JO2024.Infrastructure.Data;

namespace JO2024.Infrastructure.Repositories;

public class BilletRepository : Repository<Billet>, IBilletRepository
{
    public BilletRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Billet>> GetByUtilisateurIdAsync(int utilisateurId)
    {
        return await _dbSet
            .Where(b => b.UtilisateurId == utilisateurId)
            .OrderByDescending(b => b.DateCreation)
            .ToListAsync();
    }

    public async Task<IEnumerable<Billet>> GetByCommandeIdAsync(int commandeId)
    {
        return await _dbSet
            .Where(b => b.CommandeId == commandeId)
            .ToListAsync();
    }

    public async Task<Billet?> GetByNumeroAsync(string numero)
    {
        return await _dbSet
            .Include(b => b.Commande)
            .Include(b => b.Utilisateur)
            .FirstOrDefaultAsync(b => b.Numero == numero);
    }

    public async Task<string> GenerateNumeroBilletAsync(string sport)
    {
        var sportCode = GetSportCode(sport);
        var date = DateTime.UtcNow;
        
        // Compter les billets du sport
        var count = await _dbSet
            .Where(b => b.Sport.ToLower() == sport.ToLower())
            .CountAsync();
        
        var sequence = (count + 1).ToString("D5");
        
        return $"JO2024-{sportCode}-{sequence}";
    }

    public async Task<bool> ScanBilletAsync(int billetId)
    {
        var billet = await GetByIdAsync(billetId);
        
        if (billet == null || billet.Statut != "Actif")
            return false;
        
        billet.Statut = "Scanné";
        billet.DateScan = DateTime.UtcNow;
        
        await UpdateAsync(billet);
        return true;
    }

    private string GetSportCode(string sport)
    {
        var sportCodes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "natation", "NAT" },
            { "athletisme", "ATH" },
            { "basketball", "BSK" },
            { "surf", "SRF" },
            { "gymnastique", "GYM" },
            { "tennis", "TEN" }
        };

        return sportCodes.TryGetValue(sport, out var code) ? code : "GEN";
    }
}