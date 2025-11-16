using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using JO2024.Core.Interfaces;

namespace JO2024.Core.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var smtpHost = _configuration["Email:SmtpHost"] ?? "mailhog";
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "1025");
                var fromEmail = _configuration["Email:FromEmail"] ?? "noreply@jo2024.local";
                var fromName = _configuration["Email:FromName"] ?? "JO 2024";

                _logger.LogInformation($"[translate:Envoi email vers]{to} [translate:via]{smtpHost}:{smtpPort}");

                using var message = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                message.To.Add(to);

                using var smtp = new SmtpClient(smtpHost, smtpPort)
                {
                    EnableSsl = false,
                    UseDefaultCredentials = false,
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };

                await smtp.SendMailAsync(message);

                _logger.LogInformation($"[translate:Email envoyé avec succès à]{to}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[translate:Erreur lors de l'envoi de l'email à]{to}");
                throw;
            }
        }

        // Méthodes Newsletter
        public async Task SendNewsletterConfirmationAsync(int userId)
        {
            _logger.LogInformation($"[translate:Envoi confirmation newsletter pour utilisateur]{userId}");

            // TODO: Implémenter l'envoi réel
            await Task.CompletedTask;
        }

        public async Task SendUnsubscribeConfirmationAsync(int userId)
        {
            _logger.LogInformation($"[translate:Envoi confirmation désinscription newsletter pour utilisateur]{userId}");

            // TODO: Implémenter l'envoi réel
            await Task.CompletedTask;
        }

        // Méthode suppression compte
        public async Task SendAccountDeletionConfirmationAsync(int userId)
        {
            _logger.LogInformation($"[translate:Envoi confirmation suppression compte pour utilisateur]{userId}");

            // TODO: Implémenter l'envoi réel
            await Task.CompletedTask;
        }

        // Ajout de la méthode obligatoire manquante
        public async Task SendWeeklyNewsletterAsync(int userId, string content)
        {
            _logger.LogInformation($"[translate:Envoi newsletter hebdomadaire pour utilisateur]{userId}");

            // TODO: Implémenter l'envoi réel avec le contenu
            await Task.CompletedTask;
        }
    }
}