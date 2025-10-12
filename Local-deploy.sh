#!/bin/bash



# Nettoyer complètement
docker-compose down -v
docker system prune -af
docker volume prune -f

# Rebuild from scratch
docker-compose build --no-cache

# Démarrer
docker-compose up -d

# Docker visualisation
docker ps


# Voir les logs
# docker-compose logs -f api

