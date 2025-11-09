using JO2024.Core.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace JO2024.Infrastructure.Configurations
{
    public class EmailConfiguration : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailConfiguration> _logger;
        private readonly IUtilisateurRepository _utilisateurRepository;

        public EmailConfiguration(
            IConfiguration configuration, 
            ILogger<EmailConfiguration> logger,
            IUtilisateurRepository utilisateurRepository)
        {
            _configuration = configuration;
            _logger = logger;
            _utilisateurRepository = utilisateurRepository;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("JO2024", "noreply@jo2024.fr"));
                message.To.Add(MailboxAddress.Parse(to));
                message.Subject = subject;
                message.Body = new TextPart("html") { Text = body };

                using var client = new SmtpClient();
                
                // Récupérer config depuis appsettings ou variables d'environnement
                var smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var smtpUser = _configuration["Email:SmtpUser"] ?? "tonmail@gmail.com";
                var smtpPass = _configuration["Email:SmtpPassword"] ?? "tonMotDePasse";
                
                await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(smtpUser, smtpPass);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                
                _logger.LogInformation("Email envoyé avec succès à {To}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'envoi de l'email à {To}", to);
                throw;
            }
        }

        public async Task SendNewsletterConfirmationAsync(int userId)
        {
            var user = await _utilisateurRepository.GetByIdAsync(userId);
            if (user == null) return;

            var subject = "Confirmation d'inscription à la newsletter JO 2024";
            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #0066cc 0%, #0052a3 100%); color: white; padding: 30px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 8px 8px; }}
        .button {{ display: inline-block; background: #0066cc; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎉 Bienvenue dans notre newsletter !</h1>
        </div>
        <div class='content'>
            <p>Bonjour {user.Prenom},</p>
            <p>Merci de vous être inscrit(e) à la newsletter des Jeux Olympiques 2024 !</p>
            <p>Vous recevrez désormais :</p>
            <ul>
                <li>📰 Les actualités des épreuves sportives</li>
                <li>🎪 Les informations sur les événements</li>
                <li>🎟️ Les offres spéciales sur les billets</li>
            </ul>
            <p>Nous sommes ravis de vous compter parmi nous pour vivre cette aventure olympique ensemble !</p>
        </div>
        <div class='footer'>
            <p>Jeux Olympiques Paris 2024<br>
            Cet email a été envoyé à {user.Email}</p>
        </div>
    </div>
</body>
</html>";

            await SendEmailAsync(user.Email, subject, body);
        }

        public async Task SendUnsubscribeConfirmationAsync(int userId)
        {
            var user = await _utilisateurRepository.GetByIdAsync(userId);
            if (user == null) return;

            var subject = "Confirmation de désinscription - Newsletter JO 2024";
            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #f44336; color: white; padding: 30px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 8px 8px; }}
        .button {{ display: inline-block; background: #0066cc; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>👋 Au revoir !</h1>
        </div>
        <div class='content'>
            <p>Bonjour {user.Prenom},</p>
            <p>Votre désinscription de la newsletter des Jeux Olympiques 2024 a bien été prise en compte.</p>
            <p>Vous ne recevrez plus nos emails d'actualités.</p>
            <p>Nous espérons vous revoir bientôt ! Vous pouvez vous réabonner à tout moment depuis votre compte.</p>
            <a href='https://www.jo2024.fr/compte.html' class='button'>Accéder à mon compte</a>
        </div>
        <div class='footer'>
            <p>Jeux Olympiques Paris 2024<br>
            Cet email a été envoyé à {user.Email}</p>
        </div>
    </div>
</body>
</html>";

            await SendEmailAsync(user.Email, subject, body);
        }

        public async Task SendAccountDeletionConfirmationAsync(int userId)
        {
            var user = await _utilisateurRepository.GetByIdAsync(userId);
            if (user == null) return;

            var subject = "Confirmation de suppression de compte - JO 2024";
            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #333; color: white; padding: 30px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 8px 8px; }}
        .warning {{ background: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🗑️ Suppression de compte</h1>
        </div>
        <div class='content'>
            <p>Bonjour {user.Prenom},</p>
            <p>Votre compte JO 2024 a été supprimé avec succès.</p>
            <div class='warning'>
                <strong>⚠️ Attention :</strong> Cette action est définitive. Toutes vos données ont été supprimées de nos systèmes.
            </div>
            <p>Si cette action n'a pas été effectuée par vous, veuillez contacter notre support immédiatement.</p>
            <p>Merci d'avoir utilisé nos services. Nous espérons vous revoir lors d'un prochain événement !</p>
        </div>
        <div class='footer'>
            <p>Jeux Olympiques Paris 2024<br>
            Support : support@jo2024.fr</p>
        </div>
    </div>
</body>
</html>";

            await SendEmailAsync(user.Email, subject, body);
        }

        public async Task SendWeeklyNewsletterAsync(int userId, string content)
        {
            var user = await _utilisateurRepository.GetByIdAsync(userId);
            if (user == null) return;

            var subject = "🏅 Newsletter hebdomadaire - JO 2024";
            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #0066cc 0%, #0052a3 100%); color: white; padding: 30px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background: white; padding: 30px; }}
        .section {{ margin: 20px 0; padding: 20px; background: #f9f9f9; border-radius: 8px; }}
        .button {{ display: inline-block; background: #0066cc; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 20px; padding: 20px; background: #f9f9f9; border-radius: 0 0 8px 8px; color: #666; font-size: 12px; }}
        .unsubscribe {{ margin-top: 10px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🏅 Newsletter JO 2024</h1>
            <p>Semaine du {DateTime.Now:dd MMMM yyyy}</p>
        </div>
        <div class='content'>
            <p>Bonjour {user.Prenom},</p>
            {content}
            <a href='https://www.jo2024.fr' class='button'>Découvrir les billets</a>
        </div>
        <div class='footer'>
            <p>Jeux Olympiques Paris 2024</p>
            <div class='unsubscribe'>
                <a href='https://www.jo2024.fr/desinscription.html?token={{UNSUBSCRIBE_TOKEN}}'>Se désinscrire</a>
            </div>
        </div>
    </div>
</body>
</html>";

            await SendEmailAsync(user.Email, subject, body);
        }
    }
}