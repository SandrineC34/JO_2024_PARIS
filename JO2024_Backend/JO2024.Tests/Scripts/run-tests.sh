#!/bin/bash
# ============================================
# run-tests.sh - Exécution de tous les tests
# ============================================
# lancer dans JO2024_Backend
echo "JO2024 - Exécution des tests"
echo "================================"

# Couleurs pour l'affichage
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# 1. Tests Unitaires
echo -e "\n${YELLOW}1️⃣  Tests Unitaires${NC}"
echo "-------------------"
dotnet test JO2024.Tests/JO2024.Tests.csproj \
    --logger "console;verbosity=detailed"

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✅ Tests unitaires réussis${NC}"
else
    echo -e "${RED}❌ Tests unitaires échoués${NC}"
    exit 1
fi

# 2. Tests d'Intégration
echo -e "\n${YELLOW}2️⃣  Tests d'Intégration${NC}"
echo "------------------------"
dotnet test JO2024.Tests/JO2024.Tests.csproj \
  --filter "FullyQualifiedName~AuthIntegrationTests" \
  --logger "console;verbosity=detailed"

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✅ Tests d'intégration réussis${NC}"
else
    echo -e "${RED}❌ Tests d'intégration échoués${NC}"
    exit 1
fi

# 3. Tous les tests avec couverture
echo -e "\n${YELLOW}3️⃣  Génération du rapport de couverture${NC}"
echo "----------------------------------------"
dotnet test JO2024.Tests/JO2024.Tests.csproj \
  --collect:"XPlat Code Coverage" \
  --results-directory ./TestResults \
  --logger "console;verbosity=normal"

echo -e "\n${GREEN}✅ Tous les tests terminés${NC}"
echo "📊 Résultats de couverture disponibles dans ./TestResults/"

 ./JO2024.Tests/Scripts/generate-coverage-report.sh