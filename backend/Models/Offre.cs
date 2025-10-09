using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    /// <summary>
    /// Modèle pour les offres de billets (Solo, Duo, Famille)
    /// </summary>
    [Table("Offre ")]
    public class Offre
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Le code de l'offre est obligatoire")]
        [StringLength(50)]
        public string Code { get; set; } // 'solo', 'duo', 'famille'

        [Required(ErrorMessage = "Le nom de l'offre est obligatoire")]
        [StringLength(100)]
        public string Nom { get; set; } // 'Offre Solo', 'Offre Duo', 'Offre Famille'

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, 999999.99, ErrorMessage = "Le prix doit être supérieur à 0")]
        public decimal PrixTTC { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrixHT { get; set; }

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal TauxTVA { get; set; } = 20.00M; // 20% par défaut

        [Required]
        [Range(1, 100, ErrorMessage = "Le nombre de personnes doit être entre 1 et 100")]
        public int NombrePersonnes { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Economie { get; set; } // Économie par rapport aux billets individuels

        public bool EstMiseEnAvant { get; set; } = false; // Pour l'offre "featured"

        public bool EstActive { get; set; } = true;

        public int Ordre { get; set; } = 0; // Pour l'ordre d'affichage

        [Required]
        public DateTime DateCreation { get; set; } = DateTime.UtcNow;

        public DateTime? DateModification { get; set; }

        // Propriétés de navigation
        public virtual ICollection<OffreCaracteristique> Caracteristiques { get; set; }
        public virtual ICollection<Commande> Commandes { get; set; }
        public virtual ICollection<PanierItem> PanierItems { get; set; }

        // Propriétés calculées
        [NotMapped]
        public decimal PrixParPersonne => NombrePersonnes > 0 
            ? PrixTTC / NombrePersonnes 
            : PrixTTC;

        [NotMapped]
        public decimal MontantTVA => PrixTTC - PrixHT;

        [NotMapped]
        public string TypeOffre
        {
            get
            {
                return Code?.ToLower() switch
                {
                    "solo" => "Individuel",
                    "duo" => "Duo",
                    "famille" => "Famille",
                    _ => "Standard"
                };
            }
        }

        // Méthode pour calculer le prix HT à partir du TTC
        public void CalculerPrixHT()
        {
            PrixHT = PrixTTC / (1 + (TauxTVA / 100));
        }

        // Méthode pour calculer l'économie
        public void CalculerEconomie(decimal prixUnitaire)
        {
            var prixSansReduction = prixUnitaire * NombrePersonnes;
            Economie = prixSansReduction - PrixTTC;
        }
    }
}