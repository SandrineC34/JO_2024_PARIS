#!/bin/bash

# ============================================
# Script de démarrage rapide JO2024
# ============================================

set -e  # Arrêter en cas d'erreur

echo "🏅 ============================================"
echo "🏅  Installation JO 2024 - Backend & Database"
echo "🏅 ============================================"
echo ""

# Couleurs
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Fonction pour afficher les messages
print_info() {
    echo -e "${BLUE}ℹ️  $1${NC}"
}

print_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

print_error() {
    echo -e "${RED}❌ $1${NC}"
}

# Vérifier les prérequis
print_info "Vérification des prérequis..."

command -v dotnet >/dev/null 2>&1 || { 
    print_error ".NET 8.0 SDK n'est pas installé. Téléchargez-le sur https://dotnet.microsoft.com/download"
    exit 1
}
print_success ".NET SDK trouvé: $(dotnet --version)"

command -v docker >/dev/null 2>&1 || { 
    print_error "Docker n'est pas installé. Téléchargez-le sur https://www.docker.com/products/docker-desktop"
    exit 1
}
print_success "Docker trouvé: $(docker --version)"

command -v docker-compose >/dev/null 2>&1 || { 
    print_error "Docker Compose n'est pas installé."
    exit 1
}
print_success "Docker Compose trouvé: $(docker-compose --version)"

echo ""
print_info "Tous les prérequis sont satisfaits!"
echo ""

# Demander si on doit créer la structure
read -p "Voulez-vous créer la structure du projet? (o/n) " -n 1 -r
echo
if [[ $REPLY =~ ^[Oo]$ ]]
then
    print_info "Création de la structure du projet..."
    
    # Créer la solution
    dotnet new sln -n JO2024
    
    # Créer les projets
    print_info "Création des projets..."
    dotnet new webapi -n JO2024.API
    dotnet new classlib -n JO2024.Core
    dotnet new classlib -n JO2024.Infrastructure
    dotnet new xunit -n JO2024.Tests
    
    # Ajouter à la solution
    print_info "Ajout des projets à la solution..."
    dotnet sln add JO2024.API/JO2024.API.csproj
    dotnet sln add JO2024.Core/JO2024.Core.csproj
    dotnet sln add JO2024.Infrastructure/JO2024.Infrastructure.csproj
    dotnet sln add JO2024.Tests/JO2024.Tests.csproj
    
    # Ajouter les références
    print_info "Configuration des références entre projets..."
    dotnet add JO2024.API/JO2024.API.csproj reference JO2024.Core/JO2024.Core.csproj
    dotnet add JO2024.API/JO2024.API.csproj reference JO2024.Infrastructure/JO2024.Infrastructure.csproj
    dotnet add JO2024.Infrastructure/JO2024.Infrastructure.csproj reference JO2024.Core/JO2024.Core.csproj
    dotnet add JO2024.Tests/JO2024.Tests.csproj reference JO2024.Core/JO2024.Core.csproj
    
    print_success "Structure du projet créée!"
fi

echo ""

# Installer les packages NuGet
read -p "Voulez-vous installer les packages NuGet? (o/n) " -n 1 -r
echo
if [[ $REPLY =~ ^[Oo]$ ]]
then
    print_info "Installation des packages NuGet..."
    
    # JO2024.API
    print_info "Installation des packages pour JO2024.API..."
    cd JO2024.API
    dotnet add package Pomelo.EntityFrameworkCore.MySql --version 8.0.0
    dotnet add package Microsoft.EntityFrameworkCore.Tools --version 8.0.0
    dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.0
    dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.0
    dotnet add package BCrypt.Net-Next --version 4.0.3
    dotnet add package Swashbuckle.AspNetCore --version 6.5.0
    dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection --version 12.0.1
    dotnet add package FluentValidation.AspNetCore --version 11.3.0
    dotnet add package MailKit --version 4.3.0
    dotnet add package QRCoder --version 1.4.3
    dotnet add package itext7 --version 8.0.2
    cd ..
    
    # JO2024.Infrastructure
    print_info "Installation des packages pour JO2024.Infrastructure..."
    cd JO2024.Infrastructure
    dotnet add package Pomelo.EntityFrameworkCore.MySql --version 8.0.0
    dotnet add package Microsoft.EntityFrameworkCore.Tools --version 8.0.0
    cd ..
    
    # JO2024.Tests
    print_info "Installation des packages pour JO2024.Tests..."
    cd JO2024.Tests
    dotnet add package Microsoft.EntityFrameworkCore.InMemory --version 8.0.0
    dotnet add package Moq --version 4.20.70
    dotnet add package FluentAssertions --version 6.12.0
    cd ..
    
    print_success "Tous les packages ont été installés!"
