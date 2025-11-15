namespace JO2024.Core.DTOs.Newsletter;

/// <summary>
/// DTO pour la liste des abonnés (Admin)
/// </summary>
public class NewsletterSubscriberDto
{
    public int UtilisateurId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public bool EstAbonne { get; set; }
    public NewsletterCategoriesDto Categories { get; set; } = new();
    public DateTime DateAbonnement { get; set; }
    public DateTime? DateDesabonnement { get; set; }
}