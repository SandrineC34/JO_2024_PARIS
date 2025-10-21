// ============================================
// AuthController.cs
// ============================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using JO2024.Core.Interfaces;
using JO2024.Core.DTOs.Auth;

namespace JO2024.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        try
        {
            var result = await _authService.RegisterAsync(registerDto);
            
            if (!result.Success)
                return BadRequest(result);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'inscription");
            return StatusCode(500, new { message = "Une erreur s'est produite lors de l'inscription" });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            var result = await _authService.LoginAsync(loginDto);
            
            if (!result.Success)
                return Unauthorized(result);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la connexion");
            return StatusCode(500, new { message = "Une erreur s'est produite lors de la connexion" });
        }
    }

    [Authorize]
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            var userId = GetUserIdFromClaims();
            var user = await _authService.GetCurrentUserAsync(userId);
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de l'utilisateur");
            return StatusCode(500, new { message = "Une erreur s'est produite" });
        }
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
    {
        try
        {
            var userId = GetUserIdFromClaims();
            var success = await _authService.ChangePasswordAsync(userId, changePasswordDto);
            
            if (!success)
                return BadRequest(new { message = "Mot de passe actuel incorrect" });
            
            return Ok(new { message = "Mot de passe modifié avec succès" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du changement de mot de passe");
            return StatusCode(500, new { message = "Une erreur s'est produite" });
        }
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
    {
        try
        {
            await _authService.ResetPasswordRequestAsync(forgotPasswordDto.Email);
            // Toujours retourner success pour ne pas révéler si l'email existe
            return Ok(new { message = "Si cet email existe, un lien de réinitialisation a été envoyé" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la demande de réinitialisation");
            return StatusCode(500, new { message = "Une erreur s'est produite" });
        }
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
    {
        try
        {
            var success = await _authService.ResetPasswordAsync(resetPasswordDto);
            
            if (!success)
                return BadRequest(new { message = "Token invalide ou expiré" });
            
            return Ok(new { message = "Mot de passe réinitialisé avec succès" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la réinitialisation du mot de passe");
            return StatusCode(500, new { message = "Une erreur s'est produite" });
        }
    }

    private int GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim ?? "0");
    }
}

public class ForgotPasswordDto
{
    public string Email { get; set; } = string.Empty;
}