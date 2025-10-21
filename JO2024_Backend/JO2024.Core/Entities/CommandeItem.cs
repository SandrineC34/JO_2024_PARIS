// ============================================
// CommandeItem.cs
// ============================================
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JO2024.Core.Entities;

public class CommandeItem
{
    public int Id { get; set; }

    public int CommandeId { get; set; }
    
    public int OffreId { get; set; }

    public int Quantite { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal PrixUnitaire { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal PrixTotal { get; set; }

    [StringLength(100)]
    public string? Sport { get; set; }

    // Navigation properties
    public Commande Commande { get; set; } = null!;
    
    public Offre Offre { get; set; } = null!;
}
