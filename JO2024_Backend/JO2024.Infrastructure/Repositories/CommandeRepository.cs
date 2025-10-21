// ============================================
// CommandeRepository.cs
// ============================================
using Microsoft.EntityFrameworkCore;
using JO2024.Core.Entities;
using JO2024.Core.Interfaces;
using JO2024.Infrastructure.Data;

namespace JO2024.Infrastructure.Repositories;

public class CommandeRepository : Repository<Commande>, ICommandeRepository
{
    public CommandeRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Commande>> GetByUtilisateurIdAsync(int utilisateurId)
    {
        return await _dbSet
            .Include(c => c.Items)
                .ThenInclude(i => i.Offre)
            .Where(c => c.UtilisateurId == utilisateurId)
            .OrderByDescending(c => c.DateAchat)
            .ToListAsync();
    }

    public async Task<Commande?> GetByNumeroAsync(string numero)
    {
        return await _dbSet
            .Include(c => c.Items)
                .ThenInclude(i => i.Offre)
            .Include(c => c.Billets)
            .FirstOrDefaultAsync(c => c.Numero == numero);
    }

    public async Task<Commande?> GetWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(c => c.Items)
                .ThenInclude(i => i.Offre)
            .Include(c => c.Billets)
            .Include(c => c.Utilisateur)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<string> GenerateNumeroCommandeAsync()
    {
        var date = DateTime.UtcNow;
        var dateStr = date.ToString("yyyyMMdd");
        
        // Compter les commandes du jour
        var startOfDay = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
        var endOfDay = startOfDay.AddDays(1);
        
        var count = await _dbSet
            .Where(c => c.DateAchat >= startOfDay && c.DateAchat < endOfDay)
            .CountAsync();
        
        var sequence = (count + 1).ToString("D5");
        
        return $"CMD-{dateStr}-{sequence}";
    }
}
