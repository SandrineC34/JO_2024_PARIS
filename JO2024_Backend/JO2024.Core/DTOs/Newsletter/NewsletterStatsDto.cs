namespace JO2024.Core.DTOs.Newsletter;

/// <summary>
/// DTO pour les statistiques newsletter (Admin)
/// </summary>
public class NewsletterStatsDto
{
    /// <summary>
    /// Nombre total d'abonnés actifs
    /// </summary>
    public int TotalAbonnes { get; set; }
    
    /// <summary>
    /// Nombre total de désabonnements
    /// </summary>
    public int TotalDesabonnes { get; set; }
    
    /// <summary>
    /// Nombre d'abonnés à la catégorie Sports
    /// </summary>
    public int AbonnesSports { get; set; }
    
    /// <summary>
    /// Nombre d'abonnés à la catégorie Événements
    /// </summary>
    public int AbonnesEvenements { get; set; }
    
    /// <summary>
    /// Nombre d'abonnés à la catégorie Billets
    /// </summary>
    public int AbonnesBillets { get; set; }
    
    /// <summary>
    /// Taux d'abonnement (%)
    /// </summary>
    public decimal TauxAbonnement { get; set; }
    
    /// <summary>
    /// Taux de désabonnement (%)
    /// </summary>
    public decimal TauxDesabonnement { get; set; }
    
    /// <summary>
    /// Nombre de nouveaux abonnés cette semaine
    /// </summary>
    public int NouveauxAbonnesSemaine { get; set; }
    
    /// <summary>
    /// Nombre de désabonnements cette semaine
    /// </summary>
    public int DesabonnementsSemaine { get; set; }
    
    /// <summary>
    /// Date de calcul des statistiques
    /// </summary>
    public DateTime DateCalcul { get; set; }
}