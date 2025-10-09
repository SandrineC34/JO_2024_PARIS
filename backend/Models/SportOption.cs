using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    /// <summary>
    /// Modèle pour les options sportives disponibles
    /// </summary>
    [Table("SportOption")]
    public class SportOption
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; } // 'natation', 'athletisme', 'basketball', etc.

        [Required]
        [StringLength(100)]
        public string Nom { get; set; } // 'Natation', 'Athlétisme', 'Basketball', etc.

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [StringLength(200)]
        public string Lieu { get; set; } // 'Bassin Olympique', 'Stade de France', etc.

        [StringLength(500)]
        public string Adresse { get; set; }

        [StringLength(100)]
        public string Icone { get; set; } // Emoji ou code icône: '🏊', '🏃', '🏀', etc.

        [StringLength(500)]
        public string ImageUrl { get; set; }

        public bool EstNouveaute2024 { get; set; } = false; // Pour 'Surf', 'Skateboard', etc.

        public bool EstPopulaire { get; set; } = false;

        public bool EstDisponible { get; set; } = true;

        public int Ordre { get; set; } = 0;

        public int CapaciteMax { get; set; } = 0; // Capacité du lieu

        public int PlacesDisponibles { get; set; } = 0;

        public DateTime? DateDebutEpreuves { get; set; }

        public DateTime? DateFinEpreuves { get; set; }

        [StringLength(50)]
        public string Categorie { get; set; } // 'Aquatique', 'Athlétisme', 'Sports collectifs', etc.

        [Required]
        public DateTime DateCreation { get; set; } = DateTime.UtcNow;

        public DateTime? DateModification { get; set; }

        // Propriétés calculées
        [NotMapped]
        public string NomAvecIcone => 
            !string.IsNullOrWhiteSpace(Icone) 
                ? $"{Icone} {Nom}" 
                : Nom;

        [NotMapped]
        public string LieuComplet => 
            !string.IsNullOrWhiteSpace(Adresse) 
                ? $"{Lieu} - {Adresse}" 
                : Lieu;

        [NotMapped]
        public bool EstComplet => PlacesDisponibles <= 0;

        [NotMapped]
        public int TauxRemplissage
        {
            get
            {
                if (CapaciteMax <= 0)
                    return 0;
                
                var occupe = CapaciteMax - PlacesDisponibles;
                return (int)((double)occupe / CapaciteMax * 100);
            }
        }

        [NotMapped]
        public string StatutDisponibilite
        {
            get
            {
                if (!EstDisponible)
                    return "Indisponible";
                if (EstComplet)
                    return "Complet";
                if (PlacesDisponibles < CapaciteMax * 0.1)
                    return "Dernières places";
                return "Disponible";
            }
        }

        [NotMapped]
        public string Badge
        {
            get
            {
                if (EstNouveaute2024)
                    return "🆕 Nouveau 2024";
                if (EstPopulaire)
                    return "⭐ Populaire";
                return null;
            }
        }

        // Méthodes
        public bool ReserverPlaces(int nombre)
        {
            if (!EstDisponible || nombre <= 0 || PlacesDisponibles < nombre)
                return false;

            PlacesDisponibles -= nombre;
            DateModification = DateTime.UtcNow;
            return true;
        }

        public void LibererPlaces(int nombre)
        {
            if (nombre <= 0)
                return;

            PlacesDisponibles = Math.Min(PlacesDisponibles + nombre, CapaciteMax);
            DateModification = DateTime.UtcNow;
        }

        public bool EstDansLaPeriode(DateTime date)
        {
            if (!DateDebutEpreuves.HasValue || !DateFinEpreuves.HasValue)
                return true;

            return date >= DateDebutEpreuves.Value && date <= DateFinEpreuves.Value;
        }
    }
}