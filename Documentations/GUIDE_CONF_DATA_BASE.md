# Guide Configuration Base de Données - 2 Approches

Ce guide explique comment configurer votre base de données de **deux manières différentes** :

1. ** Docker en local** - Pour le développement
2. ** Render PostgreSQL** - Pour la production

---

## OPTION 1 : Base de données Docker en LOCAL

### Avantages
- Totalement gratuit
- Fonctionne hors ligne
- Contrôle total
- Parfait pour le développement
- Facile à réinitialiser

### Prérequis
- Docker Desktop installé ([télécharger](https://www.docker.com/products/docker-desktop))
- .NET 8 SDK installé

---

### Étape 1 : Installer Docker Desktop

#### Windows
1. Téléchargez Docker Desktop : https://www.docker.com/products/docker-desktop
2. Installez et redémarrez votre PC
3. Lancez Docker Desktop
4. Vérifiez l'installation :
   ```bash
   docker --version
   # Docker version 24.0.x
   ```

#### macOS
```bash
brew install --cask docker
```

#### Linux
```bash
sudo apt-get update
sudo apt-get install docker-ce docker-ce-cli containerd.io
```

---

### Étape 2 : Créer le fichier docker-compose.yml

À la **racine de votre projet**, créez `docker-compose.yml` :

```yaml
version: '3.8'

services:
  # Base de données MySQL
  mysql:
    image: mysql:8.0
    container_name: jo_mysql_db
    restart: always
    environment:
      MYSQL_ROOT_PASSWORD: RootPassword123!
      MYSQL_DATABASE: jeuxolympiques
      MYSQL_USER: jo_user
      MYSQL_PASSWORD: JO2024Password!
    ports:
      - "3306:3306"
    volumes:
      # Persister les données
      - mysql_data:/var/lib/mysql
      # Script d'initialisation (optionnel)
      - ./init.sql:/docker-entrypoint-initdb.d/init.sql
    networks:
      - jo_network
    healthcheck:
      test: ["CMD", "mysqladmin", "ping", "-h", "localhost", "-u", "root", "-p$$MYSQL_ROOT_PASSWORD"]
      interval: 10s
      timeout: 5s
      retries: 5

  # PHPMyAdmin (interface web pour gérer MySQL)
  phpmyadmin:
    image: phpmyadmin:latest
    container_name: jo_phpmyadmin
    restart: always
    environment:
      PMA_HOST: mysql
      PMA_PORT: 3306
      PMA_USER: jo_user
      PMA_PASSWORD: JO2024Password!
    ports:
      - "8080:80"
    depends_on:
      mysql:
        condition: service_healthy
    networks:
      - jo_network

volumes:
  mysql_data:
    driver: local

networks:
  jo_network:
    driver: bridge
```

---

### Étape 3 : Configurer appsettings.json

Modifiez votre `appsettings.json` :

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=jeuxolympiques;User=jo_user;Password=JO2024Password!;",
    "DockerConnection": "Server=mysql;Port=3306;Database=jeuxolympiques;User=jo_user;Password=JO2024Password!;"
  },
  "LoadTestData": true,
  "ForceReseed": false
}
```

**Note :** 
- `localhost` quand l'app tourne **hors Docker**
- `mysql` quand l'app tourne **dans Docker**

---

### Étape 4 : Démarrer la base de données

Ouvrez un terminal à la racine du projet :

```bash
# Démarrer les conteneurs
docker-compose up -d

