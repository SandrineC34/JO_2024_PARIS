# ================================
# Étape 1 : Build
# ================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copier uniquement les fichiers de projet pour restaurer les dépendances
COPY JO2024_Backend/JO2024.API/JO2024.API.csproj JO2024.API/
COPY JO2024_Backend/JO2024.Core/JO2024.Core.csproj JO2024.Core/
COPY JO2024_Backend/JO2024.Infrastructure/JO2024.Infrastructure.csproj JO2024.Infrastructure/


# Restaurer les dépendances
RUN dotnet restore JO2024.API/JO2024.API.csproj

# Copier le reste du code source du backend
COPY JO2024_Backend/ ./

# Nettoyer les dossiers bin/ et obj/ pour éviter les conflits éventuels
RUN find . -type d \( -name "bin" -o -name "obj" \) -exec rm -rf {} + || true

# Compiler en mode Release
WORKDIR /src/JO2024.API
RUN dotnet build JO2024.API.csproj -c Release -o /app/build

# ================================
# Étape 2 : Publish
# ================================
FROM build AS publish
RUN dotnet publish JO2024.API.csproj -c Release -o /app/publish /p:UseAppHost=false

# ================================
# Étape 3 : Runtime
# ================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Installer curl (utile pour le healthcheck)
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copier les fichiers publiés
COPY --from=publish /app/publish .

# Créer un script d'entrée robuste
RUN printf '#!/bin/sh\nset -e\n\n\
echo "Démarrage de JO2024 API..."\n\
exec dotnet JO2024.API.dll "$@"\n' > /app/entrypoint.sh && chmod +x /app/entrypoint.sh

# Exposer les ports utilisés
EXPOSE 80
EXPOSE 443

# Variables d'environnement
ENV ASPNETCORE_URLS=http://+:80 \
    DOTNET_RUNNING_IN_CONTAINER=true

# Healthcheck (vérifie l’endpoint /health si défini dans ton API)
HEALTHCHECK --interval=30s --timeout=5s --start-period=40s --retries=3 \
  CMD curl -fsS http://localhost/health || exit 1

# Point d'entrée
ENTRYPOINT ["/app/entrypoint.sh"]
