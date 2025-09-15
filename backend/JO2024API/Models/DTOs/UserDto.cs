using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace JeuxOlympiques.Models.DTOs
{
    // DTO pour l'affichage des informations utilisateur
    public class UserDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // DTO pour la mise à jour des informations utilisateur
    public class UpdateUserDto
    {
        [Required(ErrorMessage = "Le prénom est obligatoire")]
        [StringLength(50, ErrorMessage = "Le prénom ne peut pas dépasser 50 caractères")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Le nom est obligatoire")]
        [StringLength(50, ErrorMessage = "Le nom ne peut pas dépasser 50 caractères")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "L'email est obligatoire")]
        [EmailAddress(ErrorMessage = "Format d'email invalide")]
        [StringLength(100, ErrorMessage = "L'email ne peut pas dépasser 100 caractères")]
        public string Email { get; set; }
    }

    // DTO pour le changement de mot de passe
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Le mot de passe actuel est obligatoire")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "Le nouveau mot de passe est obligatoire")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Le mot de passe doit contenir au moins 8 caractères")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*])[A-Za-z\d!@#$%^&*]{8,}$", 
            ErrorMessage = "Le mot de passe doit contenir au moins une majuscule, une minuscule, un chiffre et un caractère spécial")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "La confirmation du mot de passe est obligatoire")]
        [Compare("NewPassword", ErrorMessage = "Les mots de passe ne correspondent pas")]
        public string ConfirmPassword { get; set; }
    }

    // DTO pour les billets
    public class TicketDto
    {
        public int Id { get; set; }
        public string TicketNumber { get; set; }
        public string EventName { get; set; }
        public DateTime EventDate { get; set; }
        public string Venue { get; set; }
        public string Section { get; set; }
        public string Row { get; set; }
        public string Seat { get; set; }
        public string Status { get; set; }
        public DateTime? ScannedAt { get; set; }
        public string QRCode { get; set; }
        
        // Propriétés formatées pour l'affichage
        public string FormattedDate => EventDate.ToString("dd MMMM yyyy, HH:mm");
        public string FullSeatInfo => $"Secteur {Section} - Rangée {Row} - Siège {Seat}";
        public bool IsActive => Status == "Actif";
        public bool IsScanned => Status == "Scanné";
    }

    // DTO pour les commandes
    public class OrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public List<TicketDto> Tickets { get; set; } = new List<TicketDto>();
        
        // Propriétés formatées pour l'affichage
        public string FormattedDate => OrderDate.ToString("dd/MM/yyyy");
        public string FormattedTime => OrderDate.ToString("HH:mm");
        public string FormattedAmount => $"{TotalAmount:C}";
    }

    // DTO pour les données d'export RGPD
    public class UserDataExportDto
    {
        public UserDto User { get; set; }
        public List<OrderDto> Orders { get; set; } = new List<OrderDto>();
        public List<TicketDto> Tickets { get; set; } = new List<TicketDto>();
        public DateTime ExportDate { get; set; } = DateTime.Now;
    }

    // DTO pour les réponses API
    public class ApiResponseDto<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}