using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    /// <summary>
    /// Modèle étendu pour les informations détaillées de l'utilisateur
    /// Hérite de User pour ajouter des informations supplémentaires
    /// </summary>
    [Table("Utilisateur")]
    public class Utilisateur
    {
        [Key]
        [ForeignKey("User")]
        public int UserId { get; set; }

        [StringLength(20)]
        public string Telephone { get; set; }

        [StringLength(255)]
        public string Adresse { get; set; }

        [StringLength(10)]
        public string CodePostal { get; set; }

        [StringLength(100)]
        public string Ville { get; set; }

        [StringLength(100)]
        public string Pays { get; set; } = "France";

        public DateTime? DateNaissance { get; set; }

        [StringLength(50)]
        public string Nationalite { get; set; }

        // Préférences utilisateur
        public bool NewsletterOptin { get; set; } = false;
        
        public bool NotificationsEmail { get; set; } = true;
        
        public bool NotificationsSMS { get; set; } = false;

        // Données RGPD
        public DateTime? ConsentementRGPD { get; set; }
        
        public DateTime? DerniereExportationDonnees { get; set; }

        // Statistiques
        public int NombreCommandesTotales { get; set; } = 0;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal MontantTotalDepense { get; set; } = 0;

        public DateTime? DerniereModification { get; set; } = DateTime.UtcNow;

        // Navigation vers User
        public virtual User User { get; set; }

        // Propriété calculée pour l'âge
        [NotMapped]
        public int? Age
        {
            get
            {
                if (!DateNaissance.HasValue)
                    return null;

                var today = DateTime.Today;
                var age = today.Year - DateNaissance.Value.Year;
                if (DateNaissance.Value.Date > today.AddYears(-age))
                    age--;
                return age;
            }
        }

        // Propriété calculée pour l'adresse complète
        [NotMapped]
        public string AdresseComplete => 
            string.IsNullOrWhiteSpace(Adresse) 
                ? null 
                : $"{Adresse}, {CodePostal} {Ville}, {Pays}";
    }
}