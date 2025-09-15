using JO2024API.Data.SeedData;

namespace JO2024API.Data
{
    public static class SeedDataManager
    {
        public static void Initialize(AppDbContext context)
        {
            // Créer la base si elle n'existe pas
            context.Database.EnsureCreated();

            // Exécuter les seeds dans l'ordre de dépendance
            try
            {
                // 1. Données de base (indépendantes)
                SportOptionSeedData.Seed(context);
                OffreSeedData.Seed(context);
                
                // 2. Utilisateurs
                UserSeedData.Seed(context);
                
                // 3. Données dépendantes (nécessitent les utilisateurs)
                CommandeSeedData.Seed(context);
                BilletSeedData.Seed(context);

                Console.WriteLine("✅ Initialisation des données terminée avec succès");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur lors de l'initialisation des données : {ex.Message}");
                throw;
            }
        }

        public static void SeedForProduction(AppDbContext context)
        {
            // Version allégée pour la production
            // Ne contient que les données essentielles, pas les données de test
            
            context.Database.EnsureCreated();

            try
            {
                SportOptionSeedData.Seed(context);
                OffreSeedData.Seed(context);
                
                Console.WriteLine("✅ Initialisation des données de production terminée");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur lors de l'initialisation des données de production : {ex.Message}");
                throw;
            }
        }

        public static void SeedForTesting(AppDbContext context)
        {
            // Version pour les tests unitaires/intégration
            // Contient des données spécifiques aux tests
            
            Initialize(context);
            
            // Ajouter des données supplémentaires pour les tests
            // ... données de test spécifiques
            
            Console.WriteLine("✅ Initialisation des données de test terminée");
        }
    }
}