using System.ComponentModel.DataAnnotations;

namespace JO2024.Core.Entities;

public class Billet
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Numero { get; set; } = string.Empty; // Format: JO2024-SPORT-XXXXX

    public int CommandeId { get; set; }
    
    public int UtilisateurId { get; set; }

    [Required]
    [StringLength(200)]
    public string Titre { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Sport { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Lieu { get; set; } = string.Empty;

    public DateTime DateEpreuve { get; set; }

    [StringLength(50)]
    public string? Place { get; set; }

    [Required]
    [StringLength(50)]
    public string Statut { get; set; } = "Actif"; // "Actif", "Scanné", "Annulé"

    [Required]
    public string CodeQR { get; set; } = string.Empty; // Base64 ou URL

    public DateTime? DateScan { get; set; }

    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Commande Commande { get; set; } = null!;
    
    public Utilisateur Utilisateur { get; set; } = null!;
}