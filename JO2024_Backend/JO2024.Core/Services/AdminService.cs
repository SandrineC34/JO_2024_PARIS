// ============================================
// AdminService.cs - CORRECTION DE LA TYPO
// JO2024.Core/Services/AdminService.cs
// ============================================
using Microsoft.EntityFrameworkCore;
using System.Text;
using JO2024.Core.Entities;
using JO2024.Core.Interfaces;
using JO2024.Core.DTOs.Admin;
using JO2024.Core.DTOs.Common;
using JO2024.Core.DTOs.Commandes;
using JO2024.Core.DTOs.Billets;
using JO2024.Core.DTOs.Offres;

namespace JO2024.Core.Services;

public class AdminService : IAdminService
{
    // ✅ CORRECTION : IUtilisateurRepository (avec majuscule)
    private readonly IUtilisateurRepository _utilisateurRepository;
    private readonly IOffreRepository _offreRepository;
    private readonly ICommandeRepository _commandeRepository;
    private readonly IBilletRepository _billetRepository;

    public AdminService(
        IUtilisateurRepository utilisateurRepository, // ✅ Majuscule
        IOffreRepository offreRepository,
        ICommandeRepository commandeRepository,
        IBilletRepository billetRepository)
    {
        _utilisateurRepository = utilisateurRepository;
        _offreRepository = offreRepository;
        _commandeRepository = commandeRepository;
        _billetRepository = billetRepository;
    }

    // ============================================
    // Gestion des Utilisateurs
    // ============================================

    public async Task<PagedResult<UserDetailsDto>> GetAllUsersAsync(int page, int pageSize)
    {
        var skip = (page - 1) * pageSize;
        
        var users = await _utilisateurRepository.GetAllAsync();
        var totalCount = users.Count();
        
        var pagedUsers = users
            .Skip(skip)
            .Take(pageSize)
            .ToList();

        var userDetails = new List<UserDetailsDto>();
        
        foreach (var user in pagedUsers)
        {
            var userWithDetails = await _utilisateurRepository.GetWithCommandesAsync(user.Id);
            
            userDetails.Add(new UserDetailsDto
            {
                Id = user.Id,
                Prenom = user.Prenom,
                Nom = user.Nom,
                Email = user.Email,
                Role = user.Role,
                EstActif = user.EstActif,
                DateCreation = user.DateCreation,
                DerniereConnexion = user.DerniereConnexion,
                NombreCommandes = userWithDetails?.Commandes?.Count ?? 0,
                NombreBillets = userWithDetails?.Billets?.Count ?? 0,
                TotalDepense = userWithDetails?.Commandes.Sum(c => c.MontantTotal) ?? 0
            });
        }

        return new PagedResult<UserDetailsDto>
        {
            Items = userDetails,
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize
        };
    }

    public async Task<UserDetailsDto?> GetUserDetailsAsync(int id)
    {
        var user = await _utilisateurRepository.GetWithCommandesAsync(id);
        
        if (user == null)
            return null;

        return new UserDetailsDto
        {
            Id = user.Id,
            Prenom = user.Prenom,
            Nom = user.Nom,
            Email = user.Email,
            Role = user.Role,
            EstActif = user.EstActif,
            DateCreation = user.DateCreation,
            DerniereConnexion = user.DerniereConnexion,
            NombreCommandes = user.Commandes.Count,
            NombreBillets = user.Billets.Count,
            TotalDepense = user.Commandes.Sum(c => c.MontantTotal)
        };
    }

    public async Task<bool> ToggleUserStatusAsync(int id)
    {
        var user = await _utilisateurRepository.GetByIdAsync(id);
        
        if (user == null)
            return false;

        user.EstActif = !user.EstActif;
        await _utilisateurRepository.UpdateAsync(user);
        
        return true;
    }

    public async Task<bool> UpdateUserRoleAsync(int id, string role)
    {
        if (!new[] { "User", "Admin", "SuperAdmin" }.Contains(role))
            return false;

        var user = await _utilisateurRepository.GetByIdAsync(id);
        
        if (user == null)
            return false;

        user.Role = role;
        await _utilisateurRepository.UpdateAsync(user);
        
        return true;
    }

    // ============================================
    // Gestion des Offres
    // ============================================

