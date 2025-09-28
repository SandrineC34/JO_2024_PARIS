// Models/User.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JO2024API.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Nom { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Prenom { get; set; }
        
        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; }
        
        [Required]
        public string MotDePasse { get; set; }
        
        public string CleUtilisateur { get; set; }
        
        public DateTime DateCreation { get; set; } = DateTime.UtcNow;
        
        [MaxLength(500)]
        public string Adresse { get; set; }
        
        [MaxLength(20)]
        public string Telephone { get; set; }
        
        // Relations
        public virtual ICollection<Commande> Commandes { get; set; } = new List<Commande>();
        public virtual ICollection<SessionUtilisateur> Sessions { get; set; } = new List<SessionUtilisateur>();
        public virtual Panier Panier { get; set; }
    }
}

// Models/Commande.cs
public class Commande
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string NumeroCommande { get; set; }
    
    [Required]
    public int UserId { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal MontantTotal { get; set; }
    
    public CommandeStatut Statut { get; set; }
    
    public DateTime DateCommande { get; set; } = DateTime.UtcNow;
    
    [MaxLength(500)]
    public string AdresseLivraison { get; set; }
    
    [MaxLength(1000)]
    public string Description { get; set; }
    
    // Relations
    [ForeignKey("UserId")]
    public virtual User User { get; set; }
    
    public virtual ICollection<Billet> Billets { get; set; } = new List<Billet>();
    public virtual Paiement Paiement { get; set; }
}

// Models/Billet.cs
public class Billet
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string NumeroBillet { get; set; }
    
    [Required]
    public int UserId { get; set; }
    
    [Required]
    public int CommandeId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Titre { get; set; }
    
    [MaxLength(100)]
    public string Sport { get; set; }
    
    public DateTime DateEvenement { get; set; }
    
    [MaxLength(200)]
    public string Lieu { get; set; }
    
    [MaxLength(50)]
    public string Secteur { get; set; }
    
    [MaxLength(50)]
    public string Rangee { get; set; }
    
    [MaxLength(50)]
    public string Sieges { get; set; }
    
    public BilletStatut Statut { get; set; } = BilletStatut.Actif;
    
    public DateTime DateGeneration { get; set; } = DateTime.UtcNow;
    
    public DateTime? DateScan { get; set; }
    
    public string QRCode { get; set; }
    
    // Relations
    [ForeignKey("UserId")]
    public virtual User User { get; set; }
    
    [ForeignKey("CommandeId")]
    public virtual Commande Commande { get; set; }
}

// Models/Enums.cs
public enum BilletStatut
{
    Actif,
    Scanne,
    Expire,
    Annule
}

public enum CommandeStatut
{
    EnAttente,
    Payee,
    Annulee,
    Remboursee
}

// Models/DTOs/BilletDetailsDto.cs
public class BilletDetailsDto
{
    public int Id { get; set; }
    public string NumeroBillet { get; set; }
    public string Titre { get; set; }
    public string Sport { get; set; }
    public DateTime DateEvenement { get; set; }
    public string Lieu { get; set; }
    public string Place { get; set; }
    public string Statut { get; set; }
    public string StatutDescription { get; set; }
    public DateTime? DateScan { get; set; }
    public bool PeutVoirQR { get; set; }
    public bool PeutTelecharger { get; set; }
}

// Models/DTOs/CommandeHistoriqueDto.cs
public class CommandeHistoriqueDto
{
    public string NumeroCommande { get; set; }
    public DateTime DateCommande { get; set; }
    public string Description { get; set; }
    public decimal MontantTotal { get; set; }
    public string Statut { get; set; }
}

// Models/DTOs/UpdateUserInfoDto.cs
public class UpdateUserInfoDto
{
    [Required]
    [MaxLength(100)]
    public string Nom { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Prenom { get; set; }
    
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}

// Models/DTOs/ChangePasswordDto.cs
public class ChangePasswordDto
{
    [Required]
    public string MotDePasseActuel { get; set; }
    
    [Required]
    [MinLength(8)]
    public string NouveauMotDePasse { get; set; }
    
    [Required]
    [Compare("NouveauMotDePasse")]
    public string ConfirmerMotDePasse { get; set; }
}

// Models/Panier.cs
public class Panier
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int UserId { get; set; }
    
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;
    
    public DateTime DateModification { get; set; } = DateTime.UtcNow;
    
    [MaxLength(50)]
    public string Statut { get; set; } = "Actif";
    
    // Relations
    [ForeignKey("UserId")]
    public virtual User User { get; set; }
}

// Models/Paiement.cs
public class Paiement
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int CommandeId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string MethodePaiement { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Montant { get; set; }
    
    [MaxLength(50)]
    public string StatutPaiement { get; set; }
    
    public DateTime DatePaiement { get; set; } = DateTime.UtcNow;
    
    [MaxLength(200)]
    public string ReferenceTransaction { get; set; }
    
    // Relations
    [ForeignKey("CommandeId")]
    public virtual Commande Commande { get; set; }
}

// Models/SessionUtilisateur.cs
public class SessionUtilisateur
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int UserId { get; set; }
    
    [Required]
    public string TokenSession { get; set; }
    
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;
    
    public DateTime DateExpiration { get; set; }
    
    public bool Active { get; set; } = true;
    
    [MaxLength(45)]
    public string AdresseIp { get; set; }
    
    public DateTime? LastLogin { get; set; }
    
    // Relations
    [ForeignKey("UserId")]
    public virtual User User { get; set; }
}