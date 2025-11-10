#!/bin/bash

# Script de déploiement local du projet JO2024 avec Docker Desktop

set -e  # Stoppe le script en cas d'erreur

echo "================================================"
echo "   Déploiement JO2024 - Jeux Olympiques 2024"
echo "================================================"
echo ""

# Couleurs pour les messages
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Fonction pour afficher les messages
log_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Vérifier que Docker est installé
if ! command -v docker &> /dev/null; then
    log_error "Docker n'est pas installé. Veuillez installer Docker Desktop."
    exit 1
fi

# Vérifier que docker-compose est installé
if ! command -v docker-compose &> /dev/null; then
    log_error "docker-compose n'est pas installé."
    exit 1
fi

log_info "Docker et docker-compose sont installés ✓"
echo ""

# Étape 1: Nettoyage
log_warn "Nettoyage des conteneurs et images existants..."
docker-compose -f desktop-docker-compose.yml down -v 2>/dev/null || true
log_info "Nettoyage terminé ✓"
echo ""

# Étape 2: Nettoyage des fichiers problématiques
log_warn "Nettoyage des fichiers et dossiers problématiques..."

# Supprimer le JO2024API.csproj à la racine (doublon)
if [ -f "JO2024API.csproj" ]; then
    log_warn "Suppression du JO2024API.csproj à la racine (doublon)..."
    rm JO2024API.csproj
fi

# Supprimer le dossier obj/ à la racine
if [ -d "obj" ]; then
    log_warn "Suppression du dossier obj/ à la racine..."
    rm -rf ./obj/
fi

# Nettoyage des dossiers obj/ et bin/ dans JO2024_Backend/
find ./JO2024_Backend -type d -name "obj" -exec rm -rf {} + 2>/dev/null || true
find ./JO2024_Backend -type d -name "bin" -exec rm -rf {} + 2>/dev/null || true

log_info "Nettoyage terminé ✓"
echo ""

# Étape 3: Vérification des fichiers frontend
log_info "Vérification des fichiers frontend..."
if [ ! -d "./frontend/html" ]; then
    log_error "Le dossier ./frontend/html n'existe pas!"
    exit 1
fi

if [ ! -f "./frontend/html/index.html" ]; then
    log_error "Le fichier ./frontend/html/index.html n'existe pas!"
    exit 1
fi

log_info "Fichiers frontend trouvés ✓"
echo ""

# Étape 4: Construction des images
log_info "Construction des images Docker (cela peut prendre quelques minutes)..."
docker-compose -f desktop-docker-compose.yml build --no-cache
log_info "Construction terminée ✓"
echo ""

# Étape 5: Démarrage des conteneurs
log_info "Démarrage des conteneurs..."
docker-compose -f desktop-docker-compose.yml up -d
log_info "Conteneurs démarrés ✓"
echo ""

# Étape 6: Attendre que les services soient prêts
log_info "Attente du démarrage complet des services..."
sleep 10

# Étape 7: Forcer le rechargement du frontend
log_info "Rechargement du frontend pour appliquer les changements..."
docker-compose -f desktop-docker-compose.yml restart frontend
sleep 3
log_info "Frontend rechargé ✓"
echo ""

# Étape 8: Vérifier le contenu du conteneur frontend
log_info "Vérification du contenu du frontend dans le conteneur..."
docker exec jo2024_frontend ls -la /usr/share/nginx/html/ | head -5
echo ""

# Étape 9: Vérifier l'état des conteneurs
log_info "État des conteneurs:"
docker-compose -f desktop-docker-compose.yml ps
echo ""

# Étape 10: Tester les URLs
log_info "Test de connectivité..."
sleep 2

# Test frontend
if curl -s -o /dev/null -w "%{http_code}" http://localhost:3000 | grep -q "200\|301\|302"; then
    log_info "Frontend accessible ✓"
else
    log_warn "Frontend non accessible (vérifiez les logs)"
fi

# Test API
if curl -s -o /dev/null -w "%{http_code}" http://localhost:5000 | grep -q "200\|301\|302\|404"; then
    log_info "API accessible ✓"
else
    log_warn "API non accessible (vérifiez les logs)"
fi

echo ""

# Afficher les informations d'accès
echo "================================================"
log_info "Déploiement terminé avec succès! ✓"
echo "================================================"
echo ""
echo "Services disponibles:"
echo "  • Frontend:   http://localhost:3000"
echo "  • API:        http://localhost:5000"
echo "  • Swagger:    http://localhost:5000/swagger"
echo "  • phpMyAdmin: http://localhost:8080"
echo "  • Logs Cron:   ./scheduler_logs/cron.log
echo ""
echo "Commandes utiles:"
echo "  • Voir les logs de l'API:        docker-compose -f desktop-docker-compose.yml logs -f api"
echo "  • Voir les logs du frontend:     docker-compose -f desktop-docker-compose.yml logs -f frontend"
echo "  • Voir tous les logs:            docker-compose -f desktop-docker-compose.yml logs -f"
echo "  • Arrêter les services:          docker-compose -f desktop-docker-compose.yml down"
echo "  • Redémarrer le frontend:        docker-compose -f desktop-docker-compose.yml restart frontend"
echo "  • lancer manuellement un envoi   docker exec  jo2024_scheduler /app/send-newsletter.sh
echo "  • voir le résultat               ./scheduler_logs/report.log
echo "  • voir les logs cron             ./scheduler_logs/cron.log
echo ""
log_warn "IMPORTANT: Si vous modifiez des fichiers HTML/CSS/JS, faites Ctrl+Shift+R dans le navigateur pour vider le cache!"
echo ""
log_warn "Si vous rencontrez des erreurs, consultez les logs avec les commandes ci-dessus."