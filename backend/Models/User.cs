using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    /// <summary>
    /// Modèle pour l'authentification et les données utilisateur de base
    /// </summary>
    [Table("User")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Le prénom est obligatoire")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Le prénom doit contenir entre 2 et 100 caractères")]
        [RegularExpression(@"^[a-zA-ZÀ-ÿ\s\-']+$", ErrorMessage = "Le prénom ne peut contenir que des lettres, espaces, tirets et apostrophes")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Le nom est obligatoire")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Le nom doit contenir entre 2 et 100 caractères")]
        [RegularExpression(@"^[a-zA-ZÀ-ÿ\s\-']+$", ErrorMessage = "Le nom ne peut contenir que des lettres, espaces, tirets et apostrophes")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "L'email est obligatoire")]
        [EmailAddress(ErrorMessage = "Format d'email invalide")]
        [StringLength(255)]
        public string Email { get; set; }

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; }

        [StringLength(100)]
        public string SecurityKey { get; set; }

        [StringLength(255)]
        public string ResetToken { get; set; }

        public DateTime? ResetTokenExpiry { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLogin { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        // Propriétés de navigation
        public virtual ICollection<Commande> Commandes { get; set; }
        public virtual ICollection<Billet> Billets { get; set; }
        public virtual ICollection<PanierItem> PanierItems { get; set; }

        // Propriété calculée pour le nom complet
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";
    }
}