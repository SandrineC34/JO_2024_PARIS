using System.ComponentModel.DataAnnotations;

namespace JO2024.Core.DTOs.Newsletter;

/// <summary>
/// DTO pour récupérer les préférences newsletter d'un utilisateur
/// </summary>
public class NewsletterPreferencesDto
{
    public int UtilisateurId { get; set; }
    
    public bool EstAbonne { get; set; }
    
    public NewsletterCategoriesDto Categories { get; set; } = new();
    
    public DateTime? DateAbonnement { get; set; }
    
    public DateTime? DateModification { get; set; }
    
    public DateTime? DateDesabonnement { get; set; }
    
    /// <summary>
    /// Indique si un email de confirmation a été envoyé
    /// </summary>
    public bool EmailConfirmationEnvoye { get; set; }
    
    public DateTime? DateEmailConfirmation { get; set; }
}