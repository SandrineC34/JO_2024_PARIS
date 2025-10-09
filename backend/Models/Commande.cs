using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace backend.Models
{
    /// <summary>
    /// Modèle pour les commandes d'achats de billets
    /// </summary>
    [Table("Commande")]
    public class Commande
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Numero { get; set; } // Format: CMD-YYYYMMDD-XXXXX

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }

        [Required]
        [ForeignKey("Offre")]
        public int OffreId { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "La quantité doit être entre 1 et 100")]
        public int Quantite { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MontantHT { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MontantTVA { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MontantTotal { get; set; }

        [Required]
        [StringLength(50)]
        public string Statut { get; set; } = "En attente"; // En attente, Payée, Annulée, Remboursée

        [Required]
        public DateTime DateAchat { get; set; } = DateTime.UtcNow;

        public DateTime? DatePaiement { get; set; }

        public DateTime? DateAnnulation { get; set; }

        [StringLength(50)]
        public string ModePaiement { get; set; } // CB, Virement, PayPal, etc.

        [StringLength(100)]
        public string TransactionId { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }

        // Informations du sport sélectionné
        [Required]
        [StringLength(100)]
        public string SportSelectionne { get; set; }

        [StringLength(200)]
        public string LieuEpreuve { get; set; }

        public DateTime? DateEpreuve { get; set; }

        // Navigation
        public virtual User User { get; set; }
        public virtual Offre Offre { get; set; }
        public virtual ICollection<Billet> Billets { get; set; }

        // Propriétés calculées
        [NotMapped]
        public string StatutAffichage
        {
            get
            {
                return Statut switch
                {
                    "Payée" => "✅ Payée",
                    "En attente" => "⏳ En attente",
                    "Annulée" => "❌ Annulée",
                    "Remboursée" => "💰 Remboursée",
                    _ => Statut
                };
            }
        }

        [NotMapped]
        public int NombreBillets => Billets?.Count ?? 0;

        [NotMapped]
        public bool EstAnnulable => Statut == "Payée" && DateEpreuve.HasValue && DateEpreuve.Value > DateTime.Now.AddDays(7);

        [NotMapped]
        public string DescriptionCourte => $"{Offre?.Nom} - {SportSelectionne} - {Quantite}x";

        // Méthodes
        public void GenererNumeroCommande()
        {
            var date = DateTime.Now.ToString("yyyyMMdd");
            var random = new Random().Next(10000, 99999);
            Numero = $"CMD-{date}-{random}";
        }

        public void CalculerMontants(decimal tauxTVA = 20.00M)
        {
            MontantHT = MontantTotal / (1 + (tauxTVA / 100));
            MontantTVA = MontantTotal - MontantHT;
        }

        public void ConfirmerPaiement(string modePaiement, string transactionId)
        {
            Statut = "Payée";
            DatePaiement = DateTime.UtcNow;
            ModePaiement = modePaiement;
            TransactionId = transactionId;
        }

        public bool Annuler(string motif)
        {
            if (!EstAnnulable)
                return false;

            Statut = "Annulée";
            DateAnnulation = DateTime.UtcNow;
            Notes = $"Annulation : {motif}";
            return true;
        }
    }
}