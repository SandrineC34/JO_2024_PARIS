using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    /// <summary>
    /// Modèle pour les billets électroniques avec QR Code
    /// </summary>
    [Table("Billet")]
    public class Billet
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Numero { get; set; } // Format: BIL-YYYYMMDD-XXXXX

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }

        [Required]
        [ForeignKey("Commande")]
        public int CommandeId { get; set; }

        [Required]
        [StringLength(200)]
        public string Titre { get; set; } // Ex: "Natation - Finale 100m"

        [Required]
        [StringLength(100)]
        public string Sport { get; set; }

        [Required]
        [StringLength(200)]
        public string Lieu { get; set; }

        [Required]
        public DateTime DateEpreuve { get; set; }

        [StringLength(50)]
        public string Place { get; set; } // Ex: "Tribune A - Rang 12 - Siège 45"

        [StringLength(50)]
        public string Secteur { get; set; }

        [StringLength(20)]
        public string Rang { get; set; }

        [StringLength(20)]
        public string Siege { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Prix { get; set; }

        [Required]
        [StringLength(50)]
        public string Statut { get; set; } = "Actif"; // Actif, Scanné, Annulé, Expiré

        [StringLength(500)]
        public string CodeQR { get; set; } // Base64 de l'image QR code ou URL

        [StringLength(100)]
        public string CodeQRData { get; set; } // Données encodées dans le QR

        [StringLength(500)]
        public string UrlPDF { get; set; }

        public DateTime? DateScan { get; set; }

        [StringLength(100)]
        public string LieuScan { get; set; }

        [StringLength(100)]
        public string NomTitulaire { get; set; }

        [StringLength(100)]
        public string PrenomTitulaire { get; set; }

        public bool EnvoyeParEmail { get; set; } = false;

        public DateTime? DateEnvoiEmail { get; set; }

        public bool TelechargeParUtilisateur { get; set; } = false;

        public DateTime? DateTelechargement { get; set; }

        [Required]
        public DateTime DateCreation { get; set; } = DateTime.UtcNow;

        public DateTime? DateModification { get; set; }

        // Navigation
        public virtual User User { get; set; }
        public virtual Commande Commande { get; set; }

        // Propriétés calculées
        [NotMapped]
        public bool EstValide => Statut == "Actif" && DateEpreuve > DateTime.Now;

        [NotMapped]
        public bool EstScanne => Statut == "Scanné" && DateScan.HasValue;

        [NotMapped]
        public bool EstExpire => DateEpreuve < DateTime.Now && Statut != "Scanné";

        [NotMapped]
        public string StatutAffichage
        {
            get
            {
                return Statut switch
                {
                    "Actif" => "✅ Actif",
                    "Scanné" => "✓ Utilisé",
                    "Annulé" => "❌ Annulé",
                    "Expiré" => "⏰ Expiré",
                    _ => Statut
                };
            }
        }

        [NotMapped]
        public string TitulaireComplet => 
            !string.IsNullOrWhiteSpace(PrenomTitulaire) && !string.IsNullOrWhiteSpace(NomTitulaire)
                ? $"{PrenomTitulaire} {NomTitulaire}"
                : null;

        [NotMapped]
        public string PlaceComplete
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Secteur))
                    return Place;

                var parts = new List<string>();
                if (!string.IsNullOrWhiteSpace(Secteur)) parts.Add($"Secteur {Secteur}");
                if (!string.IsNullOrWhiteSpace(Rang)) parts.Add($"Rang {Rang}");
                if (!string.IsNullOrWhiteSpace(Siege)) parts.Add($"Siège {Siege}");

                return parts.Count > 0 ? string.Join(" - ", parts) : Place;
            }
        }

        // Méthodes
        public void GenererNumeroBillet()
        {
            var date = DateTime.Now.ToString("yyyyMMdd");
            var random = new Random().Next(10000, 99999);
            Numero = $"BIL-{date}-{random}";
        }

        public void GenererCodeQR()
        {
            // Données à encoder dans le QR code
            CodeQRData = $"{Numero}|{UserId}|{DateEpreuve:yyyyMMdd}|{Sport}";
            
            // TODO: Générer l'image QR code et la convertir en Base64
            // CodeQR = GenerateQRCodeBase64(CodeQRData);
        }

        public bool Scanner(string lieu)
        {
            if (!EstValide)
                return false;

            Statut = "Scanné";
            DateScan = DateTime.UtcNow;
            LieuScan = lieu;
            return true;
        }

        public void MarquerCommeEnvoye()
        {
            EnvoyeParEmail = true;
            DateEnvoiEmail = DateTime.UtcNow;
        }

        public void MarquerCommeTelecharge()
        {
            TelechargeParUtilisateur = true;
            DateTelechargement = DateTime.UtcNow;
        }

        public bool Annuler()
        {
            if (Statut == "Scanné")
                return false;

            Statut = "Annulé";
            DateModification = DateTime.UtcNow;
            return true;
        }
    }
}