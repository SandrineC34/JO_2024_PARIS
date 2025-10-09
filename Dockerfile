# Étape 1 : Build de l’application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copier uniquement le fichier projet pour restaurer les dépendances
COPY ["JeuxOlympiques.csproj", "."]
RUN dotnet restore "JeuxOlympiques.csproj"

# Copier le reste du projet
COPY . .

# Compiler le projet en mode Release
RUN dotnet build "JeuxOlympiques.csproj" -c Release -o /app/build

# Étape 2 : Publication
FROM build AS publish
RUN dotnet publish "JeuxOlympiques.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Étape 3 : Exécution
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Port d’écoute
EXPOSE 8080

# Lancement de l’application
ENTRYPOINT ["dotnet", "JeuxOlympiques.dll"]
