// ============================================
// Commande.cs
// ============================================
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JO2024.Core.Entities;

public class Commande
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Numero { get; set; } = string.Empty; // Format: CMD-YYYYMMDD-XXXXX

    public int UtilisateurId { get; set; }
    
    public DateTime DateAchat { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "decimal(10,2)")]
    public decimal MontantHT { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal MontantTVA { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal MontantTotal { get; set; }

    [Required]
    [StringLength(50)]
    public string Statut { get; set; } = "Payée"; // "Payée", "Utilisée", "Annulée"

    [StringLength(100)]
    public string? MethodePaiement { get; set; }

    // Navigation properties
    public Utilisateur Utilisateur { get; set; } = null!;
    
    public ICollection<CommandeItem> Items { get; set; } = new List<CommandeItem>();
    
    public ICollection<Billet> Billets { get; set; } = new List<Billet>();
}
