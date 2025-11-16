namespace JO2024.Core.DTOs.Email
{
    // DTO de base pour envoyer un email générique
    public class EmailRequestDto
    {
        public string To { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
    }

    // Confirmation d'inscription newsletter
    public class NewsletterConfirmationDto
    {
        public int UserId { get; set; }
    }

    // Confirmation de désinscription newsletter
    public class UnsubscribeConfirmationDto
    {
        public int UserId { get; set; }
    }

    // Confirmation de suppression de compte
    public class AccountDeletionConfirmationDto
    {
        public int UserId { get; set; }
    }

    // Newsletter hebdomadaire
    public class WeeklyNewsletterDto
    {
        public int UserId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
