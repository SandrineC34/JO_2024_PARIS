using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using JO2024.Core.Interfaces;
using JO2024.Core.DTOs.Auth;
using JO2024.Core.Entities;

namespace JO2024.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUtilisateurRepository _utilisateurRepository;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUtilisateurRepository utilisateurRepository,
            IConfiguration configuration,
            IEmailService emailService,
            ILogger<AuthService> logger)
        {
            _utilisateurRepository = utilisateurRepository;
            _configuration = configuration;
            _emailService = emailService;
            _logger = logger;
        }

        // ============================================
        // INSCRIPTION - RETOURNE AuthResponseDto
        // ============================================

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                _logger.LogInformation($"🔍 Tentative d'inscription pour {registerDto.Email}");

                // Vérifier si l'email existe déjà 
                var existingUser = await _utilisateurRepository.GetByEmailAsync(registerDto.Email);
                if (existingUser != null)
                {
                    _logger.LogWarning($"⚠️ Email déjà utilisé : {registerDto.Email}");
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Cette adresse email est déjà utilisée"
                    };
                }

                // Créer le nouvel utilisateur
                var newUser = new Utilisateur
                {
                    Email = registerDto.Email.ToLower().Trim(),
                    Nom = registerDto.Nom?.Trim() ?? "",
                    Prenom = registerDto.Prenom?.Trim() ?? "",
                    MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                    DateCreation = DateTime.UtcNow,
                    EstActif = true,
                    Role = "Utilisateur",
                    // Newsletter
                    NewsletterAbonne = registerDto.NewsletterPreferences?.Subscribed ?? false,
                    NewsletterCategories = registerDto.NewsletterPreferences?.Categories != null 
                        ? System.Text.Json.JsonSerializer.Serialize(registerDto.NewsletterPreferences.Categories)
                        : null,
                    NewsletterSports = registerDto.NewsletterPreferences?.Sports != null && registerDto.NewsletterPreferences.Sports.Any()
                        ? System.Text.Json.JsonSerializer.Serialize(registerDto.NewsletterPreferences.Sports)
                        : null
                };

                await _utilisateurRepository.AddAsync(newUser);
                await _utilisateurRepository.SaveChangesAsync();

                _logger.LogInformation($"✅ Utilisateur créé avec succès : {newUser.Email} (ID: {newUser.Id})");

                // Envoi de l'email de bienvenue
                try
                {
                    var emailBody = GenerateWelcomeEmail(newUser, registerDto.NewsletterPreferences);
                    
                    await _emailService.SendEmailAsync(
                        newUser.Email,
                        "🎉 Bienvenue sur la plateforme JO 2024 !",
                        emailBody
                    );

                    _logger.LogInformation($"✅ Email de bienvenue envoyé à {newUser.Email}");
                }
                catch (Exception emailEx)
                {
                    _logger.LogWarning(emailEx, $"⚠️ Impossible d'envoyer l'email de bienvenue à {newUser.Email}");
                }

                // Générer le token JWT
                var token = GenerateJwtToken(newUser);

                return new AuthResponseDto
                {
                    Success = true,
                    Token = token,
                    Message = "Inscription réussie ! Un email de bienvenue vous a été envoyé.",
                    User = new UtilisateurDto
                    {
                        Id = newUser.Id,
                        Email = newUser.Email,
                        Nom = newUser.Nom,
                        Prenom = newUser.Prenom
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Erreur lors de l'inscription pour {registerDto.Email}");
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Une erreur s'est produite lors de l'inscription"
                };
            }
        }
                // ============================================
                // ENVOI DE L'EMAIL DE BIENVENUE
                // ============================================


        private string GenerateWelcomeEmail(Utilisateur user, NewsletterPreferencesDto? newsletterPreferences)
        {
            var newsletterInfo = "";
            if (newsletterPreferences?.Subscribed == true)
            {
                newsletterInfo = "<p>📧 <strong>Newsletter :</strong> Vous êtes inscrit à notre newsletter !</p>";
            }

            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background: linear-gradient(135deg, #004e92 0%, #000428 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
                        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
                        .button {{ display: inline-block; padding: 12px 30px; background: #004e92; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
                        .features {{ background: white; padding: 20px; border-radius: 8px; margin: 20px 0; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>🎉 Bienvenue aux JO 2024 !</h1>
                        </div>
                        <div class='content'>
                            <h2>Bonjour {user.Prenom} {user.Nom},</h2>
                            <p>Félicitations ! Votre compte a été créé avec succès.</p>
                            
                            <div class='features'>
                                <h3>📋 Informations de votre compte</h3>
                                <p><strong>Email :</strong> {user.Email}</p>
                                <p><strong>Date de création :</strong> {DateTime.Now:dd/MM/yyyy à HH:mm}</p>
                                {newsletterInfo}
                            </div>

                            <p><strong>Vous pouvez maintenant :</strong></p>
                            <ul>
                                <li>✅ Réserver vos billets pour les JO 2024</li>
                                <li>✅ Gérer vos commandes</li>
                                <li>✅ Télécharger vos billets électroniques</li>
                            </ul>

                            <a href='http://localhost:3000/connexion.html' class='button'>
                                🔐 Se connecter
                            </a>

                            <p style='margin-top: 30px; color: #666; font-size: 14px;'>
                                Environnement de développement - MailHog: <a href='http://localhost:8025'>http://localhost:8025</a>
                            </p>
                        </div>
                    </div>
                </body>
                </html>
            ";
        }  

        // ============================================
        // CONNEXION - RETOURNE AuthResponseDto
        // ============================================
        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            try
            {
                _logger.LogInformation($"🔍 Tentative de connexion pour {loginDto.Email}");

                var user = await _utilisateurRepository.GetByEmailAsync(loginDto.Email);
                
                if (user == null)
                {
                    _logger.LogWarning($"⚠️ Utilisateur non trouvé : {loginDto.Email}");
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Email ou mot de passe incorrect"
                    };
                }

                if (!user.EstActif)
                {
                    _logger.LogWarning($"⚠️ Compte désactivé : {loginDto.Email}");
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Ce compte a été désactivé"
                    };
                }

                if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.MotDePasseHash))
                {
                    _logger.LogWarning($"⚠️ Mot de passe incorrect pour {loginDto.Email}");
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Email ou mot de passe incorrect"
                    };
                }

                // Mettre à jour la dernière connexion
                user.DerniereConnexion = DateTime.UtcNow;
                await _utilisateurRepository.UpdateAsync(user);
                await _utilisateurRepository.SaveChangesAsync();

                var token = GenerateJwtToken(user);

                _logger.LogInformation($"✅ Connexion réussie pour {user.Email}");

                return new AuthResponseDto
                {
                    Success = true,
                    Token = token,
                    Message = "Connexion réussie",
                    User = new UtilisateurDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        Nom = user.Nom,
                        Prenom = user.Prenom
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Erreur lors de la connexion pour {loginDto.Email}");
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Une erreur s'est produite lors de la connexion"
                };
            }
        }

        // ============================================
        // RÉCUPÉRER L'UTILISATEUR ACTUEL - RETOURNE UtilisateurDto
        // ============================================
        public async Task<UtilisateurDto> GetCurrentUserAsync(int userId)
        {
            try
            {
                var user = await _utilisateurRepository.GetByIdAsync(userId);
                
                if (user == null)
                {
                    throw new Exception("Utilisateur non trouvé");
                }

                return new UtilisateurDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Nom = user.Nom,
                    Prenom = user.Prenom
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Erreur lors de la récupération de l'utilisateur {userId}");
                throw;
            }
        }

        // ============================================
        // CHANGEMENT DE MOT DE PASSE
        // ============================================
        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            try
            {
                var user = await _utilisateurRepository.GetByIdAsync(userId);
                
                if (user == null)
                {
                    _logger.LogWarning($"⚠️ Utilisateur {userId} non trouvé");
                    return false;
                }

                // Vérifier l'ancien mot de passe
                if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.MotDePasseHash))
                {
                    _logger.LogWarning($"⚠️ Mot de passe actuel incorrect pour utilisateur {userId}");
                    return false;
                }

                // Hasher et sauvegarder le nouveau mot de passe
                user.MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
                await _utilisateurRepository.UpdateAsync(user);
                await _utilisateurRepository.SaveChangesAsync();

                _logger.LogInformation($"✅ Mot de passe modifié pour utilisateur {userId}");

                // Envoyer un email de confirmation
                try
                {
                    var emailBody = GeneratePasswordChangedEmail(user);
                    await _emailService.SendEmailAsync(
                        user.Email,
                        "🔒 Confirmation de modification de mot de passe",
                        emailBody
                    );
                }
                catch (Exception emailEx)
                {
                    _logger.LogWarning(emailEx, "⚠️ Impossible d'envoyer l'email de confirmation");
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Erreur lors du changement de mot de passe pour {userId}");
                return false;
            }
        }

        // ============================================
        // RÉINITIALISATION DE MOT DE PASSE - RETOURNE bool
        // ============================================
        public async Task<bool> ResetPasswordRequestAsync(string email)
        {
            try
            {
                var user = await _utilisateurRepository.GetByEmailAsync(email);
                
                if (user == null)
                {
                    _logger.LogWarning($"⚠️ Demande de reset pour email inexistant : {email}");
                    // Retourner true pour ne pas révéler si l'email existe
                    return true;
                }

                // Générer un token de réinitialisation
                var resetToken = Guid.NewGuid().ToString("N");
                user.ResetPasswordToken = resetToken;
                user.ResetPasswordExpiry = DateTime.UtcNow.AddHours(1);
                
                await _utilisateurRepository.UpdateAsync(user);
                await _utilisateurRepository.SaveChangesAsync();

                // Envoyer l'email avec le lien
                var resetLink = $"http://localhost:3000/reset-password.html?token={resetToken}";
                
                var emailBody = GenerateResetPasswordEmail(user, resetLink);

                await _emailService.SendEmailAsync(
                    user.Email,
                    "🔒 Réinitialisation de votre mot de passe - JO 2024",
                    emailBody
                );

                _logger.LogInformation($"✅ Email de réinitialisation envoyé à {email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Erreur lors de la demande de reset pour {email}");
                return false;
            }
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            try
            {
                var user = await _utilisateurRepository.GetByResetTokenAsync(resetPasswordDto.Token);
                
                if (user == null)
                {
                    _logger.LogWarning($"⚠️ Token de reset invalide");
                    return false;
                }

                if (user.ResetPasswordExpiry < DateTime.UtcNow)
                {
                    _logger.LogWarning($"⚠️ Token de reset expiré pour {user.Email}");
                    return false;
                }

                // Réinitialiser le mot de passe
                user.MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(resetPasswordDto.NewPassword);
                user.ResetPasswordToken = null;
                user.ResetPasswordExpiry = null;
                
                await _utilisateurRepository.UpdateAsync(user);
                await _utilisateurRepository.SaveChangesAsync();

                _logger.LogInformation($"✅ Mot de passe réinitialisé pour {user.Email}");

                // Envoyer email de confirmation
                try
                {
                    var emailBody = GeneratePasswordResetConfirmationEmail(user);
                    await _emailService.SendEmailAsync(
                        user.Email,
                        "✅ Votre mot de passe a été réinitialisé",
                        emailBody
                    );
                }
                catch (Exception emailEx)
                {
                    _logger.LogWarning(emailEx, "⚠️ Impossible d'envoyer l'email de confirmation");
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors de la réinitialisation du mot de passe");
                return false;
            }
        }

        // ============================================
        // GÉNÉRATION DU TOKEN JWT
        // ============================================
        private string GenerateJwtToken(Utilisateur user)
        {
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "MaCléParDéfautTrèsSécurisée123!");
            var tokenHandler = new JwtSecurityTokenHandler();
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, $"{user.Prenom} {user.Nom}"),
                    new Claim(ClaimTypes.Role, user.Role ?? "Utilisateur"), // ✅ Ajout du rôle
                    new Claim("prenom", user.Prenom ?? ""),
                    new Claim("nom", user.Nom ?? "")
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        // ============================================
        // TEMPLATES D'EMAILS
        // ============================================
        
        private string GenerateWelcomeEmail(Utilisateur user, dynamic newsletterPreferences)
        {
            var newsletterInfo = "";
            if (newsletterPreferences?.Subscribed == true)
            {
                newsletterInfo = "<p>📧 <strong>Newsletter :</strong> Vous êtes inscrit à notre newsletter !</p>";
            }

            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background: linear-gradient(135deg, #004e92 0%, #000428 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
                        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
                        .button {{ display: inline-block; padding: 12px 30px; background: #004e92; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
                        .features {{ background: white; padding: 20px; border-radius: 8px; margin: 20px 0; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>🎉 Bienvenue aux JO 2024 !</h1>
                        </div>
                        <div class='content'>
                            <h2>Bonjour {user.Prenom} {user.Nom},</h2>
                            <p>Félicitations ! Votre compte a été créé avec succès.</p>
                            
                            <div class='features'>
                                <h3>📋 Informations de votre compte</h3>
                                <p><strong>Email :</strong> {user.Email}</p>
                                <p><strong>Date de création :</strong> {DateTime.Now:dd/MM/yyyy à HH:mm}</p>
                                {newsletterInfo}
                            </div>

                            <p><strong>Vous pouvez maintenant :</strong></p>
                            <ul>
                                <li>✅ Réserver vos billets pour les JO 2024</li>
                                <li>✅ Gérer vos commandes</li>
                                <li>✅ Télécharger vos billets électroniques</li>
                            </ul>

                            <a href='http://localhost:3000/connexion.html' class='button'>
                                🔐 Se connecter
                            </a>

                            <p style='margin-top: 30px; color: #666; font-size: 14px;'>
                                Environnement de développement - MailHog: <a href='http://localhost:8025'>http://localhost:8025</a>
                            </p>
                        </div>
                    </div>
                </body>
                </html>
            ";
        }

        private string GeneratePasswordChangedEmail(Utilisateur user)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background: linear-gradient(135deg, #004e92 0%, #000428 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
                        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
                        .warning {{ background: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>🔒 Mot de passe modifié</h1>
                        </div>
                        <div class='content'>
                            <h2>Bonjour {user.Prenom},</h2>
                            <p>Votre mot de passe a été modifié avec succès.</p>
                            <div class='warning'>
                                <strong>⚠️ Vous n'êtes pas à l'origine de cette modification ?</strong><br>
                                Contactez immédiatement notre support.
                            </div>
                            <p>Date : {DateTime.Now:dd/MM/yyyy HH:mm}</p>
                        </div>
                    </div>
                </body>
                </html>
            ";
        }

        private string GenerateResetPasswordEmail(Utilisateur user, string resetLink)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background: linear-gradient(135deg, #004e92 0%, #000428 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
                        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
                        .button {{ display: inline-block; padding: 12px 30px; background: #004e92; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
                        .warning {{ background: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>🔒 Réinitialisation de mot de passe</h1>
                        </div>
                        <div class='content'>
                            <h2>Bonjour {user.Prenom},</h2>
                            <p>Vous avez demandé à réinitialiser votre mot de passe.</p>
                            <a href='{resetLink}' class='button'>Réinitialiser mon mot de passe</a>
                            <div class='warning'>
                                <strong>⚠️ Ce lien expire dans 1 heure</strong>
                            </div>
                            <p style='font-size: 14px; color: #666;'>
                                Si vous n'avez pas fait cette demande, ignorez cet email.
                            </p>
                        </div>
                    </div>
                </body>
                </html>
            ";
        }

        private string GeneratePasswordResetConfirmationEmail(Utilisateur user)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background: linear-gradient(135deg, #004e92 0%, #000428 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
                        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>✅ Mot de passe réinitialisé</h1>
                        </div>
                        <div class='content'>
                            <h2>Bonjour {user.Prenom},</h2>
                            <p>Votre mot de passe a été réinitialisé avec succès.</p>
                            <a href='http://localhost:3000/connexion.html' style='display: inline-block; padding: 12px 30px; background: #004e92; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0;'>
                                Se connecter
                            </a>
                        </div>
                    </div>
                </body>
                </html>
            ";
        }
    }
}