// ============================================
// IAdminService.cs
// JO2024.Core/Interfaces/IAdminService.cs
// ============================================
using JO2024.Core.DTOs.Admin;
using JO2024.Core.DTOs.Common;
using JO2024.Core.DTOs.Auth;
using JO2024.Core.DTOs.Commandes;
using JO2024.Core.DTOs.Billets;
using JO2024.Core.DTOs.Offres;

namespace JO2024.Core.Interfaces;

public interface IAdminService
{
    // Utilisateurs
    Task<PagedResult<UserDetailsDto>> GetAllUsersAsync(int page, int pageSize);
    Task<UserDetailsDto?> GetUserDetailsAsync(int id);
    Task<bool> ToggleUserStatusAsync(int id);
    Task<bool> UpdateUserRoleAsync(int id, string role);
    
    // Offres
    Task<OffreDto> CreateOffreAsync(CreateOffreDto createOffreDto);
    Task<bool> UpdateOffreAsync(int id, UpdateOffreDto updateOffreDto);
    Task<bool> DeleteOffreAsync(int id);
    
    // Statistiques
    Task<DashboardStatsDto> GetDashboardStatsAsync();
    Task<List<SalesStatsDto>> GetSalesStatsAsync(DateTime? startDate, DateTime? endDate);
    
    // Commandes
    Task<PagedResult<CommandeDto>> GetAllCommandesAsync(int page, int pageSize);
    Task<bool> UpdateCommandeStatusAsync(int id, string status);
    
    // Billets
    Task<PagedResult<BilletDto>> GetAllBilletsAsync(int page, int pageSize);
    Task<bool> CancelBilletAsync(int id);
    
    // Exports
    Task<string> ExportUsersToCSVAsync();
    Task<string> ExportCommandesToCSVAsync(DateTime? startDate, DateTime? endDate);
}