    public async Task<OffreDto> CreateOffreAsync(CreateOffreDto createOffreDto)
    {
        var offre = new Offre
        {
            Type = createOffreDto.Type,
            Nom = createOffreDto.Nom,
            Description = createOffreDto.Description,
            Prix = createOffreDto.Prix,
            NombrePersonnes = createOffreDto.NombrePersonnes,
            EstActif = true,
            DateCreation = DateTime.UtcNow
        };

        offre = await _offreRepository.AddAsync(offre);

        return new OffreDto
        {
            Id = offre.Id,
            Type = offre.Type,
            Nom = offre.Nom,
            Description = offre.Description,
            Prix = offre.Prix,
            NombrePersonnes = offre.NombrePersonnes
        };
    }

    public async Task<bool> UpdateOffreAsync(int id, UpdateOffreDto updateOffreDto)
    {
        var offre = await _offreRepository.GetByIdAsync(id);
        
        if (offre == null)
            return false;

        offre.Nom = updateOffreDto.Nom;
        offre.Description = updateOffreDto.Description;
        offre.Prix = updateOffreDto.Prix;
        offre.EstActif = updateOffreDto.EstActif;

        await _offreRepository.UpdateAsync(offre);
        
        return true;
    }

    public async Task<bool> DeleteOffreAsync(int id)
    {
        var offre = await _offreRepository.GetByIdAsync(id);
        
        if (offre == null)
            return false;

        offre.EstActif = false;
        await _offreRepository.UpdateAsync(offre);
        
        return true;
    }

    // ============================================
    // Statistiques
    // ============================================

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        var users = await _utilisateurRepository.GetAllAsync();
        var commandes = await _commandeRepository.GetAllAsync();
        var billets = await _billetRepository.GetAllAsync();

        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var commandesList = commandes.ToList();
        var billetsList = billets.ToList();

        var ventesParOffre = new Dictionary<string, int>();
        foreach (var commande in commandesList)
        {
            foreach (var item in commande.Items)
            {
                var offreName = item.Offre?.Nom ?? "Inconnu";
                if (ventesParOffre.ContainsKey(offreName))
                    ventesParOffre[offreName] += item.Quantite;
                else
                    ventesParOffre[offreName] = item.Quantite;
            }
        }

        var ventesParSport = billetsList
            .GroupBy(b => b.Sport)
            .ToDictionary(g => g.Key, g => g.Count());

