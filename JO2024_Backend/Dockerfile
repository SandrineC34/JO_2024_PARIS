# backend/Dockerfile
# Dockerfile pour l'API .NET 9.0

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copier les fichiers .csproj et restaurer les dépendances
COPY ["JO2024.API/JO2024.API.csproj", "JO2024.API/"]
COPY ["JO2024.Core/JO2024.Core.csproj", "JO2024.Core/"]
COPY ["JO2024.Infrastructure/JO2024.Infrastructure.csproj", "JO2024.Infrastructure/"]

RUN dotnet restore "JO2024.API/JO2024.API.csproj"

# Copier tout le reste du code
COPY . .

# Build du projet
WORKDIR "/src/JO2024.API"
RUN dotnet build "JO2024.API.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "JO2024.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Installer les outils pour les migrations (optionnel)
# RUN dotnet tool install --global dotnet-ef

EXPOSE 80
EXPOSE 443

COPY --from=publish /app/publish .

# Point d'entrée
ENTRYPOINT ["dotnet", "JO2024.API.dll"]