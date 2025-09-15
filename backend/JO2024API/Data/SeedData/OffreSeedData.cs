using JO2024API.Models;

namespace JO2024API.Data.SeedData
{
    public static class OffreSeedData
    {
        public static void Seed(AppDbContext context)
        {
            // Si les données existent déjà, ne rien faire
            if (context.Offres.Any())
                return;

            Console.WriteLine("🔄 Initialisation des offres...");

            // 1. Créer les offres principales
            var offres = new List<Offre>
            {
                new Offre
                {
                    Type = "solo",
                    Nom = "Offre Solo",
                    SousTitre = "Tarif individuel",
                    Prix = 75.00m,
                    NbPersonnes = 1,
                    EconomieVsSolo = null,
                    Description = "Parfait pour une expérience personnelle des JO. Accès à une épreuve de votre choix avec tous les services inclus.",
                    EstFeatured = false
                },
                new Offre
                {
                    Type = "duo",
                    Nom = "Offre Duo ⭐",
                    SousTitre = "Tarif préférentiel - Économie de 20€",
                    Prix = 130.00m,
                    NbPersonnes = 2,
                    EconomieVsSolo = 20.00m,
                    Description = "L'offre idéale pour partager l'émotion olympique à deux ! Bénéficiez d'un tarif avantageux et de services prioritaires.",
                    EstFeatured = true
                },
                new Offre
                {
                    Type = "famille",
                    Nom = "Offre Famille",
                    SousTitre = "Meilleur qualité-prix - Économie de 80€",
                    Prix = 220.00m,
                    NbPersonnes = 4,
                    EconomieVsSolo = 80.00m,
                    Description = "L'offre familiale parfaite ! Vivez les JO en famille avec le meilleur rapport qualité-prix et des services premium.",
                    EstFeatured = false
                }
            };

            context.Offres.AddRange(offres);
            context.SaveChanges();

            // 2. Récupérer les IDs des offres créées
            var soloId = context.Offres.First(o => o.Type == "solo").Id;
            var duoId = context.Offres.First(o => o.Type == "duo").Id;
            var familleId = context.Offres.First(o => o.Type == "famille").Id;

            // 3. Créer les caractéristiques pour chaque offre
            var caracteristiques = new List<OffreCaracteristique>();

            // Caractéristiques Offre Solo
            caracteristiques.AddRange(new[]
            {
                new OffreCaracteristique { OffreId = soloId, Texte = "Accès pour 1 personne", Ordre = 1 },
                new OffreCaracteristique { OffreId = soloId, Texte = "Épreuve au choix", Ordre = 2 },
                new OffreCaracteristique { OffreId = soloId, Texte = "Billet électronique sécurisé", Ordre = 3 },
                new OffreCaracteristique { OffreId = soloId, Texte = "Support client standard", Ordre = 4 },
                new OffreCaracteristique { OffreId = soloId, Texte = "Téléchargement PDF", Ordre = 5 },
                new OffreCaracteristique { OffreId = soloId, Texte = "Envoi par email", Ordre = 6 }
            });

            // Caractéristiques Offre Duo
            caracteristiques.AddRange(new[]
            {
                new OffreCaracteristique { OffreId = duoId, Texte = "Accès pour 2 personnes", Ordre = 1 },
                new OffreCaracteristique { OffreId = duoId, Texte = "Économie de 20€ vs 2 billets Solo", Ordre = 2 },
                new OffreCaracteristique { OffreId = duoId, Texte = "Épreuve au choix", Ordre = 3 },
                new OffreCaracteristique { OffreId = duoId, Texte = "Billets électroniques sécurisés", Ordre = 4 },
                new OffreCaracteristique { OffreId = duoId, Texte = "Support client prioritaire", Ordre = 5 },
                new OffreCaracteristique { OffreId = duoId, Texte = "Téléchargement PDF", Ordre = 6 },
                new OffreCaracteristique { OffreId = duoId, Texte = "Envoi par email", Ordre = 7 },
                new OffreCaracteristique { OffreId = duoId, Texte = "Places côte à côte garanties", Ordre = 8 }
            });

            // Caractéristiques Offre Famille
            caracteristiques.AddRange(new[]
            {
                new OffreCaracteristique { OffreId = familleId, Texte = "Accès pour 4 personnes", Ordre = 1 },
                new OffreCaracteristique { OffreId = familleId, Texte = "Économie de 80€ vs 4 billets Solo", Ordre = 2 },
                new OffreCaracteristique { OffreId = familleId, Texte = "Tarif dégressif exceptionnel", Ordre = 3 },
                new OffreCaracteristique { OffreId = familleId, Texte = "Épreuve au choix", Ordre = 4 },
                new OffreCaracteristique { OffreId = familleId, Texte = "Billets électroniques sécurisés", Ordre = 5 },
                new OffreCaracteristique { OffreId = familleId, Texte = "Support client prioritaire", Ordre = 6 },
                new OffreCaracteristique { OffreId = familleId, Texte = "Téléchargement PDF", Ordre = 7 },
                new OffreCaracteristique { OffreId = familleId, Texte = "Envoi par email", Ordre = 8 },
                new OffreCaracteristique { OffreId = familleId, Texte = "Places groupées garanties", Ordre = 9 },
                new OffreCaracteristique { OffreId = familleId, Texte = "Kit souvenir famille offert", Ordre = 10 }
            });

            context.OffreCaracteristiques.AddRange(caracteristiques);
            context.SaveChanges();

            Console.WriteLine($"✅ {offres.Count} offres et {caracteristiques.Count} caractéristiques créées");
        }
    }

    public static class SportOptionSeedData
    {
        public static void Seed(AppDbContext context)
        {
            if (context.SportOptions.Any())
                return;

            Console.WriteLine("🔄 Initialisation des sports...");

            var sports = new List<SportOption>
            {
                new SportOption { Code = "natation", Nom = "🏊 Natation", Lieu = "Bassin Olympique", Ordre = 1 },
                new SportOption { Code = "athletisme", Nom = "🏃 Athlétisme", Lieu = "Stade de France", Ordre = 2 },
                new SportOption { Code = "basketball", Nom = "🏀 Basketball", Lieu = "Accor Arena", Ordre = 3 },
                new SportOption { Code = "surf", Nom = "🏄 Surf - Nouveau 2024", Lieu = "Teahupo'o, Tahiti", Ordre = 4 },
                new SportOption { Code = "gymnastique", Nom = "🤸 Gymnastique", Lieu = "Bercy Arena", Ordre = 5 },
                new SportOption { Code = "tennis", Nom = "🎾 Tennis", Lieu = "Roland Garros", Ordre = 6 }
            };

            context.SportOptions.AddRange(sports);
            context.SaveChanges();

            Console.WriteLine($"✅ {sports.Count} sports créés");
        }
    }
}