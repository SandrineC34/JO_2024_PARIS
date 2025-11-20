using Xunit;
using Moq;
using FluentAssertions;
using JO2024.Core.Services;
using JO2024.Core.Interfaces;
using JO2024.Core.Entities;
using JO2024.Core.DTOs.Offres;

namespace JO2024.Tests.Services
{
    public class OffreServiceTests
    {
        private readonly Mock<IOffreRepository> _mockRepo;
        private readonly OffreService _service;

        public OffreServiceTests()
        {
            _mockRepo = new Mock<IOffreRepository>();
            _service = new OffreService(_mockRepo.Object);
        }

        // ============================================
        // GET ALL OFFRES
        // ============================================

        [Fact]
        public async Task GetAllOffresAsync_ShouldReturnMappedOffres()
        {
            // Arrange
            var offres = new List<Offre>
            {
                new Offre { Id = 1, Type = "VIP", Nom = "Pack VIP", Description = "Desc", Prix = 200, NombrePersonnes = 2, Caracteristiques = "Test" },
                new Offre { Id = 2, Type = "Standard", Nom = "Pack Standard", Description = "Desc2", Prix = 100, NombrePersonnes = 1, Caracteristiques = "Test2" }
            };

            _mockRepo.Setup(r => r.GetActiveOffresAsync())
                     .ReturnsAsync(offres);

            // Act
            var result = await _service.GetAllOffresAsync();

            // Assert
            result.Should().HaveCount(2);
            result.First().Id.Should().Be(1);
            result.Last().Type.Should().Be("Standard");
        }

        // ============================================
        // GET OFFRE BY ID
        // ============================================

        [Fact]
        public async Task GetOffreByIdAsync_WhenFound_ShouldReturnDto()
        {
            // Arrange
            var offre = new Offre
            {
                Id = 5,
                Type = "Premium",
                Nom = "Offre Premium",
                Description = "Super",
                Prix = 300,
                NombrePersonnes = 4,
                Caracteristiques = "Tout inclus"
            };

            _mockRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(offre);

            // Act
            var result = await _service.GetOffreByIdAsync(5);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(5);
            result.Type.Should().Be("Premium");
        }

        [Fact]
        public async Task GetOffreByIdAsync_WhenNotFound_ShouldReturnNull()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Offre?)null);

            // Act
            var result = await _service.GetOffreByIdAsync(1);

            // Assert
            result.Should().BeNull();
        }

        // ============================================
        // GET OFFRE BY TYPE
        // ============================================

        [Fact]
        public async Task GetOffreByTypeAsync_WhenFound_ShouldReturnDto()
        {
            var offre = new Offre
            {
                Id = 3,
                Type = "VIP",
                Nom = "Pack VIP",
                Description = "Desc",
                Prix = 250,
                NombrePersonnes = 2,
                Caracteristiques = "VIP only"
            };

            _mockRepo.Setup(r => r.GetByTypeAsync("VIP")).ReturnsAsync(offre);

            var result = await _service.GetOffreByTypeAsync("VIP");

            result.Should().NotBeNull();
            result!.Type.Should().Be("VIP");
        }

        [Fact]
        public async Task GetOffreByTypeAsync_WhenNotFound_ShouldReturnNull()
        {
            _mockRepo.Setup(r => r.GetByTypeAsync("Inconnue")).ReturnsAsync((Offre?)null);

            var result = await _service.GetOffreByTypeAsync("Inconnue");

            result.Should().BeNull();
        }
    }
}
