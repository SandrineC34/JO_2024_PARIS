using Xunit;
using Moq;
using FluentAssertions;
using JO2024.Core.Services;
using JO2024.Core.Entities;
using JO2024.Core.Interfaces;
using JO2024.Core.DTOs.Admin;
using JO2024.Core.DTOs.Common;

namespace JO2024.Tests.Services;

public class AdminService_UserTests
{
    private readonly Mock<IUtilisateurRepository> _userRepo;
    private readonly Mock<IOffreRepository> _offreRepo;
    private readonly Mock<ICommandeRepository> _commandeRepo;
    private readonly Mock<IBilletRepository> _billetRepo;
    private readonly AdminService _service;

    public AdminService_UserTests()
    {
        _userRepo = new Mock<IUtilisateurRepository>();
        _offreRepo = new Mock<IOffreRepository>();
        _commandeRepo = new Mock<ICommandeRepository>();
        _billetRepo = new Mock<IBilletRepository>();

        _service = new AdminService(
            _userRepo.Object,
            _offreRepo.Object,
            _commandeRepo.Object,
            _billetRepo.Object
        );
    }

    // ============================================================
    // GetAllUsersAsync
    // ============================================================

    [Fact]
    public async Task GetAllUsersAsync_ShouldReturnPagedUsers()
    {
        // Arrange
        var users = new List<Utilisateur>
        {
            new Utilisateur { Id = 1, Prenom = "A", Nom = "X" },
            new Utilisateur { Id = 2, Prenom = "B", Nom = "Y" },
            new Utilisateur { Id = 3, Prenom = "C", Nom = "Z" }
        };

        _userRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

        // Chaque user doit renvoyer les détails complets
        _userRepo.Setup(r => r.GetWithCommandesAsync(It.IsAny<int>()))
            .ReturnsAsync(new Utilisateur
            {
                Commandes = new List<Commande>
                {
                    new Commande { MontantTotal = 100 }
                },
                Billets = new List<Billet>
                {
                    new Billet()
                }
            });

        // Act
        var result = await _service.GetAllUsersAsync(page: 1, pageSize: 2);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(3);
        result.Items.First().NombreCommandes.Should().Be(1);
        result.Items.First().NombreBillets.Should().Be(1);
        result.Items.First().TotalDepense.Should().Be(100);
    }

    // ============================================================
    // GetUserDetailsAsync
    // ============================================================

    [Fact]
    public async Task GetUserDetailsAsync_WhenUserFound_ShouldReturnDetails()
    {
        var user = new Utilisateur
        {
            Id = 10,
            Prenom = "John",
            Nom = "Doe",
            Role = "User",
            Commandes = new List<Commande> { new Commande { MontantTotal = 50 } },
            Billets = new List<Billet> { new Billet() }
        };

        _userRepo.Setup(r => r.GetWithCommandesAsync(10)).ReturnsAsync(user);

        var result = await _service.GetUserDetailsAsync(10);

        result.Should().NotBeNull();
        result!.Id.Should().Be(10);
        result.NombreCommandes.Should().Be(1);
        result.NombreBillets.Should().Be(1);
        result.TotalDepense.Should().Be(50);
    }

    [Fact]
    public async Task GetUserDetailsAsync_WhenUserNotFound_ShouldReturnNull()
    {
        _userRepo.Setup(r => r.GetWithCommandesAsync(999))
                 .ReturnsAsync((Utilisateur?)null);

        var result = await _service.GetUserDetailsAsync(999);

        result.Should().BeNull();
    }

    // ============================================================
    // ToggleUserStatusAsync
    // ============================================================

    [Fact]
    public async Task ToggleUserStatusAsync_WhenUserFound_ShouldToggle()
    {
        var user = new Utilisateur { Id = 1, EstActif = true };

        _userRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

        var result = await _service.ToggleUserStatusAsync(1);

        result.Should().BeTrue();
        user.EstActif.Should().BeFalse();
    }

