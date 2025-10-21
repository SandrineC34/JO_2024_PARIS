# 🚀 Guide de déploiement sur Render

## Vue d'ensemble de l'architecture

```
┌─────────────────────────────────────────────────────────┐
│                    RENDER.COM                           │
├─────────────────────────────────────────────────────────┤
│                                                         │
│  ┌──────────────────────┐      ┌──────────────────┐   │
│  │   Web Service        │      │   PostgreSQL     │   │
│  │   (ASP.NET Core)     │◄────►│   Database       │   │
│  │   Docker Container   │      │   (Managed)      │   │
│  └──────────────────────┘      └──────────────────┘   │
│           │                                             │
│           │ HTTPS                                       │
│           ▼                                             │
│  https://jo2024.onrender.com                           │
└─────────────────────────────────────────────────────────┘
```

## Option 1 : Déploiement avec PostgreSQL (RECOMMANDÉ)

### Étape 1 : Créer une base de données PostgreSQL sur Render

1. **Connexion à Render**
   - Allez sur [render.com](https://render.com)
   - Créez un compte ou connectez-vous

2. **Créer la base de données**
   - Cliquez sur "New +" → "PostgreSQL"
   - Configuration :
     ```
     Name: jo2024-db
     Database: jeuxolympiques
     User: jo_user (automatique)
     Region: Frankfurt (Europe) ou Paris
     Instance Type: Free
     ```
   - Cliquez sur "Create Database"

3. **Récupérer les informations de connexion**
   - Une fois créée, notez :
     - **Internal Database URL** (pour l'app)
     - **External Database URL** (pour tests locaux)
   
   Exemple :
   ```
   postgresql://jo_user:mdp@dpg-xxxxx-a.frankfurt-postgres.render.com/jeuxolympiques
   ```

### Étape 2 : Adapter le code pour PostgreSQL

Modifiez votre `JO2024.api s.csproj` :

```xml
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
<!-- Remplacer ou ajouter à Pomelo.EntityFrameworkCore.MySql -->
```

Modifiez `Program.cs` :

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString)  // Au lieu de UseMySql
);
```

### Étape 3 : Créer le Web Service sur Render

1. **Nouveau Web Service**
   - Cliquez sur "New +" → "Web Service"
   - Connectez votre repository GitHub/GitLab

2. **Configuration du service**
   ```
   Name: jo2024-app
   Region: Frankfurt (même région que la DB)
   Branch: main
   Root Directory: (vide si projet à la racine)
   Environment: Docker
   
   Dockerfile Path: ./Dockerfile
   
   Instance Type: Free (ou Starter $7/mois)
   ```

3. **Variables d'environnement**
   
   Dans l'onglet "Environment" :
   
   ```
   ASPNETCORE_ENVIRONMENT=Production
   ASPNETCORE_URLS=http://+:80
   ConnectionStrings__DefaultConnection=<INTERNAL_DATABASE_URL>
   ```
   
   ⚠️ Remplacez `<INTERNAL_DATABASE_URL>` par l'URL interne de votre DB PostgreSQL

4. **Déployer**
   - Cliquez sur "Create Web Service"
   - Render va automatiquement :
     - Cloner votre repo
     - Construire le Docker
     - Déployer l'application
     - Générer une URL HTTPS

## Option 2 : Déploiement avec MySQL externe

### Utiliser PlanetScale (MySQL gratuit)

1. **Créer une base sur PlanetScale**
   - Allez sur [planetscale.com](https://planetscale.com)
   - Créez une base gratuite
   - Récupérez la connexion string

2. **Configuration Render identique**
   - Mais utilisez la connexion MySQL de PlanetScale

## Configuration du Dockerfile optimisé pour Render

Créez un fichier `.dockerignore` :

```
bin/
obj/
*.user
*.vs/
.git/
.gitignore
node_modules/
wwwroot/uploads/
```

## Structure des variables d'environnement complètes

Sur Render, dans "Environment" :

```bash
# Base de données
ConnectionStrings__DefaultConnection=postgresql://user:pass@host/db

# ASP.NET
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:80

# Sécurité (générer des vraies clés en production)
AppSettings__JwtSecret=VotreCleSecreteSuperLonguePourProduction123!
AppSettings__TokenExpirationHours=24

# Email (si vous configurez l'envoi d'emails)
EmailSettings__SmtpServer=smtp.gmail.com
EmailSettings__SmtpPort=587
EmailSettings__SenderEmail=noreply@jo2024.fr
EmailSettings__Username=votre-email@gmail.com
EmailSettings__Password=votre-app-password

# Logs
Logging__LogLevel__Default=Information
```

## Commandes utiles pour les migrations

### En local (avant le déploiement)

```bash
# Installer l'outil de migration
dotnet tool install --global dotnet-ef