# Vérifier que tout fonctionne
docker-compose ps
```

**Résultat attendu :**
```
NAME            IMAGE           STATUS          PORTS
jo_mysql_db     mysql:8.0       Up 30 seconds   0.0.0.0:3306->3306/tcp
jo_phpmyadmin   phpmyadmin      Up 30 seconds   0.0.0.0:8080->80/tcp
```

---

### Étape 5 : Vérifier la connexion

#### Via PHPMyAdmin (interface web)

1. Ouvrez votre navigateur : http://localhost:8080
2. Connectez-vous :
   - **Serveur** : `mysql`
   - **Utilisateur** : `jo_user`
   - **Mot de passe** : `JO2024Password!`
3. Vous devriez voir la base `jeuxolympiques`

#### Via ligne de commande

```bash
# Se connecter au conteneur MySQL
docker exec -it jo_mysql_db mysql -u jo_user -p
# Entrer le mot de passe : JO2024Password!

# Dans MySQL
SHOW DATABASES;
USE jeuxolympiques;
SHOW TABLES;
```

---

### Étape 6 : Appliquer les migrations Entity Framework

```bash
# Installer l'outil de migration (si pas déjà fait)
dotnet tool install --global dotnet-ef

# Créer la première migration
dotnet ef migrations add InitialCreate

# Appliquer les migrations
dotnet ef database update
```

**Résultat :** Les tables sont créées dans MySQL

---

### Étape 7 : Lancer votre application

```bash
dotnet run
```

**Ce qui se passe :**
```
Application des migrations...
Migrations appliquées
Chargement des données de test depuis seed-data.json...
Données de test chargées !
Application démarrée sur http://localhost:5000
```

---

### Étape 8 : Vérifier les données

#### Via PHPMyAdmin
1. http://localhost:8080
2. Base `jeuxolympiques` → Tables
3. Voir les données dans `Utilisateurs`, `Offres`, etc.

#### Via SQL
```sql
-- Dans le conteneur MySQL
docker exec -it jo_mysql_db mysql -u jo_user -pJO2024Password! jeuxolympiques

-- Requêtes
SELECT * FROM Utilisateurs;
SELECT * FROM Offres;
SELECT * FROM Billets;
```

---

### Commandes utiles Docker

```bash
# Démarrer
docker-compose up -d

# Arrêter
docker-compose down

# Arrêter ET supprimer les données
docker-compose down -v

# Voir les logs
docker-compose logs -f mysql

# Redémarrer
docker-compose restart

# Voir les conteneurs en cours
docker ps

# Supprimer tout et recommencer
docker-compose down -v
docker-compose up -d
dotnet ef database update
```

---

### Structure complète des fichiers

```
JeuxOlympiques/
│
├── docker-compose.yml        # Configuration Docker
├── init.sql                  # (optionnel) Script d'init
├── appsettings.json          # Configuration app
├── Program.cs
│
├── Data/
│   ├── seed-data.json        # Données de test
│   ├── ApplicationDbContext.cs
│   └── DbSeeder.cs
│
└── Migrations/               # Créé par EF
    └── xxxxx_InitialCreate.cs
```

---

### Dépannage Docker local

#### Problème : Port 3306 déjà utilisé

**Erreur :**
```
Error: Bind for 0.0.0.0:3306 failed: port is already allocated
```

**Solution 1 :** Arrêter MySQL local
```bash
# Windows
net stop MySQL80

# macOS/Linux
sudo service mysql stop
```

**Solution 2 :** Changer le port dans docker-compose.yml
```yaml
ports:
  - "3307:3306"  # Utiliser le port 3307
```

Puis dans `appsettings.json` :
```json
"DefaultConnection": "Server=localhost;Port=3307;..."
```

#### Problème : Données perdues après redémarrage

**Cause :** Vous avez fait `docker-compose down -v` (supprime les volumes)

**Solution :** Ne jamais utiliser `-v` sauf si vous voulez tout effacer
```bash
docker-compose down        # Garde les données
docker-compose down -v     # Supprime tout
```

#### Problème : Conteneur ne démarre pas

```bash
# Voir les logs détaillés
docker-compose logs mysql

