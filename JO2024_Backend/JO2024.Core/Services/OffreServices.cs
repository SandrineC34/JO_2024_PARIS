// ============================================
// OffreService.cs
// ============================================
using JO2024.Core.Entities;
using JO2024.Core.Interfaces;
using JO2024.Core.DTOs.Offres;

namespace JO2024.Core.Services;

public class OffreService : IOffreService
{
    private readonly IOffreRepository _offreRepository;

    public OffreService(IOffreRepository offreRepository)
    {
        _offreRepository = offreRepository;
    }

    public async Task<IEnumerable<OffreDto>> GetAllOffresAsync()
    {
        var offres = await _offreRepository.GetActiveOffresAsync();
        return offres.Select(MapToOffreDto);
    }

    public async Task<OffreDto?> GetOffreByIdAsync(int id)
    {
        var offre = await _offreRepository.GetByIdAsync(id);
        return offre != null ? MapToOffreDto(offre) : null;
    }

    public async Task<OffreDto?> GetOffreByTypeAsync(string type)
    {
        var offre = await _offreRepository.GetByTypeAsync(type);
        return offre != null ? MapToOffreDto(offre) : null;
    }

    private OffreDto MapToOffreDto(Entities.Offre offre)
    {
        return new OffreDto
        {
            Id = offre.Id,
            Type = offre.Type,
            Nom = offre.Nom,
            Description = offre.Description,
            Prix = offre.Prix,
            NombrePersonnes = offre.NombrePersonnes,
            Caracteristiques = offre.Caracteristiques
        };
    }
}