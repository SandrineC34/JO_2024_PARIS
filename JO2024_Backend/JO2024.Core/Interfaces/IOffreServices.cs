// ============================================
// IOffreService.cs
// ============================================
using JO2024.Core.DTOs.Offres;

namespace JO2024.Core.Interfaces;

public interface IOffreService
{
    Task<IEnumerable<OffreDto>> GetAllOffresAsync();
    Task<OffreDto?> GetOffreByIdAsync(int id);
    Task<OffreDto?> GetOffreByTypeAsync(string type);
}