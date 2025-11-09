using System.ComponentModel.DataAnnotations;

namespace JO2024.Core.DTOs.Newsletter;

/// <summary>
/// DTO pour confirmer la désinscription depuis un lien email
/// </summary>
public class UnsubscribeConfirmDto
{
    [Required(ErrorMessage = "Le token est requis")]
    [StringLength(100, MinimumLength = 10, ErrorMessage = "Token invalide")]
    public string Token { get; set; } = string.Empty;
}