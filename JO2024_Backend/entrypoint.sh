#!/bin/bash
set -e

echo "🚀 Démarrage de l'API JO2024..."

# Attendre que MySQL soit prêt
echo "⏳ AtAtente de la disponibilité de MySQL..."
MAX_RETRIES=30
RETRY_COUNT=0

until dotnet JO2024.API.dll --health-check 2>/dev/null || curl -f http://mysql:3306 2>/dev/null; do
  RETRY_COUNT=$((RETRY_COUNT+1))
  if [ $RETRY_COUNT -ge $MAX_RETRIES ]; then
    echo "❌ MySQL n'est pas disponible après $MAX_RETRIES tentatives"
    exit 1
  fi
  echo "MySQL n'est pas encore prêt - attente 2 secondes... ($RETRY_COUNT/$MAX_RETRIES)"
  sleep 2
done

echo "✅ MySQL est prêt!"

# Créer les migrations automatiquement si elles n'existent pas
echo "📊 Vérification et création des migrations..."
if command -v dotnet-ef &> /dev/null; then
    echo "✓ dotnet-ef est disponible"
    
    # Vérifier si des migrations existent
    MIGRATION_COUNT=$(find ./Infrastructure/Data/Migrations -name "*.cs" 2>/dev/null | wc -l || echo "0")
    
    if [ "$MIGRATION_COUNT" -eq "0" ]; then
        echo "⚠️  Aucune migration trouvée, création de la migration initiale..."
        dotnet ef migrations add InitialCreate --project Infrastructure --startup-project . || echo "⚠️  Impossible de créer les migrations"
    else
        echo "✓ Migrations existantes trouvées ($MIGRATION_COUNT fichiers)"
    fi
    
    # Appliquer les migrations
    echo "📊 Application des migrations de base de données..."
    dotnet ef database update --no-build || echo "⚠️  Migrations déjà appliquées ou erreur (l'app continuera)"
else
    echo "ℹ️  dotnet-ef non disponible, Entity Framework Core créera/mettra à jour la base automatiquement"
fi

# Démarrer l'application
echo "✅ Démarrage de l'application..."
exec dotnet JO2024.API.dll
