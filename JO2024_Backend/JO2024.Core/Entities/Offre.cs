// ============================================
// Offre.cs
// ============================================
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JO2024.Core.Entities;

public class Offre
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Type { get; set; } = string.Empty; // "solo", "duo", "famille"

    [Required]
    [StringLength(200)]
    public string Nom { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Prix { get; set; }

    public int NombrePersonnes { get; set; }

    [StringLength(500)]
    public string? Caracteristiques { get; set; } // Stocké en JSON

    public bool EstActif { get; set; } = true;

    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<CommandeItem> CommandeItems { get; set; } = new List<CommandeItem>();
}
