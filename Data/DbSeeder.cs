using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using JeuxOlympiques.Models;

namespace JeuxOlympiques.Data
{
    /// <summary>
    /// Service pour initialiser la base de données avec des données de test
    /// </summary>
    public class DbSeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DbSeeder> _logger;
        private readonly string _jsonFilePath;

        public DbSeeder(ApplicationDbContext context, ILogger<DbSeeder> logger)
        {
            _context = context;
            _logger = logger;
            _jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "seed-data.json");
        }

        /// <summary>
        /// Charge les données depuis le fichier JSON
        /// </summary>
        public async Task SeedDataAsync(bool forceReseed = false)
        {
            try
            {
                // Vérifier si la base contient déjà des données
                if (!forceReseed && await _context.Utilisateurs.AnyAsync())
                {
                    _logger.LogInformation("La base de données contient déjà des données. Seed ignoré.");
                    return;
                }

                _logger.LogInformation("Début du chargement des données de test...");

                // Lire le fichier JSON
                if (!File.Exists(_jsonFilePath))
                {
                    _logger.LogWarning($"Fichier seed-data.json introuvable à : {_jsonFilePath}");
                    return;
                }

                string jsonContent = await File.ReadAllTextAsync(_jsonFilePath);
                var seedData = JsonSerializer.Deserialize<SeedDataModel>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (seedData == null)
                {
                    _logger.LogError("Impossible de désérialiser le fichier JSON");
                    return;
                }

                // Si forceReseed, nettoyer les données existantes
                if (forceReseed)
                {
                    _logger.LogWarning("Force reseed activé - Suppression des données existantes...");
                    await ClearAllDataAsync();
                }

                // Charger les utilisateurs
                await SeedUtilisateursAsync(seedData.Utilisateurs);

                // Charger les offres
                await SeedOffresAsync(seedData.Offres);

                // Charger les commandes
                await SeedCommandesAsync(seedData.Commandes);

                // Charger les billets
                await SeedBilletsAsync(seedData.Billets);

                _logger.LogInformation("✅ Données de test chargées avec succès !");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors du chargement des données de test");
                throw;
            }
        }

        private async Task SeedUtilisateursAsync(List<UtilisateurSeed> utilisateurs)
        {
            if (utilisateurs == null || !utilisateurs.Any()) return;

            _logger.LogInformation($"Chargement de {utilisateurs.Count} utilisateurs...");

            foreach (var user in utilisateurs)
            {
                // Vérifier si l'utilisateur existe déjà
                if (await _context.Utilisateurs.AnyAsync(u => u.Email == user.Email))
                {
                    _logger.LogInformation($"Utilisateur {user.Email} déjà existant, ignoré.");
                    continue;
                }

                var utilisateur = new Utilisateur
                {
                    Prenom = user.Prenom,
                    Nom = user.Nom,
                    Email = user.Email,
                    // Hacher le mot de passe
                    MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(user.MotDePasse),
                    // Générer une clé unique
                    CleUtilisateur = Guid.NewGuid().ToString(),
                    Role = user.Role,
                    DateCreation = DateTime.Now,
                    DateModification = DateTime.Now
                };

                _context.Utilisateurs.Add(utilisateur);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Utilisateurs chargés");
        }

        private async Task SeedOffresAsync(List<OffreSeed> offres)
        {
            if (offres == null || !offres.Any()) return;

            _logger.LogInformation($"Chargement de {offres.Count} offres...");

            foreach (var offreSeed in offres)
            {
                var offre = new Offre
                {
                    Type = offreSeed.Type,
                    Nom = offreSeed.Nom,
                    Description = offreSeed.Description,
                    Prix = offreSeed.Prix,
                    NombrePersonnes = offreSeed.NombrePersonnes,
                    Actif = offreSeed.Actif,
                    DateCreation = DateTime.Now
                };

                _context.Offres.Add(offre);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Offres chargées");
        }

        private async Task SeedCommandesAsync(List<CommandeSeed> commandes)
        {
            if (commandes == null || !commandes.Any()) return;

            _logger.LogInformation($"Chargement de {commandes.Count} commandes...");

            foreach (var cmdSeed in commandes)
            {
                var commande = new Commande
                {
                    Numero = cmdSeed.Numero,
                    UtilisateurId = cmdSeed.UtilisateurId,
                    DateAchat = cmdSeed.DateAchat,
                    MontantTotal = cmdSeed.MontantTotal,
                    Statut = cmdSeed.Statut,
                    CleTransaction = cmdSeed.CleTransaction ?? Guid.NewGuid().ToString(),
                    DateCreation = cmdSeed.DateAchat
                };

                _context.Commandes.Add(commande);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Commandes chargées");
        }

        private async Task SeedBilletsAsync(List<BilletSeed> billets)
        {
            if (billets == null || !billets.Any()) return;

            _logger.LogInformation($"Chargement de {billets.Count} billets...");

            foreach (var billetSeed in billets)
            {
                var billet = new Billet
                {
                    Numero = billetSeed.Numero,
                    CommandeId = billetSeed.CommandeId,
                    OffreId = billetSeed.OffreId,
                    Titre = billetSeed.Titre,
                    DateEpreuve = billetSeed.DateEpreuve,
                    Lieu = billetSeed.Lieu,
                    Place = billetSeed.Place,
                    Statut = billetSeed.Statut,
                    CodeQR = billetSeed.CodeQR,
                    CleFinal = billetSeed.CleFinal,
                    DateEmission = billetSeed.DateEmission,
                    DateScan = billetSeed.DateScan,
                    DateCreation = billetSeed.DateEmission,
                    DateModification = billetSeed.DateEmission
                };

                _context.Billets.Add(billet);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Billets chargés");
        }

        private async Task ClearAllDataAsync()
        {
            // Supprimer dans l'ordre inverse des dépendances
            _context.Billets.RemoveRange(_context.Billets);
            _context.Commandes.RemoveRange(_context.Commandes);
            _context.LignesPanier.RemoveRange(_context.LignesPanier);
            _context.Paniers.RemoveRange(_context.Paniers);
            _context.Offres.RemoveRange(_context.Offres);
            _context.Utilisateurs.RemoveRange(_context.Utilisateurs);

            await _context.SaveChangesAsync();
            _logger.LogInformation("Données existantes supprimées");
        }

        /// <summary>
        /// Exporte les données actuelles de la base vers un fichier JSON
        /// </summary>
        public async Task ExportDataToJsonAsync(string outputPath = null)
        {
            try
            {
                outputPath ??= Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "export-data.json");

                var exportData = new
                {
                    ExportDate = DateTime.Now,
                    Utilisateurs = await _context.Utilisateurs
                        .Select(u => new
                        {
                            u.Id,
                            u.Prenom,
                            u.Nom,
                            u.Email,
                            u.Role,
                            u.DateCreation
                        })
                        .ToListAsync(),
                    
                    Offres = await _context.Offres.ToListAsync(),
                    
                    Commandes = await _context.Commandes
                        .Select(c => new
                        {
                            c.Id,
                            c.Numero,
                            c.UtilisateurId,
                            c.DateAchat,
                            c.MontantTotal,
                            c.Statut
                        })
                        .ToListAsync(),
                    
                    Billets = await _context.Billets
                        .Select(b => new
                        {
                            b.Id,
                            b.Numero,
                            b.CommandeId,
                            b.OffreId,
                            b.Titre,
                            b.DateEpreuve,
                            b.Lieu,
                            b.Place,
                            b.Statut,
                            b.DateEmission,
                            b.DateScan
                        })
                        .ToListAsync()
                };

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                string json = JsonSerializer.Serialize(exportData, options);
                await File.WriteAllTextAsync(outputPath, json);

                _logger.LogInformation($"✅ Données exportées vers : {outputPath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'export des données");
                throw;
            }
        }
    }

    // Modèles pour la désérialisation JSON
    public class SeedDataModel
    {
        public List<UtilisateurSeed> Utilisateurs { get; set; }
        public List<OffreSeed> Offres { get; set; }
        public List<CommandeSeed> Commandes { get; set; }
        public List<BilletSeed> Billets { get; set; }
    }

    public class UtilisateurSeed
    {
        public int Id { get; set; }
        public string Prenom { get; set; }
        public string Nom { get; set; }
        public string Email { get; set; }
        public string MotDePasse { get; set; }
        public string Role { get; set; }
    }

    public class OffreSeed
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Nom { get; set; }
        public string Description { get; set; }
        public decimal Prix { get; set; }
        public int NombrePersonnes { get; set; }
        public bool Actif { get; set; }
        public string ImageUrl { get; set; }
    }

    public class CommandeSeed
    {
        public int Id { get; set; }
        public string Numero { get; set; }
        public int UtilisateurId { get; set; }
        public DateTime DateAchat { get; set; }
        public decimal MontantTotal { get; set; }
        public string Statut { get; set; }
        public string CleTransaction { get; set; }
    }

    public class BilletSeed
    {
        public int Id { get; set; }
        public string Numero { get; set; }
        public int CommandeId { get; set; }
        public int OffreId { get; set; }
        public string Titre { get; set; }
        public DateTime DateEpreuve { get; set; }
        public string Lieu { get; set; }
        public string Place { get; set; }
        public string Statut { get; set; }
        public string CodeQR { get; set; }
        public string CleFinal { get; set; }
        public DateTime DateEmission { get; set; }
        public DateTime? DateScan { get; set; }
    }
}