#!/usr/bin/env bash

# Script de déploiement local du projet JO2024 avec Docker Desktop
# - Détecte docker compose v2 ou docker-compose (compatibilité)
# - Nettoie les builds/obj, arrête et supprime conteneurs/volumes
# - Reconstruit sans cache et relance les services
# - Vérifie l'accessibilité des services

set -euo pipefail

echo "================================================"
echo "   Déploiement JO2024 - Jeux Olympiques 2024"
echo "================================================"
echo ""

# Couleurs pour les messages
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

log_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Détecter la commande docker-compose (v1) ou docker compose (v2)
DOCKER_COMPOSE_CMD=""
if command -v docker-compose &> /dev/null; then
    DOCKER_COMPOSE_CMD="docker-compose"
elif docker compose version &> /dev/null; then
    DOCKER_COMPOSE_CMD="docker compose"
else
    log_error "docker-compose (v1) ou docker compose (v2) introuvable. Veuillez installer Docker Desktop."
    exit 1
fi

log_info "Utilisation de la commande : ${DOCKER_COMPOSE_CMD}"
echo ""

# Vérifier que docker est installé et accessible
if ! command -v docker &> /dev/null; then
    log_error "Docker n'est pas installé ou non accessible."
    exit 1
fi

log_info "Docker est installé ✓"
echo ""

# Fichier compose
COMPOSE_FILE="desktop-docker-compose.yml"

# Étape 1: Nettoyage (arrêt si existant)
log_warn "Arrêt et suppression (si existants) des conteneurs et volumes définis dans ${COMPOSE_FILE}..."
# on ignore les erreurs si déjà down
set +e
${DOCKER_COMPOSE_CMD} -f "${COMPOSE_FILE}" down -v
DOWN_STATUS=$?
set -e
if [ $DOWN_STATUS -eq 0 ]; then
    log_info "Arrêt et suppression terminés."
else
    log_warn "Aucun conteneur actif pour ${COMPOSE_FILE} ou erreur lors de l'arrêt (non bloquant)."
fi
echo ""

# Étape 2: Nettoyage des fichiers problématiques
log_warn "Nettoyage des fichiers et dossiers problématiques..."

# Supprimer le JO2024API.csproj à la racine (doublon)
if [ -f "JO2024API.csproj" ]; then
    log_warn "Suppression du JO2024API.csproj à la racine (doublon)..."
    rm -f JO2024API.csproj
fi

# Supprimer le dossier obj/ à la racine
if [ -d "obj" ]; then
    log_warn "Suppression du dossier obj/ à la racine..."
    rm -rf ./obj/
fi

# Nettoyage des dossiers obj/ et bin/ dans JO2024_Backend/
if [ -d "JO2024_Backend" ]; then
    find ./JO2024_Backend -type d -name "obj" -prune -exec rm -rf {} + 2>/dev/null || true
    find ./JO2024_Backend -type d -name "bin" -prune -exec rm -rf {} + 2>/dev/null || true
fi

log_info "Nettoyage terminé ✓"
echo ""

# Étape 3: Vérification des fichiers frontend
log_info "Vérification des fichiers frontend..."
if [ ! -d "./frontend" ]; then
    log_error "Le dossier ./frontend n'existe pas!"
    exit 1
fi

if [ ! -f "./frontend/index.html" ] && [ ! -f "./frontend/html/index.html" ]; then
    log_warn "index.html non trouvé dans ./frontend/ ni ./frontend/html/ (assurez-vous du chemin)."
else
    log_info "Fichiers frontend trouvés ✓"
fi
echo ""

# Étape 4: Reconstruction des images
log_info "Construction des images Docker (no-cache) ..."
${DOCKER_COMPOSE_CMD} -f "${COMPOSE_FILE}" build --no-cache
log_info "Construction terminée ✓"
echo ""

# Étape 5: Démarrage des conteneurs
log_info "Démarrage des conteneurs..."
${DOCKER_COMPOSE_CMD} -f "${COMPOSE_FILE}" up -d
log_info "Conteneurs démarrés ✓"
echo ""

