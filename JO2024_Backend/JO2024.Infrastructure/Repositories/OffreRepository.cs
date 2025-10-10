// ============================================
// OffreRepository.cs
// ============================================
using Microsoft.EntityFrameworkCore;
using JO2024.Core.Entities;
using JO2024.Core.Interfaces;
using JO2024.Infrastructure.Data;

namespace JO2024.Infrastructure.Repositories;

public class OffreRepository : Repository<Offre>, IOffreRepository
{
    public OffreRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Offre>> GetActiveOffresAsync()
    {
        return await _dbSet
            .Where(o => o.EstActif)
            .OrderBy(o => o.Prix)
            .ToListAsync();
    }

    public async Task<Offre?> GetByTypeAsync(string type)
    {
        return await _dbSet
            .FirstOrDefaultAsync(o => o.Type.ToLower() == type.ToLower() && o.EstActif);
    }
}