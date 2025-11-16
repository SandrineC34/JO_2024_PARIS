using System.ComponentModel.DataAnnotations;

namespace JO2024.Core.DTOs.Newsletter;

/// <summary>
/// DTO pour envoyer la newsletter (Admin)
/// </summary>
public class SendNewsletterDto
{
    [Required(ErrorMessage = "Le sujet est requis")]
    [StringLength(200, MinimumLength = 5, ErrorMessage = "Le sujet doit contenir entre 5 et 200 caractères")]
    public string Sujet { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Le contenu est requis")]
    [MinLength(50, ErrorMessage = "Le contenu doit contenir au moins 50 caractères")]
    public string ContenuHtml { get; set; } = string.Empty;
    
    /// <summary>
    /// Filtrer par catégories (null = envoyer à tous)
    /// </summary>
    public NewsletterCategoriesDto? Categories { get; set; }
    
    /// <summary>
    /// Date/heure d'envoi programmé (null = envoi immédiat)
    /// </summary>
    public DateTime? DateEnvoiProgramme { get; set; }
}