# Supprimer et recréer
docker-compose down -v
docker-compose up -d
```

---

## OPTION 2 : Base de données Render PostgreSQL (PRODUCTION)

### Avantages
- Gratuit (plan Free)
- Hébergé dans le cloud
- Backups automatiques (plan payant)
- Accessible depuis n'importe où
- Pas de gestion d'infrastructure

### Inconvénients
- Nécessite une connexion internet
- Plan gratuit limité à 1 GB
- Spindown après 90 jours d'inactivité

---

### Étape 1 : Créer un compte Render

1. Allez sur https://render.com
2. Cliquez sur **"Get Started"**
3. Inscrivez-vous avec :
   - GitHub (recommandé)
   - GitLab
   - Email

---

### Étape 2 : Créer une base de données PostgreSQL

1. **Dans le Dashboard Render**, cliquez sur **"New +"**
2. Sélectionnez **"PostgreSQL"**

3. **Configuration :**
   ```
   Name: jo2024-database
   Database: jeuxolympiques
   User: (généré automatiquement)
   Region: Frankfurt (Europe) 
   PostgreSQL Version: 16
   Datadog API Key: (laisser vide)
   Instance Type: Free
   ```

4. Cliquez sur **"Create Database"**

5. **Attendez 2-3 minutes** que la base soit créée

---

### Étape 3 : Récupérer les informations de connexion

Une fois créée, vous voyez :

```
Status: Available

Connections:
┌─────────────────────────────────────────────────────────┐
│ Internal Database URL (à utiliser dans votre app)       │
│ postgresql://jo_user:xxxxx@dpg-xxxxx.frankfurt-postgres │
│ .render.com/jeuxolympiques                              │
│                                                         │
│ External Database URL (pour tests locaux)               │
│ postgresql://jo_user:xxxxx@dpg-xxxxx.frankfurt-postgres │
│ .render.com/jeuxolympiques                              │
│                                                         │
│ PSQL Command:                                           │
│ psql -h dpg-xxxxx.frankfurt-postgres.render.com \      │
│      -U jo_user jeuxolympiques                          │
└─────────────────────────────────────────────────────────┘
```

**IMPORTANT :** Copiez ces URLs, vous en aurez besoin !

---

### Étape 4 : Adapter votre projet pour PostgreSQL

#### 4.1 Installer le package NuGet PostgreSQL

```bash
# Supprimer MySQL
dotnet remove package Pomelo.EntityFrameworkCore.MySql

# Ajouter PostgreSQL
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

#### 4.2 Modifier Program.cs

**AVANT (MySQL) :**
```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    )
);
```

**APRÈS (PostgreSQL) :**
```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString)
);
```

#### 4.3 Modifier appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=dpg-xxxxx.frankfurt-postgres.render.com;Database=jeuxolympiques;Username=jo_user;Password=xxxxx;SSL Mode=Require"
  },
  "LoadTestData": true,
  "ForceReseed": false
}
```

Remplacez par votre **External Database URL** de Render

---

### Étape 5 : Créer et appliquer les migrations

```bash
# Supprimer les anciennes migrations MySQL
rm -rf Migrations/

# Créer une nouvelle migration pour PostgreSQL
dotnet ef migrations add InitialPostgreSQL

# Appliquer à la base Render
dotnet ef database update
```

**Vérification :** Les tables sont créées sur Render

---

### Étape 6 : Tester la connexion

```bash
dotnet run
```

**Résultat attendu :**
```
Application des migrations...
Migrations appliquées
Chargement des données de test...
Données chargées sur Render PostgreSQL !
Application démarrée
```

---

### Étape 7 : Vérifier les données sur Render

#### Via Render Dashboard

1. Allez dans votre base de données sur Render
2. Onglet **"Shell"**
3. Exécutez :
   ```sql
   \dt                           -- Lister les tables
   SELECT * FROM "Utilisateurs";  -- Voir les utilisateurs
   SELECT * FROM "Offres";        -- Voir les offres
   ```

#### Via client PostgreSQL local

```bash
# Installer psql (si pas installé)
# Windows : https://www.postgresql.org/download/windows/
# macOS : brew install postgresql
# Linux : sudo apt-get install postgresql-client

