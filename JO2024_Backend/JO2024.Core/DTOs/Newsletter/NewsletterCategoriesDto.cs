namespace JO2024.Core.DTOs.Newsletter;

/// <summary>
/// DTO pour les catégories de newsletter
/// </summary>
public class NewsletterCategoriesDto
{
    /// <summary>
    /// Catégorie Sports (actualités sportives, résultats, etc.)
    /// </summary>
    public bool Sports { get; set; }
    
    /// <summary>
    /// Catégorie Événements (cérémonies, événements spéciaux, etc.)
    /// </summary>
    public bool Evenements { get; set; }
    
    /// <summary>
    /// Catégorie Offres billets (promotions, nouvelles offres, etc.)
    /// </summary>
    public bool Billets { get; set; }
}