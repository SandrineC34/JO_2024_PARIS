// ============================================
// Billet DTOs
// JO2024.Core/DTOs/Billets/
// ============================================
namespace JO2024.Core.DTOs.Billets;

public class BilletDto
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public string Titre { get; set; } = string.Empty;
    public string Sport { get; set; } = string.Empty;
    public string Lieu { get; set; } = string.Empty;
    public DateTime DateEpreuve { get; set; }
    public string? Place { get; set; }
    public string Statut { get; set; } = string.Empty;
    public string CodeQR { get; set; } = string.Empty;
    public DateTime? DateScan { get; set; }
    public DateTime DateCreation { get; set; }
}