fi

echo ""

# Démarrer Docker
read -p "Voulez-vous démarrer les conteneurs Docker (MySQL + phpMyAdmin)? (o/n) " -n 1 -r
echo
if [[ $REPLY =~ ^[Oo]$ ]]
then
    print_info "Démarrage des conteneurs Docker..."
    docker-compose up -d
    
    print_warning "Attente du démarrage de MySQL (30 secondes)..."
    sleep 30
    
    print_success "Conteneurs Docker démarrés!"
    echo ""
    print_info "Services disponibles:"
    echo "   - MySQL: localhost:3306"
    echo "   - phpMyAdmin: http://localhost:8080"
    echo ""
fi

# Appliquer les migrations
read -p "Voulez-vous appliquer les migrations Entity Framework? (o/n) " -n 1 -r
echo
if [[ $REPLY =~ ^[Oo]$ ]]
then
    print_info "Application des migrations..."
    cd JO2024.API
    
    # Créer la migration initiale
    print_info "Création de la migration initiale..."
    dotnet ef migrations add InitialCreate --project ../JO2024.Infrastructure/JO2024.Infrastructure.csproj
    
    # Appliquer les migrations
    print_info "Application des migrations à la base de données..."
    dotnet ef database update --project ../JO2024.Infrastructure/JO2024.Infrastructure.csproj
    
    cd ..
    
    print_success "Migrations appliquées avec succès!"
fi

echo ""

# Build du projet
read -p "Voulez-vous compiler le projet? (o/n) " -n 1 -r
echo
if [[ $REPLY =~ ^[Oo]$ ]]
then
    print_info "Compilation du projet..."
    dotnet build
    print_success "Projet compilé avec succès!"
fi

echo ""

# Lancer l'API
read -p "Voulez-vous démarrer l'API maintenant? (o/n) " -n 1 -r
echo
if [[ $REPLY =~ ^[Oo]$ ]]
then
    print_success "Démarrage de l'API..."
    echo ""
    print_info "L'API sera disponible sur:"
    echo "   - HTTP: http://localhost:5000"
    echo "   - HTTPS: https://localhost:5001"
    echo "   - Swagger: http://localhost:5000/swagger"
    echo ""
    print_info "Appuyez sur Ctrl+C pour arrêter l'API"
    echo ""
    
    cd JO2024.API
    dotnet run
else
    echo ""
    print_success "🎉 Installation terminée avec succès!"
    echo ""
    print_info "Pour démarrer l'API plus tard, exécutez:"
    echo "   cd JO2024.API"
    echo "   dotnet run"
    echo ""
    print_info "Pour accéder à Swagger:"
    echo "   http://localhost:5000/swagger"
    echo ""
    print_info "Pour accéder à phpMyAdmin:"
    echo "   http://localhost:8080"
    echo "   Utilisateur: jo2024_user"
    echo "   Mot de passe: JO2024Pass123!"
    echo ""
fi

# ============================================
# Version Windows (PowerShell)
# setup.ps1
# ============================================
: '
# Enregistrez ce contenu dans setup.ps1

Write-Host "🏅 ============================================" -ForegroundColor Cyan
Write-Host "🏅  Installation JO 2024 - Backend & Database" -ForegroundColor Cyan
Write-Host "🏅 ============================================" -ForegroundColor Cyan
Write-Host ""

function Print-Info($message) {
    Write-Host "ℹ️  $message" -ForegroundColor Blue
}

function Print-Success($message) {
    Write-Host "✅ $message" -ForegroundColor Green
}

function Print-Warning($message) {
    Write-Host "⚠️  $message" -ForegroundColor Yellow
}

