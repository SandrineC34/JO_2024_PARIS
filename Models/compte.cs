using System.ComponentModel.DataAnnotations;

namespace JO2024API.Models
{
    // Modèle pour les billets (adapté à votre HTML)
    public class Billet
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string NumeroBillet { get; set; } // "JO2024-NAT-001"

        public int UserId { get; set; }

        public int CommandeId { get; set; }

        [Required]
        [StringLength(100)]
        public string Titre { get; set; } // "Natation - Finale 100m Nage Libre"

        [Required]
        [StringLength(50)]
        public string Sport { get; set; } // "Natation", "Basketball", etc.

        [Required]
        public DateTime DateEvenement { get; set; }

        [Required]
        [StringLength(200)]
        public string Lieu { get; set; } // "Centre Aquatique - Paris La Défense"

        [StringLength(50)]
        public string Secteur { get; set; } // "Secteur A"

        [StringLength(10)]
        public string Rangee { get; set; } // "Rangée 15"

        [StringLength(20)]
        public string Sieges { get; set; } // "Siège 12" ou "Sièges 1-4"

        public BilletStatut Statut { get; set; } = BilletStatut.Actif;

        public DateTime? DateScan { get; set; }

        [StringLength(500)]
        public string QRCode { get; set; } // Données du QR code

        public DateTime DateCreation { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual User User { get; set; }
        public virtual Commande Commande { get; set; }
    }

    public enum BilletStatut
    {
        Actif = 0,
        Scanne = 1,
        Expire = 2,
        Annule = 3
    }

    // Modèle pour les commandes (adapté à votre table HTML)
    public class Commande
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string NumeroCommande { get; set; } // "CMD-2024-001"

        public int UserId { get; set; }

        public DateTime DateCommande { get; set; } = DateTime.Now;

        [Required]
        [StringLength(50)]
        public string TypeOffre { get; set; } // "solo", "duo", "famille"

        [Required]
        [StringLength(50)]
        public string Sport { get; set; }

        public int Quantite { get; set; }

        public decimal MontantTotal { get; set; }

        public CommandeStatut Statut { get; set; } = CommandeStatut.Payee;

        [StringLength(500)]
        public string Description { get; set; } // Pour affichage dans le tableau

        // Navigation properties
        public virtual User User { get; set; }
        public virtual ICollection<Billet> Billets { get; set; } = new List<Billet>();
    }

    public enum CommandeStatut
    {
        EnAttente = 0,
        Payee = 1,
        Utilisee = 2,
        Annulee = 3
    }

    // DTOs pour les API
    public class UpdateUserInfoDto
    {
        [Required]
        [StringLength(50)]
        public string Prenom { get; set; }

        [Required]
        [StringLength(50)]
        public string Nom { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }
    }

    public class ChangePasswordDto
    {
        [Required]
        public string MotDePasseActuel { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 8)]
        public string NouveauMotDePasse { get; set; }

        [Required]
        [Compare("NouveauMotDePasse")]
        public string ConfirmerMotDePasse { get; set; }
    }

    public class BilletDetailsDto
    {
        public int Id { get; set; }
        public string NumeroBillet { get; set; }
        public string Titre { get; set; }
        public string Sport { get; set; }
        public DateTime DateEvenement { get; set; }
        public string Lieu { get; set; }
        public string Place { get; set; } // Secteur + Rangée + Sièges formatés
        public string Statut { get; set; }
        public string StatutDescription { get; set; }
        public DateTime? DateScan { get; set; }
        public bool PeutVoirQR { get; set; }
        public bool PeutTelecharger { get; set; }
    }

    public class CommandeHistoriqueDto
    {
        public string NumeroCommande { get; set; }
        public DateTime DateCommande { get; set; }
        public string Description { get; set; }
        public decimal MontantTotal { get; set; }
        public string Statut { get; set; }
    }
}