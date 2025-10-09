using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using backend.Data;
using backend.Models;
using Newtonsoft.Json;

public static class DbSeeder
{
    public static void Seed(IServiceProvider serviceProvider)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Offres
            if (!context.Offres.Any())
            {
                var offresJson = File.ReadAllText("seed-data.json");
                var seedData = JsonConvert.DeserializeObject<SeedDataModel>(offresJson);
                context.Offres.AddRange(seedData.Offres);
            }

            // Utilisateurs
            if (!context.Utilisateurs.Any())
            {
                var utilisateursJson = File.ReadAllText("seed-data.json");
                var seedData = JsonConvert.DeserializeObject<SeedDataModel>(utilisateursJson);
                context.Utilisateurs.AddRange(seedData.Utilisateurs);
            }

            context.SaveChanges();
        }
    }
}

// Modèle racine du JSON
public class SeedDataModel
{
    public List<Offre> Offres { get; set; }
    public List<Utilisateur> Utilisateurs { get; set; }
}

