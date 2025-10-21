#!/bin/bash

# Nettoyer complètement
docker-compose -f desktop-docker-compose.yml down -v
docker system prune -af
docker volume prune -f

# Rebuild from scratch
docker-compose -f desktop-docker-compose.yml build --no-cache

# Démarrer
docker-compose -f desktop-docker-compose.yml up -d

# Docker visualisation
docker ps

# Voir les logs
# docker-compose -f desktop-docker-compose.yml logs -f api
