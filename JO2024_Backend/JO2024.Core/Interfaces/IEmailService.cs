namespace JO2024.Core.Interfaces
{
    public interface IEmailService
    {
        // Méthode de base
        Task SendEmailAsync(string to, string subject, string body);
        
        // Emails Newsletter
        Task SendNewsletterConfirmationAsync(int userId);
        Task SendUnsubscribeConfirmationAsync(int userId);
        
        // Email suppression de compte (utilisé dans CompteController)
        Task SendAccountDeletionConfirmationAsync(int userId);
        
        // Email newsletter hebdomadaire (pour le job planifié)
        Task SendWeeklyNewsletterAsync(int userId, string content);
    }
}