        return new DashboardStatsDto
        {
            TotalUtilisateurs = users.Count(),
            UtilisateursActifs = users.Count(u => u.EstActif),
            TotalCommandes = commandesList.Count,
            TotalBillets = billetsList.Count,
            ChiffreAffaireTotal = commandesList.Sum(c => c.MontantTotal),
            ChiffreAffaireMoisActuel = commandesList
                .Where(c => c.DateAchat >= startOfMonth)
                .Sum(c => c.MontantTotal),
            VentesParOffre = ventesParOffre,
            VentesParSport = ventesParSport
        };
    }

    public async Task<List<SalesStatsDto>> GetSalesStatsAsync(DateTime? startDate, DateTime? endDate)
    {
        var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
        var end = endDate ?? DateTime.UtcNow;

        var commandes = await _commandeRepository.GetAllAsync();
        
        var stats = commandes
            .Where(c => c.DateAchat >= start && c.DateAchat <= end)
            .GroupBy(c => c.DateAchat.Date)
            .Select(g => new SalesStatsDto
            {
                Date = g.Key,
                NombreVentes = g.Count(),
                Montant = g.Sum(c => c.MontantTotal)
            })
            .OrderBy(s => s.Date)
            .ToList();

        return stats;
    }

    // ============================================
    // Gestion des Commandes
    // ============================================

    public async Task<PagedResult<CommandeDto>> GetAllCommandesAsync(int page, int pageSize)
    {
        var skip = (page - 1) * pageSize;
        
        var commandes = await _commandeRepository.GetAllAsync();
        var totalCount = commandes.Count();
        
        var pagedCommandes = commandes
            .OrderByDescending(c => c.DateAchat)
            .Skip(skip)
            .Take(pageSize)
            .ToList();

        var commandeDtos = pagedCommandes.Select(c => new CommandeDto
        {
            Id = c.Id,
            Numero = c.Numero,
            DateAchat = c.DateAchat,
            MontantHT = c.MontantHT,
            MontantTVA = c.MontantTVA,
            MontantTotal = c.MontantTotal,
            Statut = c.Statut,
            Items = c.Items.Select(i => new CommandeItemDetailDto
            {
                Id = i.Id,
                OffreNom = i.Offre?.Nom ?? "N/A",
                Quantite = i.Quantite,
                Prix = i.PrixTotal,
                Sport = i.Sport ?? "N/A"
            }).ToList()
        }).ToList();

        return new PagedResult<CommandeDto>
        {
            Items = commandeDtos,
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize
        };
    }

    public async Task<bool> UpdateCommandeStatusAsync(int id, string status)
    {
        var validStatuses = new[] { "Payée", "Utilisée", "Annulée", "Remboursée" };
        
        if (!validStatuses.Contains(status))
            return false;

        var commande = await _commandeRepository.GetByIdAsync(id);
        
        if (commande == null)
            return false;

        commande.Statut = status;
        await _commandeRepository.UpdateAsync(commande);
        
        return true;
    }

    // ============================================
    // Gestion des Billets
    // ============================================

    public async Task<PagedResult<BilletDto>> GetAllBilletsAsync(int page, int pageSize)
    {
        var skip = (page - 1) * pageSize;
        
        var billets = await _billetRepository.GetAllAsync();
        var totalCount = billets.Count();
        
        var pagedBillets = billets
            .OrderByDescending(b => b.DateCreation)
            .Skip(skip)
            .Take(pageSize)
            .ToList();

        var billetDtos = pagedBillets.Select(b => new BilletDto
        {
            Id = b.Id,
            Numero = b.Numero,
            Titre = b.Titre,
            Sport = b.Sport,
            Lieu = b.Lieu,
            DateEpreuve = b.DateEpreuve,
            Place = b.Place,
            Statut = b.Statut,
            CodeQR = b.CodeQR,
            DateScan = b.DateScan,
            DateCreation = b.DateCreation
        }).ToList();

        return new PagedResult<BilletDto>
        {
            Items = billetDtos,
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize
        };
    }

    public async Task<bool> CancelBilletAsync(int id)
    {
        var billet = await _billetRepository.GetByIdAsync(id);
        
        if (billet == null || billet.Statut == "Scanné")
            return false;

        billet.Statut = "Annulé";
        await _billetRepository.UpdateAsync(billet);
        
        return true;
    }

    // ============================================
    // Exports
    // ============================================

    public async Task<string> ExportUsersToCSVAsync()
    {
        var users = await _utilisateurRepository.GetAllAsync();
        var csv = new StringBuilder();
        
        csv.AppendLine("Id,Prénom,Nom,Email,Rôle,Actif,Date Création,Dernière Connexion");
        
        foreach (var user in users)
        {
            csv.AppendLine($"{user.Id}," +
                          $"\"{user.Prenom}\"," +
                          $"\"{user.Nom}\"," +
                          $"\"{user.Email}\"," +
                          $"{user.Role}," +
                          $"{user.EstActif}," +
                          $"{user.DateCreation:yyyy-MM-dd HH:mm:ss}," +
                          $"{user.DerniereConnexion?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Jamais"}");
        }

        return csv.ToString();
    }

    public async Task<string> ExportCommandesToCSVAsync(DateTime? startDate, DateTime? endDate)
    {
        var commandes = await _commandeRepository.GetAllAsync();
        
        if (startDate.HasValue)
            commandes = commandes.Where(c => c.DateAchat >= startDate.Value);
        
        if (endDate.HasValue)
            commandes = commandes.Where(c => c.DateAchat <= endDate.Value);

        var csv = new StringBuilder();
        
        csv.AppendLine("Numéro,Date,Utilisateur,Montant HT,TVA,Montant Total,Statut");
        
        foreach (var commande in commandes)
        {
            var utilisateur = await _utilisateurRepository.GetByIdAsync(commande.UtilisateurId);
            var userName = utilisateur != null ? $"{utilisateur.Prenom} {utilisateur.Nom}" : "Inconnu";
            
            csv.AppendLine($"{commande.Numero}," +
                          $"{commande.DateAchat:yyyy-MM-dd HH:mm:ss}," +
                          $"\"{userName}\"," +
                          $"{commande.MontantHT:F2}," +
                          $"{commande.MontantTVA:F2}," +
                          $"{commande.MontantTotal:F2}," +
                          $"{commande.Statut}");
        }

        return csv.ToString();
    }
}