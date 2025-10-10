// ============================================
// AuthService.cs
// ============================================
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;
using JO2024.Core.Entities;
using JO2024.Core.Interfaces;
using JO2024.Core.DTOs.Auth;

namespace JO2024.Core.Services;

public class AuthService : IAuthService
{
    private readonly IUtilisateurRepository _utilisateurRepository;
    private readonly IConfiguration _configuration;

    public AuthService(IUtilisateurRepository utilisateurRepository, IConfiguration configuration)
    {
        _utilisateurRepository = utilisateurRepository;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
    {
        // Vérifier si l'email existe déjà
        if (await _utilisateurRepository.EmailExistsAsync(registerDto.Email))
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "Cette adresse email est déjà utilisée"
            };
        }

        // Créer le nouvel utilisateur
        var utilisateur = new Utilisateur
        {
            Prenom = registerDto.Prenom,
            Nom = registerDto.Nom,
            Email = registerDto.Email.ToLower(),
            MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
            DateCreation = DateTime.UtcNow,
            CleSecurite = GenerateSecurityKey(),
            EstActif = true
        };

        utilisateur = await _utilisateurRepository.AddAsync(utilisateur);

        // Générer le token JWT
        var token = GenerateJwtToken(utilisateur);

        return new AuthResponseDto
        {
            Success = true,
            Message = "Compte créé avec succès",
            Token = token,
            User = MapToUtilisateurDto(utilisateur)
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        // Rechercher l'utilisateur
        var utilisateur = await _utilisateurRepository.GetByEmailAsync(loginDto.Email);

        if (utilisateur == null)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "Email ou mot de passe incorrect"
            };
        }

        // Vérifier le mot de passe
        if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, utilisateur.MotDePasseHash))
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "Email ou mot de passe incorrect"
            };
        }

        if (!utilisateur.EstActif)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "Votre compte a été désactivé"
            };
        }

        // Mettre à jour la dernière connexion
        utilisateur.DerniereConnexion = DateTime.UtcNow;
        await _utilisateurRepository.UpdateAsync(utilisateur);

        // Générer le token JWT
        var token = GenerateJwtToken(utilisateur);

        return new AuthResponseDto
        {
            Success = true,
            Message = "Connexion réussie",
            Token = token,
            User = MapToUtilisateurDto(utilisateur)
        };
    }

    public async Task<UtilisateurDto> GetCurrentUserAsync(int userId)
    {
        var utilisateur = await _utilisateurRepository.GetByIdAsync(userId);
        
        if (utilisateur == null)
            throw new Exception("Utilisateur non trouvé");
        
        return MapToUtilisateurDto(utilisateur);
    }

    public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
    {
        var utilisateur = await _utilisateurRepository.GetByIdAsync(userId);
        
        if (utilisateur == null)
            return false;
        
        // Vérifier l'ancien mot de passe
        if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, utilisateur.MotDePasseHash))
            return false;
        
        // Mettre à jour le mot de passe
        utilisateur.MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
        await _utilisateurRepository.UpdateAsync(utilisateur);
        
        return true;
    }

    public async Task<bool> ResetPasswordRequestAsync(string email)
    {
        var utilisateur = await _utilisateurRepository.GetByEmailAsync(email);
        
        if (utilisateur == null)
            return false; // Ne pas révéler si l'email existe
        
        // Générer un token de réinitialisation
        utilisateur.TokenReinitialisation = GenerateResetToken();
        utilisateur.TokenReinitExpiration = DateTime.UtcNow.AddHours(1);
        
        await _utilisateurRepository.UpdateAsync(utilisateur);
        
        // TODO: Envoyer l'email avec le lien de réinitialisation
        
        return true;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
    {
        var utilisateur = await _utilisateurRepository.GetByEmailAsync(resetPasswordDto.Email);
        
        if (utilisateur == null || 
            utilisateur.TokenReinitialisation != resetPasswordDto.Token ||
            utilisateur.TokenReinitExpiration == null ||
            utilisateur.TokenReinitExpiration < DateTime.UtcNow)
        {
            return false;
        }
        
        // Mettre à jour le mot de passe
        utilisateur.MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(resetPasswordDto.NewPassword);
        utilisateur.TokenReinitialisation = null;
        utilisateur.TokenReinitExpiration = null;
        
        await _utilisateurRepository.UpdateAsync(utilisateur);
        
        return true;
    }

    private string GenerateJwtToken(Utilisateur utilisateur)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
        var jwtAudience = _configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");
        var expirationMinutes = int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "1440");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, utilisateur.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, utilisateur.Email),
            new Claim(JwtRegisteredClaimNames.GivenName, utilisateur.Prenom),
            new Claim(JwtRegisteredClaimNames.FamilyName, utilisateur.Nom),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private UtilisateurDto MapToUtilisateurDto(Utilisateur utilisateur)
    {
        return new UtilisateurDto
        {
            Id = utilisateur.Id,
            Prenom = utilisateur.Prenom,
            Nom = utilisateur.Nom,
            Email = utilisateur.Email,
            DateCreation = utilisateur.DateCreation,
            DerniereConnexion = utilisateur.DerniereConnexion
        };
    }

    private string GenerateSecurityKey()
    {
        return $"sk_{Guid.NewGuid():N}_{DateTime.UtcNow.Ticks}";
    }

    private string GenerateResetToken()
    {
        return $"rst_{Guid.NewGuid():N}";
    }
}