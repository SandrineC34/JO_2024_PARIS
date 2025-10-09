using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    /// <summary>
    /// Modèle pour la gestion du compte utilisateur (profil, billets, commandes, sécurité)
    /// Compatible avec compte.html et compte.js
    /// </summary>
    [Table("Compte")]
    public class Compte
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // 🔹 Informations de profil
        [Required(ErrorMessage = "Le prénom est obligatoire")]
        [StringLength(100, MinimumLength = 2)]
        [RegularExpression(@"^[a-zA-ZÀ-ÿ\s\-']+$", ErrorMessage = "Caractères invalides dans le prénom")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Le nom est obligatoire")]
        [StringLength(100, MinimumLength = 2)]
        [RegularExpression(@"^[a-zA-ZÀ-ÿ\s\-']+$", ErrorMessage = "Caractères invalides dans le nom")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "L'adresse email est obligatoire")]
        [EmailAddress(ErrorMessage = "Format d'email invalide")]
        [StringLength(255)]
        public string Email { get; set; }

        // 🔹 Sécurité & connexion
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

        // 🔹 Données du compte (liées aux pages compte.html / compte.js)
        public virtual ICollection<Billet> Billets { get; set; } = new List<Billet>();
        public virtual ICollection<Commande> Commandes { get; set; } = new List<Commande>();

        // 🔹 Journal et actions RGPD
        [StringLength(255)]
        public string LastAction { get; set; } // ex: "Profil modifié", "Mot de passe changé"

        public DateTime? LastUpdate { get; set; } // Date de dernière modification du profil

        // 🔹 Propriété calculée pour affichage
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";
    }
}
