namespace JO2024.Core.DTOs.Newsletter;

/// <summary>
/// DTO pour le résultat d'envoi de newsletter
/// </summary>
public class NewsletterSendResultDto
{
    public bool Success { get; set; }
    
    public string Message { get; set; } = string.Empty;
    
    public int DestinatairesTotal { get; set; }
    
    public int EnvoisReussis { get; set; }
    
    public int EnvoisEchoues { get; set; }
    
    public DateTime DateEnvoi { get; set; }
    
    public List<string>? Erreurs { get; set; }
}