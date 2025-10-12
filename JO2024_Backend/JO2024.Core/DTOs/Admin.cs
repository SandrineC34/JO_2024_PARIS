// ============================================
// DTOs Admin
// JO2024.Core/DTOs/Admin/
// ============================================
using System.ComponentModel.DataAnnotations;

namespace JO2024.Core.DTOs.Admin;

public class UpdateRoleDto
{
    [Required]
    public string Role { get; set; } = string.Empty; // "User", "Admin", "SuperAdmin"
}

public class UpdateStatusDto
{
    [Required]
    public string Status { get; set; } = string.Empty;
}

public class CreateOffreDto
{
    [Required]
    public string Type { get; set; } = string.Empty;

    [Required]
    public string Nom { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    [Range(0.01, 10000)]
    public decimal Prix { get; set; }

    [Required]
    [Range(1, 100)]
    public int NombrePersonnes { get; set; }
}

public class UpdateOffreDto
{
    [Required]
    public string Nom { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    [Range(0.01, 10000)]
    public decimal Prix { get; set; }

    public bool EstActif { get; set; }
}

public class UserDetailsDto
{
    public int Id { get; set; }
    public string Prenom { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool EstActif { get; set; }
    public DateTime DateCreation { get; set; }
    public DateTime? DerniereConnexion { get; set; }
    public int NombreCommandes { get; set; }
    public int NombreBillets { get; set; }
    public decimal TotalDepense { get; set; }
}

public class DashboardStatsDto
{
    public int TotalUtilisateurs { get; set; }
    public int UtilisateursActifs { get; set; }
    public int TotalCommandes { get; set; }
    public int TotalBillets { get; set; }
    public decimal ChiffreAffaireTotal { get; set; }
    public decimal ChiffreAffaireMoisActuel { get; set; }
    public Dictionary<string, int> VentesParOffre { get; set; } = new();
    public Dictionary<string, int> VentesParSport { get; set; } = new();
}

public class SalesStatsDto
{
    public DateTime Date { get; set; }
    public int NombreVentes { get; set; }
    public decimal Montant { get; set; }
}