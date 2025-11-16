using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JO2024.Core.Entities
{
    [Table("Utilisateurs")]
    public class Utilisateur
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Prenom { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Nom { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string MotDePasseHash { get; set; } = string.Empty;

        public DateTime DateCreation { get; set; } = DateTime.UtcNow;
        
        public DateTime? DerniereConnexion { get; set; }

        public bool EstActif { get; set; } = true;

        // ✅ Propriété Role (manquante!)
        [Required]
        [StringLength(50)]
        public string Role { get; set; } = "Utilisateur";

        // Propriétés pour la réinitialisation de mot de passe
        [StringLength(255)]
        public string? ResetPasswordToken { get; set; }
        
        public DateTime? ResetPasswordExpiry { get; set; }

        // Propriétés pour la newsletter
        public bool NewsletterAbonne { get; set; } = false;

        [Column(TypeName = "nvarchar(max)")]
        public string? NewsletterCategories { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? NewsletterSports { get; set; }

        [StringLength(255)]
        public string? NewsletterUnsubscribeToken { get; set; }

        public DateTime? NewsletterDateInscription { get; set; }

        public DateTime? NewsletterDateDesinscription { get; set; }

        // Relations
        public virtual ICollection<Commande>? Commandes { get; set; }
        public virtual ICollection<Billet>? Billets { get; set; }
    }
}