    [Fact]
    public async Task ToggleUserStatusAsync_WhenUserNotFound_ShouldReturnFalse()
    {
        _userRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Utilisateur?)null);

        var result = await _service.ToggleUserStatusAsync(1);

        result.Should().BeFalse();
    }

    // ============================================================
    // UpdateUserRoleAsync
    // ============================================================

    [Theory]
    [InlineData("Admin")]
    [InlineData("SuperAdmin")]
    [InlineData("User")]
    public async Task UpdateUserRoleAsync_WithValidRole_ShouldUpdate(string validRole)
    {
        var user = new Utilisateur { Id = 1, Role = "User" };

        _userRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

        var result = await _service.UpdateUserRoleAsync(1, validRole);

        result.Should().BeTrue();
        user.Role.Should().Be(validRole);
    }

    [Fact]
    public async Task UpdateUserRoleAsync_WithInvalidRole_ShouldReturnFalse()
    {
        var result = await _service.UpdateUserRoleAsync(1, "Hacker");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateUserRoleAsync_WhenUserNotFound_ShouldReturnFalse()
    {
        _userRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Utilisateur?)null);

        var result = await _service.UpdateUserRoleAsync(1, "Admin");

        result.Should().BeFalse();
    }
    
    // ########################################""
    // test Offres

    // ============================================================
    // CreateOffreAsync
    // ============================================================

    [Fact]
    public async Task CreateOffreAsync_ShouldCreateAndReturnDto()
    {
        // Arrange
        var dto = new CreateOffreDto
        {
            Type = "VIP",
            Nom = "Pack VIP",
            Description = "Desc",
            Prix = 100,
            NombrePersonnes = 2
        };

        _offreRepo.Setup(r => r.AddAsync(It.IsAny<Offre>()))
            .ReturnsAsync((Offre o) =>
            {
                o.Id = 10;
                return o;
            });

        // Act
        var result = await _service.CreateOffreAsync(dto);

        // Assert
        result.Id.Should().Be(10);
        result.Type.Should().Be("VIP");
        result.Nom.Should().Be("Pack VIP");
    }

    // ============================================================
    // UpdateOffreAsync
    // ============================================================

    [Fact]
    public async Task UpdateOffreAsync_WhenFound_ShouldUpdate()
    {
        var offre = new Offre { Id = 3, Nom = "Old" };

        _offreRepo.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(offre);

        var dto = new UpdateOffreDto
        {
            Nom = "NewName",
            Description = "Nouveau",
            Prix = 120,
            EstActif = true
        };

        var result = await _service.UpdateOffreAsync(3, dto);

        result.Should().BeTrue();
        offre.Nom.Should().Be("NewName");
        offre.Description.Should().Be("Nouveau");
        offre.Prix.Should().Be(120);
        offre.EstActif.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateOffreAsync_WhenNotFound_ShouldReturnFalse()
    {
        _offreRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync((Offre?)null);

        var result = await _service.UpdateOffreAsync(5, new UpdateOffreDto());

        result.Should().BeFalse();
    }

    // ============================================================
    // DeleteOffreAsync
    // ============================================================

    [Fact]
    public async Task DeleteOffreAsync_WhenFound_ShouldDisable()
    {
        var offre = new Offre { Id = 2, EstActif = true };

        _offreRepo.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(offre);

        var result = await _service.DeleteOffreAsync(2);

        result.Should().BeTrue();
        offre.EstActif.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteOffreAsync_WhenNotFound_ShouldReturnFalse()
    {
        _offreRepo.Setup(r => r.GetByIdAsync(2)).ReturnsAsync((Offre?)null);

        var result = await _service.DeleteOffreAsync(2);

        result.Should().BeFalse();
    }

    // ###################################
    // TEST Commandes

    [Fact]
    public async Task GetAllCommandesAsync_ShouldReturnPagedResult()
    {
        var commandes = new List<Commande>
        {
            new() {
                Id=1, Numero="C1", DateAchat=DateTime.UtcNow, MontantHT=10, MontantTotal=12,
                Items = new List<CommandeItem> {
                    new CommandeItem { Id=99, Quantite=2, PrixTotal=20 }
                }
            },
            new() { Id=2, Numero="C2", DateAchat=DateTime.UtcNow }
        };

        _commandeRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(commandes);

        var result = await _service.GetAllCommandesAsync(1, 1);

        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(2);
        result.Items.First().Numero.Should().Be("C2");
    }

    [Fact]
    public async Task UpdateCommandeStatusAsync_WhenValid_ShouldUpdate()
    {
        var commande = new Commande { Id = 10, Statut = "Payée" };
        _commandeRepo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(commande);

        var result = await _service.UpdateCommandeStatusAsync(10, "Annulée");

        result.Should().BeTrue();
        commande.Statut.Should().Be("Annulée");
    }

    [Fact]
    public async Task UpdateCommandeStatusAsync_WhenInvalidStatus_ShouldReturnFalse()
    {
        var result = await _service.UpdateCommandeStatusAsync(10, "Hackée");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateCommandeStatusAsync_WhenNotFound_ShouldReturnFalse()
    {
        _commandeRepo.Setup(r => r.GetByIdAsync(10))
                     .ReturnsAsync((Commande?)null);

        var result = await _service.UpdateCommandeStatusAsync(10, "Payée");

        result.Should().BeFalse();
    }
    // #######################################"
    // Test Billets
    [Fact]
    public async Task GetAllBilletsAsync_ShouldReturnPagedResult()
    {
        var billets = new List<Billet>
        {
            new() { Id = 1, Numero = "A", DateCreation = DateTime.UtcNow },
            new() { Id = 2, Numero = "B", DateCreation = DateTime.UtcNow }
        };

        _billetRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(billets);

        var result = await _service.GetAllBilletsAsync(1, 1);

        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task CancelBilletAsync_WhenValid_ShouldCancel()
    {
        var billet = new Billet { Id = 3, Statut = "Valide" };

        _billetRepo.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(billet);

        var result = await _service.CancelBilletAsync(3);

        result.Should().BeTrue();
        billet.Statut.Should().Be("Annulé");
    }

    [Fact]
    public async Task CancelBilletAsync_WhenAlreadyScanned_ShouldReturnFalse()
    {
        var billet = new Billet { Id = 3, Statut = "Scanné" };

        _billetRepo.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(billet);

        var result = await _service.CancelBilletAsync(3);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task CancelBilletAsync_WhenNotFound_ShouldReturnFalse()
    {
        _billetRepo.Setup(r => r.GetByIdAsync(3))
            .ReturnsAsync((Billet?)null);

        var result = await _service.CancelBilletAsync(3);

        result.Should().BeFalse();
    }
    // ##################################
    // test Statistique

    [Fact]
    public async Task GetDashboardStatsAsync_ShouldReturnComputedStats()
    {
        _userRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Utilisateur>
            {
                new() { Id=1, EstActif=true },
                new() { Id=2, EstActif=false }
            });

        _commandeRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Commande>
            {
                new() {
                    DateAchat = DateTime.UtcNow,
                    MontantTotal = 100,
                    Items = new List<CommandeItem>{
                        new CommandeItem { Quantite=2, Offre = new Offre { Nom="VIP" } }
                    }
                }
            });

        _billetRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Billet>
            {
                new() { Sport="Natation" },
                new() { Sport="Natation" },
                new() { Sport="Athlétisme" }
            });

        var stats = await _service.GetDashboardStatsAsync();

        stats.TotalUtilisateurs.Should().Be(2);
        stats.UtilisateursActifs.Should().Be(1);
        stats.TotalBillets.Should().Be(3);
        stats.VentesParSport["Natation"].Should().Be(2);
        stats.VentesParOffre["VIP"].Should().Be(2);
    }

    [Fact]
    public async Task GetSalesStatsAsync_ShouldReturnGroupedData()
    {
        var date = DateTime.UtcNow.Date;

        _commandeRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Commande>
            {
                new() { DateAchat = date.AddHours(1), MontantTotal=50 },
                new() { DateAchat = date.AddHours(5), MontantTotal=100 }
            });

        var stats = await _service.GetSalesStatsAsync(null, null);

        stats.Should().HaveCount(1);
        stats.First().Montant.Should().Be(150);
        stats.First().NombreVentes.Should().Be(2);
    }

    // #####################################
    // Test Export csv

    [Fact]
    public async Task ExportUsersToCSVAsync_ShouldGenerateCSV()
    {
        _userRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Utilisateur>
        {
            new() { Id = 1, Prenom="John", Nom="Doe", Email="a@a.com", Role="User", EstActif=true, DateCreation=DateTime.Parse("2024-01-01") }
        });

        var csv = await _service.ExportUsersToCSVAsync();

        csv.Should().Contain("Id,Prénom,Nom,Email,Rôle,Actif");
        csv.Should().Contain("John");
        csv.Should().Contain("Doe");
    }

    [Fact]
    public async Task ExportCommandesToCSVAsync_ShouldGenerateCSV()
    {
        _commandeRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Commande>
            {
                new() {
                    Numero="CMD1",
                    UtilisateurId=99,
                    DateAchat=DateTime.Parse("2024-02-02"),
                    MontantHT=10, MontantTVA=2, MontantTotal=12,
                    Statut="Payée"
                }
            });

        _userRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync(
            new Utilisateur { Prenom="Alice", Nom="Dupont" }
        );

        var csv = await _service.ExportCommandesToCSVAsync(null, null);

        csv.Should().Contain("CMD1");
        csv.Should().Contain("Alice Dupont");
        
    }


}
