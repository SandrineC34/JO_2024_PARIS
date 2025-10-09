# Étape 1 : Build de l’application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copier uniquement le fichier projet pour restaurer les dépendances
COPY ["JO2024API.csproj", "."]
RUN dotnet restore "JO2024API.csproj"

# Copier le reste du projet
COPY . .

# Compiler le projet en mode Release
RUN dotnet build "JO2024API.csproj" -c Release -o /app/build

# Étape 2 : Publication
FROM build AS publish
RUN dotnet publish "JO2024API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Étape 3 : Exécution
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Port d’écoute
EXPOSE 8080

# Lancement de l’application
ENTRYPOINT ["dotnet", "JO2024API.dll"]
