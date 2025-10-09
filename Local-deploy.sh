#!/bin/bash
# Déploiement local du projet JeuxOlympiques avec Docker Desktop

set -e  # Stoppe le script en cas d'erreur

# Nom de l'image et du conteneur
IMAGE_NAME="jeuxolympiques:latest"
CONTAINER_NAME="jeuxolympiques_app"

# Nettoyage ancien build si existant
echo "Suppression ancienne image/ancien conteneur..."
docker rm -f $CONTAINER_NAME 2>/dev/null || true
docker rmi -f $IMAGE_NAME 2>/dev/null || true

# Construction de l'image
echo "Construction de l'image Docker..."
docker build -t $IMAGE_NAME .

# Lancement du conteneur
echo "Démarrage du conteneur..."
docker run -d \
  --name $CONTAINER_NAME \
  -p 8080:8080 \
  $IMAGE_NAME

# Vérification du statut
echo "Application déployée localement sur http://localhost:8080"
docker ps | grep $CONTAINER_NAME
