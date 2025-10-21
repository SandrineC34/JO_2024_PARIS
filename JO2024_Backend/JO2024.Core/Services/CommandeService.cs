// ============================================
// CommandeService.cs
// ============================================
using JO2024.Core.Entities;
using JO2024.Core.Interfaces;
using JO2024.Core.DTOs.Commandes;
using JO2024.Core.DTOs.Billets; 

namespace JO2024.Core.Services;

public class CommandeService : ICommandeService
{
    private readonly ICommandeRepository _commandeRepository;
    private readonly IOffreRepository _offreRepository;
    private readonly IBilletRepository _billetRepository;
    private readonly IQRCodeService _qrCodeService;
    private const decimal TauxTVA = 0.20m;

    public CommandeService(
        ICommandeRepository commandeRepository,
        IOffreRepository offreRepository,
        IBilletRepository billetRepository,
        IQRCodeService qrCodeService)
    {
        _commandeRepository = commandeRepository;
        _offreRepository = offreRepository;
        _billetRepository = billetRepository;
        _qrCodeService = qrCodeService;
    }

    public async Task<CommandeResponseDto> CreateCommandeAsync(int utilisateurId, CreateCommandeDto createCommandeDto)
    {
        if (createCommandeDto.Items == null || !createCommandeDto.Items.Any())
        {
            return new CommandeResponseDto
            {
                Success = false,
                Message = "La commande doit contenir au moins un article"
            };
        }

        // Créer la commande
        var commande = new Commande
        {
            Numero = await _commandeRepository.GenerateNumeroCommandeAsync(),
            UtilisateurId = utilisateurId,
            DateAchat = DateTime.UtcNow,
            Statut = "Payée",
            MethodePaiement = "Carte bancaire"
        };

        decimal montantTotal = 0;
        var billetIds = new List<int>();

        // Traiter chaque item
        foreach (var itemDto in createCommandeDto.Items)
        {
            var offre = await _offreRepository.GetByIdAsync(itemDto.OffreId);
            
            if (offre == null || !offre.EstActif)
            {
                return new CommandeResponseDto
                {
                    Success = false,
                    Message = $"L'offre {itemDto.OffreId} n'est pas disponible"
                };
            }

            var prixTotal = offre.Prix * itemDto.Quantite;
            montantTotal += prixTotal;

            // Créer l'item de commande
            var commandeItem = new CommandeItem
            {
                OffreId = offre.Id,
                Quantite = itemDto.Quantite,
                PrixUnitaire = offre.Prix,
                PrixTotal = prixTotal,
                Sport = itemDto.Sport
            };

            commande.Items.Add(commandeItem);

            // Créer les billets pour cet item
            var nombreBillets = offre.NombrePersonnes * itemDto.Quantite;
            
            for (int i = 0; i < nombreBillets; i++)
            {
                var billet = await CreateBilletAsync(utilisateurId, itemDto.Sport, offre.Nom);
                commande.Billets.Add(billet);
            }
        }

        // Calculer les montants
        commande.MontantHT = montantTotal / (1 + TauxTVA);
        commande.MontantTVA = montantTotal - commande.MontantHT;
        commande.MontantTotal = montantTotal;

        // Sauvegarder la commande
        commande = await _commandeRepository.AddAsync(commande);

        // Récupérer les IDs des billets créés
        billetIds = commande.Billets.Select(b => b.Id).ToList();

        return new CommandeResponseDto
        {
            Success = true,
            Message = "Commande créée avec succès",
            Commande = MapToCommandeDto(commande),
            BilletIds = billetIds
        };
    }

    public async Task<IEnumerable<CommandeDto>> GetCommandesByUtilisateurAsync(int utilisateurId)
    {
        var commandes = await _commandeRepository.GetByUtilisateurIdAsync(utilisateurId);
        return commandes.Select(MapToCommandeDto);
    }

    public async Task<CommandeDto?> GetCommandeByIdAsync(int commandeId, int utilisateurId)
    {
        var commande = await _commandeRepository.GetWithDetailsAsync(commandeId);
        
        if (commande == null || commande.UtilisateurId != utilisateurId)
            return null;
        
        return MapToCommandeDto(commande);
    }

    private async Task<Billet> CreateBilletAsync(int utilisateurId, string sport, string offreName)
    {
        var numero = await _billetRepository.GenerateNumeroBilletAsync(sport);
        var lieu = GetLieuForSport(sport);
        var dateEpreuve = GetDateEpreuveForSport(sport);
        
        // Données pour le QR code
        var qrData = $"{numero}|{utilisateurId}|{dateEpreuve:yyyy-MM-dd}|{sport}";
        var qrCode = _qrCodeService.GenerateQRCode(qrData);

        return new Billet
        {
            Numero = numero,
            UtilisateurId = utilisateurId,
            Titre = $"{sport} - {offreName}",
            Sport = sport,
            Lieu = lieu,
            DateEpreuve = dateEpreuve,
            Statut = "Actif",
            CodeQR = qrCode,
            DateCreation = DateTime.UtcNow
        };
    }

    private CommandeDto MapToCommandeDto(Commande commande)
    {
        return new CommandeDto
        {
            Id = commande.Id,
            Numero = commande.Numero,
            DateAchat = commande.DateAchat,
            MontantHT = commande.MontantHT,
            MontantTVA = commande.MontantTVA,
            MontantTotal = commande.MontantTotal,
            Statut = commande.Statut,
            Items = commande.Items.Select(i => new CommandeItemDetailDto
            {
                Id = i.Id,
                OffreNom = i.Offre?.Nom ?? "N/A",
                Quantite = i.Quantite,
                Prix = i.PrixTotal,
                Sport = i.Sport ?? "N/A"
            }).ToList()
        };
    }

    private string GetLieuForSport(string sport)
    {
        var lieux = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "natation", "Centre Aquatique Olympique" },
            { "athletisme", "Stade de France" },
            { "basketball", "Accor Arena" },
            { "surf", "Teahupo'o, Tahiti" },
            { "gymnastique", "Bercy Arena" },
            { "tennis", "Roland Garros" }
        };

        return lieux.TryGetValue(sport, out var lieu) ? lieu : "Lieu à déterminer";
    }

    private DateTime GetDateEpreuveForSport(string sport)
    {
        // Dates fictives pour les épreuves (à adapter selon vos besoins)
        var random = new Random();
        var startDate = new DateTime(2024, 7, 26);
        var daysToAdd = random.Next(0, 17); // 17 jours de JO
        
        return startDate.AddDays(daysToAdd).AddHours(random.Next(9, 21));
    }
}