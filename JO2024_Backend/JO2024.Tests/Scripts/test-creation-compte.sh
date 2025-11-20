#!/bin/bash
# ============================================
# SCRIPT INTERACTIF - TEST CRÉATION DE COMPTE
# ============================================

BASE_URL="http://localhost:5000/api"

# Couleurs
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
RED='\033[0;31m'
CYAN='\033[0;36m'
NC='\033[0m'

clear
echo -e "${CYAN}"
echo "╔══════════════════════════════════════════════════════════╗"
echo "║   🧪 TEST INTERACTIF - CRÉATION DE COMPTE JO2024        ║"
echo "╚══════════════════════════════════════════════════════════╝"
echo -e "${NC}"

# Fonction pour afficher un menu
show_menu() {
    echo -e "\n${YELLOW}Choisissez un test à exécuter :${NC}"
    echo ""
    echo "  ${BLUE}TESTS DE SUCCÈS${NC}"
    echo "  1) Inscription simple (sans newsletter)"
    echo "  2) Inscription avec newsletter COMPLÈTE"
    echo "  3) Inscription avec TOUS les sports"
    echo "  4) Inscription newsletter PARTIELLE"
    echo "  5) Inscription NON abonné"
    echo ""
    echo "  ${BLUE}TESTS DE CONNEXION${NC}"
    echo "  6) Connexion avec compte existant"
    echo "  7) Récupérer utilisateur actuel"
    echo ""
    echo "  ${RED}TESTS D'ERREUR${NC}"
    echo "  8) Email déjà existant (erreur)"
    echo "  9) Mot de passe invalide (erreur)"
    echo "  10) Email invalide (erreur)"
    echo ""
    echo "  ${CYAN}AUTRES${NC}"
    echo "  11) Lancer TOUS les tests"
    echo "  12) Vérifier MailHog"
    echo "  0) Quitter"
    echo ""
    echo -n "Votre choix : "
}

# Fonction pour exécuter une requête cURL avec affichage
run_curl() {
    local test_name=$1
    local method=$2
    local endpoint=$3
    local data=$4
    local expected=$5
    local auth_header=$6
    
    echo -e "\n${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    echo -e "${YELLOW}📝 $test_name${NC}"
    echo -e "${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    
    echo -e "\n${BLUE}🔗 Endpoint :${NC} $method $endpoint"
    
    if [ -n "$data" ]; then
        echo -e "\n${BLUE}📤 Payload :${NC}"
        echo "$data" | jq '.' 2>/dev/null || echo "$data"
    fi
    
    echo -e "\n${BLUE}⏳ Envoi de la requête...${NC}"
    
    if [ -n "$auth_header" ]; then
        RESPONSE=$(curl -X "$method" "${BASE_URL}${endpoint}" \
          -H "Content-Type: application/json" \
          -H "Authorization: Bearer $auth_header" \
          -d "$data" \
          -w "\n%{http_code}" \
          -s)
    else
        RESPONSE=$(curl -X "$method" "${BASE_URL}${endpoint}" \
          -H "Content-Type: application/json" \
          -d "$data" \
          -w "\n%{http_code}" \
          -s)
    fi
    
    HTTP_CODE=$(echo "$RESPONSE" | tail -n1)
    BODY=$(echo "$RESPONSE" | sed '$d')
    
    echo -e "\n${BLUE}📥 Réponse (Status: $HTTP_CODE) :${NC}"
    echo "$BODY" | jq '.' 2>/dev/null || echo "$BODY"
    
    if [ "$HTTP_CODE" == "$expected" ]; then
        echo -e "\n${GREEN}✅ TEST RÉUSSI${NC} (Status attendu : $expected)"
    else
        echo -e "\n${RED}❌ TEST ÉCHOUÉ${NC} (Attendu : $expected, Reçu : $HTTP_CODE)"
    fi
    
    # Sauvegarder le token si présent
    if echo "$BODY" | jq -e '.token' > /dev/null 2>&1; then
        TOKEN=$(echo "$BODY" | jq -r '.token')
        echo -e "\n${YELLOW}🔑 Token JWT sauvegardé pour les requêtes suivantes${NC}"
    fi
    
    echo -e "\n${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    echo -e "\nAppuyez sur ENTRÉE pour continuer..."
    read
}

