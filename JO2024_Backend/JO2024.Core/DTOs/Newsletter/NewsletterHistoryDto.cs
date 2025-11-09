namespace JO2024.Core.DTOs.Newsletter;

/// <summary>
/// DTO pour l'historique des actions newsletter (traçabilité RGPD)
/// </summary>
public class NewsletterHistoryDto
{
    public int Id { get; set; }
    
    public int UtilisateurId { get; set; }
    
    public bool EstAbonne { get; set; }
    
    public NewsletterCategoriesDto Categories { get; set; } = new();
    
    public DateTime DateAction { get; set; }
    
    /// <summary>
    /// Type d'action: "Abonnement", "Modification", "Désabonnement"
    /// </summary>
    public string Action { get; set; } = string.Empty;
    
    /// <summary>
    /// Source de l'action: "Inscription", "Compte", "Email"
    /// </summary>
    public string? Source { get; set; }
}