using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace JeuxOlympiques.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

        // Méthode pour le nom complet
        public string FullName => $"{FirstName} {LastName}";
    }

    // Modèles associés pour les commandes et billets
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string OrderNumber { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } // "Payée", "Utilisée", "Annulée"

        public string Description { get; set; }

        // Navigation property
        public virtual User User { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }

    public class Ticket
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string TicketNumber { get; set; }

        [Required]
        public int UserId { get; set; }

        public int? OrderId { get; set; }

        [Required]
        [StringLength(100)]
        public string EventName { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        [StringLength(100)]
        public string Venue { get; set; }

        [Required]
        [StringLength(50)]
        public string Section { get; set; }

        [Required]
        [StringLength(10)]
        public string Row { get; set; }

        [Required]
        [StringLength(20)]
        public string Seat { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } // "Actif", "Scanné", "Expiré"

        public DateTime? ScannedAt { get; set; }

        public string QRCode { get; set; }

        // Navigation properties
        public virtual User User { get; set; }
        public virtual Order Order { get; set; }
    }
}