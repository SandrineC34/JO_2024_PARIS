namespace JO2024.Core.Entities;

public class NewsletterSubscriptionHistory
{
    public int Id { get; set; }
    public int UtilisateurId { get; set; }
    public bool EstAbonne { get; set; }
    public bool CategoriesSports { get; set; }
    public bool CategoriesEvenements { get; set; }
    public bool CategoriesBillets { get; set; }
    public DateTime DateAction { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Source { get; set; }
    
    // Relation
    public virtual Utilisateur Utilisateur { get; set; } = null!;
}