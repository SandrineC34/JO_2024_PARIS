namespace JO2024.Core.Entities;

public class NewsletterSubscription
{
    public int Id { get; set; }
    public int UtilisateurId { get; set; }
    public bool EstAbonne { get; set; }
    
    // Catégories
    public bool CategoriesSports { get; set; }
    public bool CategoriesEvenements { get; set; }
    public bool CategoriesBillets { get; set; }
    
    // Traçabilité RGPD
    public DateTime DateAbonnement { get; set; }
    public DateTime? DateDesabonnement { get; set; }
    public DateTime DateModification { get; set; }
    public string? TokenDesabonnement { get; set; }
    public DateTime? TokenExpiration { get; set; }
    
    // Email de confirmation
    public bool EmailConfirmationEnvoye { get; set; }
    public DateTime? DateEmailConfirmation { get; set; }
    
    // Relation
    public virtual Utilisateur Utilisateur { get; set; } = null!;
}