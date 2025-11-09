using JO2024.Core.DTOs.Newsletter;

namespace JO2024.Core.Interfaces;

public interface INewsletterService
{
    // Gestion des préférences
    Task<NewsletterPreferencesDto?> GetPreferencesAsync(int userId);
    Task UpdatePreferencesAsync(int userId, UpdateNewsletterPreferencesDto dto);
    
    // Désinscription
    Task<bool> UnsubscribeByTokenAsync(string token);
    Task<int?> GetUserIdByTokenAsync(string token);
    
    // Statistiques (Admin)
    Task<NewsletterStatsDto> GetStatsAsync();
    
    // Génération de token pour désinscription
    Task<string> GenerateUnsubscribeTokenAsync(int userId);
}