# Étape 6: Attente que les services soient prêts (basic wait)
log_info "Attente du démarrage complet des services (10s)..."
sleep 10

# Étape 7: Forcer le rechargement du frontend (restart)
log_info "Redémarrage du service frontend pour appliquer les changements..."
# Nom du service dans compose : frontend
set +e
${DOCKER_COMPOSE_CMD} -f "${COMPOSE_FILE}" restart frontend
set -e
sleep 3
log_info "Frontend rechargé ✓"
echo ""

# Étape 8: Vérifier le contenu du conteneur frontend
log_info "Vérification du contenu du frontend dans le conteneur (les 20 premiers fichiers)..."
if docker ps --format '{{.Names}}' | grep -q jo2024_frontend; then
    docker exec jo2024_frontend ls -la /usr/share/nginx/html/ | head -n 20 || true
else
    log_warn "Conteneur jo2024_frontend non trouvé."
fi
echo ""

# Étape 9: Vérifier l'état des conteneurs
log_info "État des conteneurs:"
${DOCKER_COMPOSE_CMD} -f "${COMPOSE_FILE}" ps || true
echo ""

# Étape 10: Tests de connectivité par curl (localhost)
log_info "Test de connectivité..."
sleep 2

# Test frontend (port 3000 -> nginx)
FRONTEND_CODE=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:3000 || echo "000")
if echo "${FRONTEND_CODE}" | grep -qE "^(200|301|302)$"; then
    log_info "Frontend accessible (http://localhost:3000) ✓"
else
    log_warn "Frontend non accessible (code: ${FRONTEND_CODE}). Vérifiez les logs : ${DOCKER_COMPOSE_CMD} -f ${COMPOSE_FILE} logs -f frontend"
fi

# Test API (port 5000 -> api)
API_CODE=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:5000 || echo "000")
if echo "${API_CODE}" | grep -qE "^(200|301|302|404)$"; then
    log_info "API accessible (http://localhost:5000) ✓"
else
    log_warn "API non accessible (code: ${API_CODE}). Vérifiez les logs : ${DOCKER_COMPOSE_CMD} -f ${COMPOSE_FILE} logs -f api"
fi

echo ""

# Résumé et commandes utiles
echo "================================================"
log_info "Déploiement terminé (partiellement vérifié)."
echo "================================================"
echo ""
echo "Services disponibles (si démarrés):"
echo "  • Frontend:   http://localhost:3000"
echo "  • API:        http://localhost:5000"
echo "  • Swagger:    http://localhost:5000/swagger"
echo "  • phpMyAdmin: http://localhost:8080"
echo "  • MailHog:    http://localhost:8025"
echo ""
echo "Commandes utiles:"
echo "  • Voir les logs de l'API:        ${DOCKER_COMPOSE_CMD} -f ${COMPOSE_FILE} logs -f api"
echo "  • Voir les logs du frontend:     ${DOCKER_COMPOSE_CMD} -f ${COMPOSE_FILE} logs -f frontend"
echo "  • Voir tous les logs:            ${DOCKER_COMPOSE_CMD} -f ${COMPOSE_FILE} logs -f"
echo "  • Arrêter les services:          ${DOCKER_COMPOSE_CMD} -f ${COMPOSE_FILE} down"
echo "  • Redémarrer le frontend:        ${DOCKER_COMPOSE_CMD} -f ${COMPOSE_FILE} restart frontend"
echo "  • Lancer manuellement un envoi:   docker exec jo2024_scheduler /app/send-newsletter.sh"
echo "  • Voir le résultat:               ./scheduler_logs/report.log"
echo "  • Voir les logs cron:             ./scheduler_logs/cron.log"
echo ""
log_warn "IMPORTANT: Si vous modifiez des fichiers HTML/CSS/JS, faites Ctrl+Shift+R dans le navigateur pour vider le cache!"
echo ""
log_warn "Si vous rencontrez des erreurs, consultez les logs avec les commandes ci-dessus."
echo ""
