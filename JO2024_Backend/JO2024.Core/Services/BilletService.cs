// ============================================
// BilletService.cs
// ============================================
using JO2024.Core.Interfaces;
using JO2024.Core.DTOs.Billets;

namespace JO2024.Core.Services;

public class BilletService : IBilletService
{
    private readonly IBilletRepository _billetRepository;

    public BilletService(IBilletRepository billetRepository)
    {
        _billetRepository = billetRepository;
    }

    public async Task<IEnumerable<BilletDto>> GetBilletsByUtilisateurAsync(int utilisateurId)
    {
        var billets = await _billetRepository.GetByUtilisateurIdAsync(utilisateurId);
        return billets.Select(MapToBilletDto);
    }

    public async Task<BilletDto?> GetBilletByIdAsync(int billetId, int utilisateurId)
    {
        var billet = await _billetRepository.GetByIdAsync(billetId);
        
        if (billet == null || billet.UtilisateurId != utilisateurId)
            return null;
        
        return MapToBilletDto(billet);
    }

    public async Task<string> GeneratePdfAsync(int billetId, int utilisateurId)
    {
        var billet = await _billetRepository.GetByIdAsync(billetId);
        
        if (billet == null || billet.UtilisateurId != utilisateurId)
            throw new UnauthorizedAccessException("Accès non autorisé à ce billet");
        
        // TODO: Implémenter la génération du PDF avec iText7
        // Pour l'instant, retourner une URL fictive
        return $"/api/billets/{billetId}/pdf";
    }

    public async Task<bool> SendBilletByEmailAsync(int billetId, int utilisateurId)
    {
        var billet = await _billetRepository.GetByIdAsync(billetId);
        
        if (billet == null || billet.UtilisateurId != utilisateurId)
            return false;
        
        // TODO: Implémenter l'envoi d'email avec MailKit
        // Pour l'instant, simuler l'envoi
        return true;
    }

    public async Task<bool> ScanBilletAsync(string numeroBillet)
    {
        var billet = await _billetRepository.GetByNumeroAsync(numeroBillet);
        
        if (billet == null)
            return false;
        
        return await _billetRepository.ScanBilletAsync(billet.Id);
    }

    private BilletDto MapToBilletDto(Entities.Billet billet)
    {
        return new BilletDto
        {
            Id = billet.Id,
            Numero = billet.Numero,
            Titre = billet.Titre,
            Sport = billet.Sport,
            Lieu = billet.Lieu,
            DateEpreuve = billet.DateEpreuve,
            Place = billet.Place,
            Statut = billet.Statut,
            CodeQR = billet.CodeQR,
            DateScan = billet.DateScan,
            DateCreation = billet.DateCreation
        };
    }
}