// ============================================
// UtilisateurRepository.cs
// ============================================
using Microsoft.EntityFrameworkCore;
using JO2024.Core.Entities;
using JO2024.Core.Interfaces;
using JO2024.Infrastructure.Data;

namespace JO2024.Infrastructure.Repositories;

public class UtilisateurRepository : Repository<Utilisateur>, IUtilisateurRepository
{
    public UtilisateurRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Utilisateur?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _dbSet
            .AnyAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<Utilisateur?> GetWithCommandesAsync(int id)
    {
        return await _dbSet
            .Include(u => u.Commandes)
                .ThenInclude(c => c.Items)
                    .ThenInclude(i => i.Offre)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<Utilisateur?> GetWithBilletsAsync(int id)
    {
        return await _dbSet
            .Include(u => u.Billets)
            .FirstOrDefaultAsync(u => u.Id == id);
    }
}
