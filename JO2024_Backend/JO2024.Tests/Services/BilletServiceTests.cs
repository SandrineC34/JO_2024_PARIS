using Xunit;
using Moq;
using FluentAssertions;
using JO2024.Core.Services;
using JO2024.Core.Interfaces;
using JO2024.Core.Entities;
using JO2024.Core.DTOs.Billets;

namespace JO2024.Tests.Services
{
    public class BilletServiceTests
    {
        private readonly Mock<IBilletRepository> _mockRepo;
        private readonly BilletService _service;

        public BilletServiceTests()
        {
            _mockRepo = new Mock<IBilletRepository>();
            _service = new BilletService(_mockRepo.Object);
        }

        // ============================================
        // GET BY UTILISATEUR
        // ============================================

        [Fact]
        public async Task GetBilletsByUtilisateurAsync_ShouldReturnMappedBillets()
        {
            // Arrange
            var billets = new List<Billet>
            {
                new Billet { Id = 1, Numero = "A1", Titre = "Natation", Sport = "Natation" },
                new Billet { Id = 2, Numero = "B2", Titre = "Athlétisme", Sport = "Athlétisme" }
            };

            _mockRepo.Setup(r => r.GetByUtilisateurIdAsync(10)).ReturnsAsync(billets);

            // Act
            var result = await _service.GetBilletsByUtilisateurAsync(10);

            // Assert
            result.Should().HaveCount(2);
            result.First().Numero.Should().Be("A1");
        }

        // ============================================
        // GET BY ID
        // ============================================

        [Fact]
        public async Task GetBilletByIdAsync_WhenBilletExistsAndUserMatches_ShouldReturnDto()
        {
            var billet = new Billet
            {
                Id = 5,
                UtilisateurId = 20,
                Numero = "JO2024-TEST"
            };

            _mockRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(billet);

            var result = await _service.GetBilletByIdAsync(5, 20);

            result.Should().NotBeNull();
            result!.Id.Should().Be(5);
            result.Numero.Should().Be("JO2024-TEST");
        }

        [Fact]
        public async Task GetBilletByIdAsync_WhenBilletNotFound_ShouldReturnNull()
        {
            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Billet?)null);

            var result = await _service.GetBilletByIdAsync(1, 10);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetBilletByIdAsync_WhenUserDoesNotMatch_ShouldReturnNull()
        {
            var billet = new Billet { Id = 1, UtilisateurId = 99 };

            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(billet);

            var result = await _service.GetBilletByIdAsync(1, 10);

            result.Should().BeNull();
        }

        // ============================================
        // GENERATE PDF
        // ============================================

        [Fact]
        public async Task GeneratePdfAsync_WhenValidUser_ShouldReturnPdfUrl()
        {
            var billet = new Billet { Id = 3, UtilisateurId = 7 };

            _mockRepo.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(billet);

            var result = await _service.GeneratePdfAsync(3, 7);

            result.Should().Be("/api/billets/3/pdf");
        }

        [Fact]
        public async Task GeneratePdfAsync_WhenInvalidUser_ShouldThrowUnauthorized()
        {
            var billet = new Billet { Id = 3, UtilisateurId = 7 };

            _mockRepo.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(billet);

            Func<Task> act = async () => await _service.GeneratePdfAsync(3, 99);

            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task GeneratePdfAsync_WhenBilletNotFound_ShouldThrowUnauthorized()
        {
            _mockRepo.Setup(r => r.GetByIdAsync(3)).ReturnsAsync((Billet?)null);

            Func<Task> act = async () => await _service.GeneratePdfAsync(3, 5);

            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        // ============================================
        // SEND BY EMAIL
        // ============================================

        [Fact]
        public async Task SendBilletByEmailAsync_WhenValidUser_ShouldReturnTrue()
        {
            var billet = new Billet { Id = 10, UtilisateurId = 50 };

            _mockRepo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(billet);

            var result = await _service.SendBilletByEmailAsync(10, 50);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SendBilletByEmailAsync_WhenUnauthorized_ShouldReturnFalse()
        {
            var billet = new Billet { Id = 10, UtilisateurId = 50 };

            _mockRepo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(billet);

            var result = await _service.SendBilletByEmailAsync(10, 99);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task SendBilletByEmailAsync_WhenBilletNotFound_ShouldReturnFalse()
        {
            _mockRepo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync((Billet?)null);

            var result = await _service.SendBilletByEmailAsync(10, 50);

            result.Should().BeFalse();
        }

        // ============================================
        // SCAN BILLET
        // ============================================

        [Fact]
        public async Task ScanBilletAsync_WhenBilletExists_ShouldReturnRepositoryResult()
        {
            var billet = new Billet { Id = 4, Numero = "XYZ" };

            _mockRepo.Setup(r => r.GetByNumeroAsync("XYZ")).ReturnsAsync(billet);
            _mockRepo.Setup(r => r.ScanBilletAsync(billet.Id)).ReturnsAsync(true);

            var result = await _service.ScanBilletAsync("XYZ");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task ScanBilletAsync_WhenBilletNotFound_ShouldReturnFalse()
        {
            _mockRepo.Setup(r => r.GetByNumeroAsync("NONE"))
                     .ReturnsAsync((Billet?)null);

            var result = await _service.ScanBilletAsync("NONE");

            result.Should().BeFalse();
        }
    }
}
