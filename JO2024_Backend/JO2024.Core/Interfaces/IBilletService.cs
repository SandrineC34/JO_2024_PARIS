// ============================================
// IBilletService.cs
// ============================================
using JO2024.Core.DTOs.Billets;

namespace JO2024.Core.Interfaces;

public interface IBilletService
{
    Task<IEnumerable<BilletDto>> GetBilletsByUtilisateurAsync(int utilisateurId);
    Task<BilletDto?> GetBilletByIdAsync(int billetId, int utilisateurId);
    Task<string> GeneratePdfAsync(int billetId, int utilisateurId);
    Task<bool> SendBilletByEmailAsync(int billetId, int utilisateurId);
    Task<bool> ScanBilletAsync(string numeroBillet);
}