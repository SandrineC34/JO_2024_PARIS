namespace JO2024.Core.DTOs.Newsletter;

/// <summary>
/// DTO pour la réponse de désinscription
/// </summary>
public class UnsubscribeResponseDto
{
    public bool Success { get; set; }
    
    public string Message { get; set; } = string.Empty;
    
    public DateTime? DateDesabonnement { get; set; }
}