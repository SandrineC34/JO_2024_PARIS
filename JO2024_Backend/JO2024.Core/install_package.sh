#!/bin/bash

echo "Installation des packages NuGet pour JO2024.Core"
echo "=================================================="

cd JO2024.Core

echo ""
echo "1. Entity Framework Core..."
dotnet add package Microsoft.EntityFrameworkCore --version 8.0.0
dotnet add package Microsoft.EntityFrameworkCore.Relational --version 8.0.0

echo ""
echo "2. Configuration et Logging..."
dotnet add package Microsoft.Extensions.Configuration.Abstractions --version 10.0.0
dotnet add package Microsoft.Extensions.Logging.Abstractions --version 10.0.0

echo ""
echo "3. JWT Authentication (version sécurisée)..."
dotnet add package System.IdentityModel.Tokens.Jwt --version 8.1.2
dotnet add package Microsoft.IdentityModel.Tokens --version 8.1.2

echo ""
echo "4. BCrypt pour les mots de passe..."
dotnet add package BCrypt.Net-Next --version 4.0.3

echo ""
echo "5. QR Code Generator..."
dotnet add package QRCoder --version 1.4.3

echo ""
echo "6. Validation des données..."
dotnet add package System.ComponentModel.Annotations --version 5.0.0

echo ""
echo "Tous les packages sont installés !"
echo ""
echo "Restauration des dépendances..."
dotnet restore

echo ""
echo "🏗️  Compilation..."
cd ..
dotnet build JO2024.Core

echo ""
echo "Terminé !"