using System.ComponentModel.DataAnnotations;

namespace JO2024.Core.DTOs.Newsletter;

/// <summary>
/// DTO pour mettre à jour les préférences newsletter
/// </summary>
public class UpdateNewsletterPreferencesDto
{
    [Required(ErrorMessage = "Le statut d'abonnement est requis")]
    public bool EstAbonne { get; set; }
    [Required(ErrorMessage = "Les catégories sont requises")]
    public NewsletterCategoriesDto Categories { get; set; } = new();
}