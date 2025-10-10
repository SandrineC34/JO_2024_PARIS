// ============================================
// ICommandeService.cs
// ============================================
using JO2024.Core.DTOs.Commandes;

namespace JO2024.Core.Interfaces;

public interface ICommandeService
{
    Task<CommandeResponseDto> CreateCommandeAsync(int utilisateurId, CreateCommandeDto createCommandeDto);
    Task<IEnumerable<CommandeDto>> GetCommandesByUtilisateurAsync(int utilisateurId);
    Task<CommandeDto?> GetCommandeByIdAsync(int commandeId, int utilisateurId);
}