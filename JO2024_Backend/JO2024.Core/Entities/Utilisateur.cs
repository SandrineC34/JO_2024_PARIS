// ============================================
// Utilisateur.cs
// ============================================
using System.ComponentModel.DataAnnotations;

namespace JO2024.Core.Entities;

public class Utilisateur
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Prenom { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Nom { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string MotDePasseHash { get; set; } = string.Empty;

    public DateTime DateCreation { get; set; } = DateTime.UtcNow;
    
    public DateTime? DerniereConnexion { get; set; }
    
    public bool EstActif { get; set; } = true;

    // Clé de sécurité pour la réinitialisation du mot de passe
    public string? CleSecurite { get; set; }
    
    public string? TokenReinitialisation { get; set; }
    
    public DateTime? TokenReinitExpiration { get; set; }

    // Navigation properties
    public ICollection<Commande> Commandes { get; set; } = new List<Commande>();
    
    public ICollection<Billet> Billets { get; set; } = new List<Billet>();
}