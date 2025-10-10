// ============================================
// IOffreRepository.cs
// ============================================
using JO2024.Core.Entities;

namespace JO2024.Core.Interfaces;

public interface IOffreRepository : IRepository<Offre>
{
    Task<IEnumerable<Offre>> GetActiveOffresAsync();
    Task<Offre?> GetByTypeAsync(string type);
}