FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copier les fichiers csproj et restaurer les dépendances
COPY ["JO2024.API/JO2024.API.csproj", "JO2024.API/"]
COPY ["JO2024.Core/JO2024.Core.csproj", "JO2024.Core/"]
COPY ["JO2024.Infrastructure/JO2024.Infrastructure.csproj", "JO2024.Infrastructure/"]
RUN dotnet restore "JO2024.API/JO2024.API.csproj"

# Copier tout le reste et build
COPY . .
WORKDIR "/src/JO2024.API"
RUN dotnet build "JO2024.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "JO2024.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "JO2024.API.dll"]