# Créer une migration
dotnet ef migrations add InitialCreate

# Appliquer les migrations localement
dotnet ef database update

# Générer un script SQL
dotnet ef migrations script -o migrations.sql
```

### Sur Render (automatique au démarrage)

Le code dans `Program.cs` applique automatiquement les migrations :

```csharp
context.Database.Migrate();
```

## Commandes Docker locales pour tester

```bash
# Construire l'image
docker build -t jo2024-app .

# Lancer avec docker-compose
docker-compose up

# Accéder à l'application
http://localhost:5000
```

## Tests après déploiement

### 1. Vérifier la santé de l'application

```bash
# Test de l'API
curl https://votre-app.onrender.com/api/health

# Test de la base de données
curl https://votre-app.onrender.com/api/offres
```

### 2. Consulter les logs

Sur Render :
- Allez dans votre service
- Onglet "Logs"
- Surveillez les erreurs

### 3. Tests fonctionnels

1. ✅ Page d'accueil accessible
2. ✅ Inscription utilisateur
3. ✅ Connexion
4. ✅ Affichage des offres
5. ✅ Ajout au panier
6. ✅ Passage de commande
7. ✅ Génération de QR code
8. ✅ Mon compte - billets visibles

## Monitoring et performance

### Free Tier Render - Limitations

⚠️ **IMPORTANT** : Le plan gratuit Render a des limitations :

- **Spindown** : L'app s'arrête après 15 min d'inactivité
  - Premier accès = 30-60 secondes de démarrage
  - Solution : Plan Starter ($7/mois) = pas de spindown

- **Bande passante** : 100 GB/mois
- **Build minutes** : 500 minutes/mois

### Solutions pour le Free Tier

1. **Prévenir le spindown** (usage léger)
   - Utiliser un service comme UptimeRobot pour ping toutes les 14 min
   - ⚠️ Consomme de la bande passante

2. **Optimiser les builds**
   - Utiliser le cache Docker
   - Minimiser les dépendances

## Sécurité en production

### Checklist de sécurité

- [ ] HTTPS activé (automatique sur Render)
- [ ] Variables d'environnement pour secrets
- [ ] Pas de clés hardcodées dans le code
- [ ] Rate limiting sur les API
- [ ] Validation des entrées utilisateur
- [ ] CORS configuré correctement
- [ ] Hachage des mots de passe (BCrypt)
- [ ] Tokens JWT sécurisés
- [ ] Logs de sécurité activés

### Configuration CORS pour production

Dans `Program.cs` :

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins("https://votre-app.onrender.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
```

## Mise à jour de l'application

### Déploiement continu

1. **Push sur GitHub**
   ```bash
   git add .
   git commit -m "Nouvelle fonctionnalité"
   git push origin main
   ```

2. **Auto-déploiement**
   - Render détecte automatiquement
   - Rebuild et redéploie
   - Durée : 2-5 minutes

### Rollback en cas de problème

1. Sur Render → Votre service
2. Onglet "Events"
3. Cliquez sur un déploiement précédent
4. "Redeploy"

## Backup de la base de données

### PostgreSQL sur Render

```bash
# Exporter la base (avec pg_dump)
pg_dump postgresql://user:pass@host/db > backup.sql

# Restaurer
psql postgresql://user:pass@host/db < backup.sql
```

### Automatisation des backups

Render Free : backups manuels uniquement
Render Starter : backups automatiques quotidiens

## Coûts estimés

| Service | Free | Starter | Pro |
|---------|------|---------|-----|
| Web Service | 0€ (avec spindown) | 7€/mois | 25€/mois |
| PostgreSQL | 0€ (1GB) | 7€/mois (10GB) | 20€/mois (100GB) |
| **TOTAL** | **0€** | **14€/mois** | **45€/mois** |

## Alternatives à Render

Si vous voulez comparer :

| Plateforme | Avantages | Inconvénients |
|------------|-----------|---------------|
| **Render** | Simple, free tier généreux | Spindown gratuit |
| **Railway** | Excellent DX, $5 gratuit/mois | Puis payant |
| **Fly.io** | Excellente perf, edge computing | Config complexe |
| **Azure** | Crédit étudiant 100€ | Complexe |
| **Heroku** | Très simple | Plus de free tier |

## Ressources

- [Documentation Render](https://render.com/docs)
- [Render Community](https://community.render.com)
- [ASP.NET Core Deployment](https://learn.microsoft.com/aspnet/core/host-and-deploy/)

## Support

En cas de problème :

1. Consultez les logs Render
2. Vérifiez les variables d'environnement
3. Testez en local avec Docker
4. Consultez le forum Render
5. Contactez le support Render (très réactif)

---

✅ **Votre application est maintenant déployée et accessible mondialement !**

🌐 URL : `https://votre-app.onrender.com`