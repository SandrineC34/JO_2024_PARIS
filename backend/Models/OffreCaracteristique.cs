using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    /// <summary>
    /// Modèle pour les caractéristiques/features des offres
    /// (liste à puces affichée dans les cartes d'offres)
    /// </summary>
    [Table("OffreCaracteristique")]
    public class OffreCaracteristique
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Offre")]
        public int OffreId { get; set; }

        [Required]
        [StringLength(200)]
        public string Texte { get; set; }

        [StringLength(50)]
        public string Icone { get; set; } // Ex: "✓", "★", emoji, etc.

        public int Ordre { get; set; } = 0;

        public bool EstMiseEnAvant { get; set; } = false; // Pour mettre en gras ou surligner

        public bool EstActive { get; set; } = true;

        [Required]
        public DateTime DateCreation { get; set; } = DateTime.UtcNow;

        // Navigation
        public virtual Offre Offre { get; set; }

        // Propriété calculée
        [NotMapped]
        public string TexteAvecIcone => 
            !string.IsNullOrWhiteSpace(Icone) 
                ? $"{Icone} {Texte}" 
                : Texte;
    }
}