# Se connecter
psql -h dpg-xxxxx.frankfurt-postgres.render.com \
     -U jo_user \
     -d jeuxolympiques

# Requêtes
\dt
SELECT * FROM "Utilisateurs";
```

---

### Étape 8 : Configuration pour développement local + Render

Vous voulez utiliser **Docker en local** et **Render en production** ?

#### appsettings.Development.json (local)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=jeuxolympiques;User=jo_user;Password=JO2024Password!;"
  },
  "LoadTestData": true,
  "ForceReseed": false
}
```

#### appsettings.Production.json (Render)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "${DATABASE_URL}"
  },
  "LoadTestData": false,
  "ForceReseed": false
}
```

#### Dans Program.cs

```csharp
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Détecter le type de base de données
if (connectionString.Contains("postgres"))
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString)
    );
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseMySql(
            connectionString,
            ServerVersion.AutoDetect(connectionString)
        )
    );
}
```

---

## Workflow complet : Local → Production

### Développement local (Docker)

```bash
# 1. Démarrer Docker
docker-compose up -d

# 2. Développer avec MySQL local
dotnet run

# 3. Tester avec données de test
# Les données sont dans Docker MySQL
```

### Déploiement sur Render (PostgreSQL)

```bash
# 1. S'assurer que PostgreSQL est configuré
# (packages NuGet installés)

# 2. Créer les migrations pour PostgreSQL
dotnet ef migrations add DeployToRender

# 3. Pousser sur GitHub
git add .
git commit -m "Ready for production"
git push origin main

# 4. Render détecte le push et déploie automatiquement
# Les migrations s'appliquent automatiquement (Program.cs)
```

---

## Comparaison des deux approches

| Critère | Docker Local | Render PostgreSQL |
|---------|-------------|-------------------|
| **Coût** | Gratuit | Gratuit (1 GB) |
| **Internet** | Non requis | Requis |
| **Performance** | Très rapide | Dépend connexion |
| **Sauvegardes** | Manuelles | Automatiques (payant) |
| **Accès distant** | Non | Oui |
| **Développement** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ |
| **Production** | ❌ | ⭐⭐⭐⭐⭐ |
| **Réinitialisation** | Très facile | Facile |
| **Gestion** | Vous | Render |

---

## Recommandation finale

### Pour le développement : **Docker Local (MySQL)**
```bash
# Rapide, gratuit, hors ligne
docker-compose up -d
dotnet run
```

### Pour la production : **Render PostgreSQL**
```bash
# Hébergé, accessible, backups
# Configuration dans appsettings.Production.json
```

### Configuration idéale

```
┌─────────────────────────────────────────┐
│  DÉVELOPPEMENT LOCAL                     │
│  ├─ Docker MySQL                        │
│  ├─ seed-data.json (ForceReseed: true)  │
│  └─ Tests rapides                       │
└─────────────────────────────────────────┘
              │
              │ git push
              ▼
┌─────────────────────────────────────────┐
│  PRODUCTION (Render)                     │
│  ├─ PostgreSQL managé                   │
│  ├─ Données réelles                     │
│  └─ LoadTestData: false                 │
└─────────────────────────────────────────┘
```

---

## Aide rapide

### Je veux développer rapidement
```bash
docker-compose up -d
dotnet run
# Accéder à http://localhost:5000
```

### Je veux déployer en production
1. Créer base Render PostgreSQL
2. Configurer `appsettings.Production.json`
3. `git push origin main`
4. Render déploie automatiquement

### Je veux passer de MySQL à PostgreSQL
1. `dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL`
2. Modifier `Program.cs` (UseNpgsql)
3. `rm -rf Migrations/`
4. `dotnet ef migrations add PostgreSQLMigration`
5. `dotnet ef database update`

