# =====================================================
# 🐳 Dockerfile pour l'API .NET 8.0
# Inclut deux environnements :
#   - Docker local (MySQL)
#   - Render (PostgreSQL) [commenté]
# =====================================================

# ===============================
# === Étape 1 : Build commun ===
# ===============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copier les fichiers .csproj et restaurer les dépendances
COPY ["JO2024.API/JO2024.API.csproj", "JO2024.API/"]
COPY ["JO2024.Core/JO2024.Core.csproj", "JO2024.Core/"]
COPY ["JO2024.Infrastructure/JO2024.Infrastructure.csproj", "JO2024.Infrastructure/"]

RUN dotnet restore "JO2024.API/JO2024.API.csproj"

# Copier tout le code source
COPY . .

# Build du projet
WORKDIR "/src/JO2024.API"
RUN dotnet build "JO2024.API.csproj" -c Release -o /app/build


# ================================
# === Étape 2 : Publish commun ===
# ================================
FROM build AS publish
RUN dotnet publish "JO2024.API.csproj" -c Release -o /app/publish /p:UseAppHost=false


# ============================================================
# === 🐋 Section active : Déploiement Docker local (MySQL) ===
# ============================================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# --- Docker Local : Configuration standard ---
# Port exposé par défaut pour Docker local
EXPOSE 8080

# Copie du build publié
COPY --from=publish /app/publish .

# Point d’entrée de l’application
ENTRYPOINT ["dotnet", "JO2024.API.dll"]

# ============================================================
# === 🌐 Section Render (commentée pour l’instant) ============
# === Décommente si tu veux déployer sur Render ==============
# ============================================================
# # === Render ===
# FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
# WORKDIR /app
#
# # Render fournit dynamiquement le port via la variable PORT
# ENV ASPNETCORE_URLS=http://+:${PORT}
#
# # Exposition du port (Render l’ignore, mais bonne pratique)
# EXPOSE ${PORT}
#
# COPY --from=publish /app/publish .
#
# ENTRYPOINT ["dotnet", "JO2024.API.dll"]
# ============================================================

