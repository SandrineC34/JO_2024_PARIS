#!/bin/bash
# ============================================
# Script de diagnostic JO2024
# ============================================

echo "Diagnostic de l'application JO2024 en local"
echo "======================================"
echo ""

# Vérifier Docker
echo "Vérification de Docker..."
docker --version
docker-compose --version
echo ""

# Lister les conteneurs
echo "Conteneurs en cours d'exécution:"
docker ps
echo ""

# Vérifier la santé de MySQL
echo "Vérification de MySQL..."
docker exec jo2024_mysql mysqladmin ping -h localhost -ujo2024_user -pJO2024Pass123! 2>/dev/null
if [ $? -eq 0 ]; then
    echo "MySQL est accessible"
else
    echo "MySQL n'est pas accessible"
fi
echo ""

# Vérifier la base de données
echo "Vérification de la base de données..."
docker exec jo2024_mysql mysql -ujo2024_user -pJO2024Pass123! -e "USE jo2024_db; SHOW TABLES;" 2>/dev/null
echo ""

# Compter les enregistrements
echo "Comptage des données..."
docker exec jo2024_mysql mysql -ujo2024_user -pJO2024Pass123! -e "
USE jo2024_db;
SELECT 'Offres' as Table_Name, COUNT(*) as Count FROM Offres
UNION ALL
SELECT 'Utilisateurs', COUNT(*) FROM Utilisateurs
UNION ALL
SELECT 'Commandes', COUNT(*) FROM Commandes
UNION ALL
SELECT 'Billets', COUNT(*) FROM Billets;
" 2>/dev/null
echo ""

# Tester l'API
echo "🔌 Test de l'API..."
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:5000/api/offres)
if [ "$HTTP_CODE" == "200" ]; then
    echo "API répond correctement (HTTP $HTTP_CODE)"
    echo "Données des offres:"
    curl -s http://localhost:5000/api/offres | python3 -m json.tool 2>/dev/null || curl -s http://localhost:5000/api/offres
else
    echo "Problème avec l'API (HTTP $HTTP_CODE)"
fi
echo ""

# Logs récents de l'API
echo "Derniers logs de l'API:"
docker logs jo2024_api --tail 20
echo ""

echo "======================================"
echo "Diagnostic terminé"
echo ""
echo "URLs d'accès:"
echo "   - Frontend:   http://localhost:3000"
echo "   - API:        http://localhost:5000"
echo "   - Swagger:    http://localhost:5000/swagger"
echo "   - phpMyAdmin: http://localhost:8080"