function Print-Error($message) {
    Write-Host "❌ $message" -ForegroundColor Red
}

# Vérifier les prérequis
Print-Info "Vérification des prérequis..."

if (!(Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Print-Error ".NET 8.0 SDK nest pas installé"
    exit 1
}
Print-Success ".NET SDK trouvé: $(dotnet --version)"

if (!(Get-Command docker -ErrorAction SilentlyContinue)) {
    Print-Error "Docker nest pas installé"
    exit 1
}
Print-Success "Docker trouvé"

Write-Host ""

# Créer la structure
$createStructure = Read-Host "Voulez-vous créer la structure du projet? (o/n)"
if ($createStructure -eq "o") {
    Print-Info "Création de la structure..."
    
    dotnet new sln -n JO2024
    dotnet new webapi -n JO2024.API
    dotnet new classlib -n JO2024.Core
    dotnet new classlib -n JO2024.Infrastructure
    dotnet new xunit -n JO2024.Tests
    
    dotnet sln add JO2024.API/JO2024.API.csproj
    dotnet sln add JO2024.Core/JO2024.Core.csproj
    dotnet sln add JO2024.Infrastructure/JO2024.Infrastructure.csproj
    dotnet sln add JO2024.Tests/JO2024.Tests.csproj
    
    dotnet add JO2024.API/JO2024.API.csproj reference JO2024.Core/JO2024.Core.csproj
    dotnet add JO2024.API/JO2024.API.csproj reference JO2024.Infrastructure/JO2024.Infrastructure.csproj
    dotnet add JO2024.Infrastructure/JO2024.Infrastructure.csproj reference JO2024.Core/JO2024.Core.csproj
    dotnet add JO2024.Tests/JO2024.Tests.csproj reference JO2024.Core/JO2024.Core.csproj
    
    Print-Success "Structure créée!"
}

# Installer les packages
$installPackages = Read-Host "Voulez-vous installer les packages NuGet? (o/n)"
if ($installPackages -eq "o") {
    Print-Info "Installation des packages..."
    
    cd JO2024.API
    dotnet add package Pomelo.EntityFrameworkCore.MySql --version 8.0.0
    dotnet add package Microsoft.EntityFrameworkCore.Tools --version 8.0.0
    dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.0
    dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.0
    dotnet add package BCrypt.Net-Next --version 4.0.3
    dotnet add package Swashbuckle.AspNetCore --version 6.5.0
    dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection --version 12.0.1
    dotnet add package FluentValidation.AspNetCore --version 11.3.0
    dotnet add package QRCoder --version 1.4.3
    cd ..
    
    cd JO2024.Infrastructure
    dotnet add package Pomelo.EntityFrameworkCore.MySql --version 8.0.0
    dotnet add package Microsoft.EntityFrameworkCore.Tools --version 8.0.0
    cd ..
    
    cd JO2024.Tests
    dotnet add package Microsoft.EntityFrameworkCore.InMemory --version 8.0.0
    dotnet add package Moq --version 4.20.70
    cd ..
    
    Print-Success "Packages installés!"
}

# Démarrer Docker
$startDocker = Read-Host "Voulez-vous démarrer Docker? (o/n)"
if ($startDocker -eq "o") {
    Print-Info "Démarrage de Docker..."
    docker-compose up -d
    Print-Warning "Attente de MySQL (30 secondes)..."
    Start-Sleep -Seconds 30
    Print-Success "Docker démarré!"
}

# Migrations
$applyMigrations = Read-Host "Voulez-vous appliquer les migrations? (o/n)"
if ($applyMigrations -eq "o") {
    cd JO2024.API
    dotnet ef migrations add InitialCreate --project ../JO2024.Infrastructure/JO2024.Infrastructure.csproj
    dotnet ef database update --project ../JO2024.Infrastructure/JO2024.Infrastructure.csproj
    cd ..
    Print-Success "Migrations appliquées!"
}

Write-Host ""
Print-Success "🎉 Installation terminée!"
Write-Host ""
Print-Info "Pour démarrer lAPI:"
Write-Host "   cd JO2024.API"
Write-Host "   dotnet run"
Write-Host ""
'