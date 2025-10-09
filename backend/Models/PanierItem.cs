using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    /// <summary>
    /// Modèle pour les articles dans le panier d'achat
    /// </summary>
    [Table("PanierItem")]
    public class PanierItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }

        [Required]
        [ForeignKey("Offre")]
        public int OffreId { get; set; }

        [Required]
        [ForeignKey("SportOption")]
        public int SportOptionId { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "La quantité doit être entre 1 et 100")]
        public int Quantite { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrixUnitaire { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrixTotal { get; set; }

        public bool EstSelectionne { get; set; } = false;

        [Required]
        public DateTime DateAjout { get; set; } = DateTime.UtcNow;

        public DateTime? DateModification { get; set; }

        // Session info (pour panier temporaire non authentifié)
        [StringLength(100)]
        public string SessionId { get; set; }

        public DateTime? DateExpiration { get; set; }

        // Navigation
        public virtual User User { get; set; }
        public virtual Offre Offre { get; set; }
        public virtual SportOption SportOption { get; set; }

        // Propriétés calculées
        [NotMapped]
        public decimal SousTotal => PrixUnitaire * Quantite;

        [NotMapped]
        public decimal SousTotalHT
        {
            get
            {
                const decimal tauxTVA = 20.00M;
                return SousTotal / (1 + (tauxTVA / 100));
            }
        }

        [NotMapped]
        public decimal MontantTVA => SousTotal - SousTotalHT;

        [NotMapped]
        public int NombrePersonnesTotal => Offre != null ? Offre.NombrePersonnes * Quantite : Quantite;

        [NotMapped]
        public bool EstExpire => DateExpiration.HasValue && DateExpiration.Value < DateTime.UtcNow;

        [NotMapped]
        public string Description
        {
            get
            {
                if (Offre == null || SportOption == null)
                    return "Article";

                return $"{Offre.Nom} - {SportOption.Nom} ({NombrePersonnesTotal} personne{(NombrePersonnesTotal > 1 ? "s" : "")})";
            }
        }

        [NotMapped]
        public string DescriptionCourte
        {
            get
            {
                if (Offre == null)
                    return "Article";

                return $"{Offre.Nom} x{Quantite}";
            }
        }

        // Méthodes
        public void CalculerPrixTotal()
        {
            PrixTotal = PrixUnitaire * Quantite;
            DateModification = DateTime.UtcNow;
        }

        public bool AugmenterQuantite(int nombre = 1)
        {
            if (nombre <= 0 || Quantite + nombre > 100)
                return false;

            Quantite += nombre;
            CalculerPrixTotal();
            return true;
        }

        public bool DiminuerQuantite(int nombre = 1)
        {
            if (nombre <= 0 || Quantite - nombre < 1)
                return false;

            Quantite -= nombre;
            CalculerPrixTotal();
            return true;
        }

        public void DefinirQuantite(int nouvelleQuantite)
        {
            if (nouvelleQuantite < 1)
                nouvelleQuantite = 1;
            if (nouvelleQuantite > 100)
                nouvelleQuantite = 100;

            Quantite = nouvelleQuantite;
            CalculerPrixTotal();
        }

        public void ProlongerExpiration(int heures = 24)
        {
            DateExpiration = DateTime.UtcNow.AddHours(heures);
            DateModification = DateTime.UtcNow;
        }

        public void Selectionner(bool selectionne = true)
        {
            EstSelectionne = selectionne;
            DateModification = DateTime.UtcNow;
        }

        public bool ValiderDisponibilite()
        {
            // Vérifier que l'offre et le sport sont toujours disponibles
            if (Offre == null || !Offre.EstActive)
                return false;

            if (SportOption == null || !SportOption.EstDisponible)
                return false;

            if (SportOption.EstComplet)
                return false;

            return true;
        }
    }
}