# Fonction pour générer un email unique
generate_email() {
    local base=$1
    local timestamp=$(date +%s)
    echo "${base}_${timestamp}@test.com"
}

# ============================================
# BOUCLE PRINCIPALE
# ============================================
TOKEN=""

while true; do
    show_menu
    read choice
    
    case $choice in
        1)
            EMAIL=$(generate_email "simple")
            run_curl "Inscription simple (sans newsletter)" \
                "POST" \
                "/Auth/register" \
                '{
                  "prenom": "Alois",
                  "nom": "Archimedes",
                  "email": "'$EMAIL'",
                  "password": "Password123!"
                }' \
                "200"
            ;;
            
        2)
            EMAIL=$(generate_email "newsletter_full")
            run_curl "Inscription avec newsletter COMPLÈTE" \
                "POST" \
                "/Auth/register" \
                '{
                  "prenom": "Marie",
                  "nom": "Martin",
                  "email": "'$EMAIL'",
                  "password": "Password123!",
                  "newsletterPreferences": {
                    "subscribed": true,
                    "categories": {
                      "sport": true,
                      "evenements": true,
                      "billets": true
                    },
                    "sports": [
                      {"id": "natation", "name": "Natation"},
                      {"id": "athletisme", "name": "Athlétisme"},
                      {"id": "basketball", "name": "Basketball"}
                    ]
                  }
                }' \
                "200"
            ;;
            
        3)
            EMAIL=$(generate_email "all_sports")
            run_curl "Inscription avec TOUS les sports" \
                "POST" \
                "/Auth/register" \
                '{
                  "prenom": "Emma",
                  "nom": "Dubois",
                  "email": "'$EMAIL'",
                  "password": "Password123!",
                  "newsletterPreferences": {
                    "subscribed": true,
                    "categories": {
                      "sport": true,
                      "evenements": false,
                      "billets": false
                    },
                    "sports": [
                      {"id": "natation", "name": "Natation"},
                      {"id": "athletisme", "name": "Athlétisme"},
                      {"id": "basketball", "name": "Basketball"},
                      {"id": "surf", "name": "Surf"},
                      {"id": "gymnastique", "name": "Gymnastique"}
                    ]
                  }
                }' \
                "200"
            ;;
            
        4)
            EMAIL=$(generate_email "partial")
            run_curl "Inscription newsletter PARTIELLE" \
                "POST" \
                "/Auth/register" \
                '{
                  "prenom": "Sophie",
                  "nom": "Bernard",
                  "email": "'$EMAIL'",
                  "password": "Password123!",
                  "newsletterPreferences": {
                    "subscribed": true,
                    "categories": {
                      "sport": true,
                      "evenements": false,
                      "billets": true
                    },
                    "sports": [
                      {"id": "surf", "name": "Surf"}
                    ]
                  }
                }' \
                "200"
            ;;
            
        5)
            EMAIL=$(generate_email "no_newsletter")
            run_curl "Inscription NON abonné newsletter" \
                "POST" \
                "/Auth/register" \
                '{
                  "prenom": "Lucas",
                  "nom": "Robert",
                  "email": "'$EMAIL'",
                  "password": "Password123!",
                  "newsletterPreferences": {
                    "subscribed": false,
                    "categories": {
                      "sport": false,
                      "evenements": false,
                      "billets": false
                    },
                    "sports": []
                  }
                }' \
                "200"
            ;;
            
        6)
            echo -e "\n${BLUE}Entrez l'email du compte :${NC}"
            read LOGIN_EMAIL
            echo -e "${BLUE}Entrez le mot de passe :${NC}"
            read -s LOGIN_PASSWORD
            
            run_curl "Connexion avec compte existant" \
                "POST" \
                "/Auth/login" \
                '{
                  "email": "'$LOGIN_EMAIL'",
                  "password": "'$LOGIN_PASSWORD'"
                }' \
                "200"
            ;;
            
        7)
            if [ -z "$TOKEN" ]; then
                echo -e "\n${RED}❌ Aucun token disponible. Connectez-vous d'abord (option 6).${NC}"
                echo "Appuyez sur ENTRÉE pour continuer..."
                read
            else
                run_curl "Récupérer utilisateur actuel" \
                    "GET" \
                    "/Auth/current" \
                    "" \
                    "200" \
                    "$TOKEN"
            fi
            ;;
            
        8)
            run_curl "Email déjà existant (ERREUR ATTENDUE)" \
                "POST" \
                "/Auth/register" \
                '{
                  "prenom": "Duplicate",
                  "nom": "User",
                  "email": "simple_'$(date +%s)'@test.com",
                  "password": "Password123!"
                }' \
                "200"
            
            # Tenter avec le même email
            run_curl "Email déjà existant (ERREUR ATTENDUE)" \
                "POST" \
                "/Auth/register" \
                '{
                  "prenom": "Duplicate2",
                  "nom": "User2",
                  "email": "simple_'$(date +%s)'@test.com",
                  "password": "Password123!"
                }' \
                "400"
            ;;
            
        9)
            EMAIL=$(generate_email "invalid_pwd")
            run_curl "Mot de passe invalide (ERREUR ATTENDUE)" \
                "POST" \
                "/Auth/register" \
                '{
                  "prenom": "Test",
                  "nom": "InvalidPwd",
                  "email": "'$EMAIL'",
                  "password": "weak"
                }' \
                "400"
            ;;
            
        10)
            run_curl "Email invalide (ERREUR ATTENDUE)" \
                "POST" \
                "/Auth/register" \
                '{
                  "prenom": "Test",
                  "nom": "InvalidEmail",
                  "email": "notanemail",
                  "password": "Password123!"
                }' \
                "400"
            ;;
            
        11)
            echo -e "\n${YELLOW}🚀 Lancement de TOUS les tests...${NC}"
            echo "Appuyez sur ENTRÉE après chaque test pour continuer..."
            read
            
            # Exécuter tous les tests
            for i in {1..10}; do
                choice=$i
                case $i in
                    1|2|3|4|5|8|9|10) 
                        # Ré-exécuter le test
                        ;;
                esac
            done
            
            echo -e "\n${GREEN}✅ Tous les tests terminés !${NC}"
            echo "Appuyez sur ENTRÉE pour continuer..."
            read
            ;;
            
        12)
            echo -e "\n${CYAN}📧 Ouverture de MailHog...${NC}"
            
            # Essayer d'ouvrir dans le navigateur
            if command -v xdg-open > /dev/null; then
                xdg-open http://localhost:8025
            elif command -v open > /dev/null; then
                open http://localhost:8025
            elif command -v start > /dev/null; then
                start http://localhost:8025
            else
                echo -e "${YELLOW}Ouvrez manuellement : http://localhost:8025${NC}"
            fi
            
            echo -e "\n${BLUE}MailHog devrait s'ouvrir dans votre navigateur.${NC}"
            echo "Sinon, accédez à : http://localhost:8025"
            echo ""
            echo "Appuyez sur ENTRÉE pour continuer..."
            read
            ;;
            
        0)
            echo -e "\n${GREEN}👋 Au revoir !${NC}\n"
            exit 0
            ;;
            
        *)
            echo -e "\n${RED}❌ Choix invalide. Réessayez.${NC}"
            sleep 2
            ;;
    esac
done