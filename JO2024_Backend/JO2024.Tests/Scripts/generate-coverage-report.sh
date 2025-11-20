#!/bin/bash
echo "Génération du rapport de couverture - MODULE AUTHENTIFICATION"
echo "=================================================================="

# 1. Exécuter les tests avec couverture
echo "Exécution des tests AuthService..."
dotnet test JO2024.Tests/JO2024.Tests.csproj \
  --filter "FullyQualifiedName~AuthService" \
  --collect:"XPlat Code Coverage" \
  --results-directory ./TestResults

# 2. Trouver le fichier de couverture
COVERAGE_FILE=$(find ./TestResults -name "coverage.cobertura.xml" -type f -printf '%T@ %p\n' | sort -rn | head -1 | cut -d' ' -f2-)

echo "Fichier de couverture : $COVERAGE_FILE"

# 3. Générer le rapport FILTRÉ
echo "Génération du rapport filtré (AuthService + AuthController + UtilisateurRepository)..."
reportgenerator \
  -reports:"$COVERAGE_FILE" \
  -targetdir:"./CoverageReport_Auth" \
  -reporttypes:"Html;Badges;TextSummary" \
  -classfilters:"+JO2024.Core.Services.AuthService;+JO2024.API.Controllers.AuthController"

echo ""
echo "Rapport généré dans ./CoverageReport_Auth/"
echo "Ouvrir : ./CoverageReport_Auth/index.html"
echo ""

# 4. Afficher le résumé
if [ -f "./CoverageReport_Auth/Summary.txt" ]; then
    echo "RÉSUMÉ DE COUVERTURE - MODULE AUTHENTIFICATION"
    echo "================================================"
    cat ./CoverageReport_Auth/Summary.txt
fi