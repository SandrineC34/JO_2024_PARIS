// ============================================
// Commande DTOs
// JO2024.Core/DTOs/Commandes/
// ============================================
using System.ComponentModel.DataAnnotations;

namespace JO2024.Core.DTOs.Commandes;

public class CreateCommandeDto
{
    [Required]
    public List<CommandeItemDto> Items { get; set; } = new();
}

public class CommandeItemDto
{
    [Required]
    public int OffreId { get; set; }

    [Required]
    [Range(1, 10, ErrorMessage = "La quantité doit être entre 1 et 10")]
    public int Quantite { get; set; }

    [Required]
    public string Sport { get; set; } = string.Empty;
}

public class CommandeDto
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public DateTime DateAchat { get; set; }
    public decimal MontantHT { get; set; }
    public decimal MontantTVA { get; set; }
    public decimal MontantTotal { get; set; }
    public string Statut { get; set; } = string.Empty;
    public List<CommandeItemDetailDto> Items { get; set; } = new();
}

public class CommandeItemDetailDto
{
    public int Id { get; set; }
    public string OffreNom { get; set; } = string.Empty;
    public int Quantite { get; set; }
    public decimal Prix { get; set; }
    public string Sport { get; set; } = string.Empty;
}

public class CommandeResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public CommandeDto? Commande { get; set; }
    public List<int> BilletIds { get; set; } = new();
}