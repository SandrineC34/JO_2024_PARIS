# Documentation Technique - Application JO 2024

## Table des Matières

1. [Vue d'Ensemble](#1-vue-densemble)
2. [Architecture](#2-architecture)
3. [Technologies Utilisées](#3-technologies-utilisées)
4. [Structure du Projet](#4-structure-du-projet)
5. [Base de Données](#5-base-de-données)
6. [Backend API](#6-backend-api)
7. [Frontend](#7-frontend)
8. [Containerisation Docker](#8-containerisation-docker)
9. [Déploiement](#9-déploiement)
10. [Sécurité](#10-sécurité)
11. [Tests et Monitoring](#11-tests-et-monitoring)
12. [Maintenance](#12-maintenance)

---

## 1. Vue d'Ensemble

### 1.1 Contexte du Projet

Application web de billetterie pour les Jeux Olympiques Paris 2024, permettant :
- La consultation des offres de billets
- L'achat de billets (Solo, Duo, Famille)
- La gestion des commandes
- La génération de QR codes pour l'accès aux épreuves
- L'administration des utilisateurs et des billets

### 1.2 Objectifs Techniques

- ✅ Architecture modulaire et maintenable (Clean Architecture)
- ✅ API RESTful sécurisée avec JWT
- ✅ Containerisation complète avec Docker
- ✅ Déploiement flexible (local et cloud)
- ✅ Initialisation automatique de la base de données
- ✅ Documentation interactive avec Swagger

### 1.3 Environnements

| Environnement | Description | URL |
|---------------|-------------|-----|
| **Développement** | Docker Desktop en local | http://localhost |
| **Production** | Render (Cloud) | https://jo2024.onrender.com |

---

## 2. Architecture

### 2.1 Architecture Globale

```
┌─────────────────────────────────────────────────────────────┐
│                         FRONTEND                             │
│                    (Nginx + HTML/CSS/JS)                     │
│                      Port 3000 (local)                       │
└────────────────────┬────────────────────────────────────────┘
                     │ HTTP/HTTPS
                     │ CORS configuré
                     ▼
┌─────────────────────────────────────────────────────────────┐
│                       BACKEND API                            │
│                    (.NET 8.0 Web API)                        │
│                      Port 5000 (local)                       │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │              Authentication Layer                     │  │
│  │                    (JWT Bearer)                       │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │                  Controllers                          │  │
│  │   Auth | Offres | Commandes | Billets | Admin       │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │                Business Services                      │  │
│  │   AuthService | OffreService | CommandeService       │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │                  Repositories                         │  │
│  │          (Entity Framework Core - ORM)               │  │
│  └──────────────────────────────────────────────────────┘  │
└────────────────────┬────────────────────────────────────────┘
                     │ MySQL Protocol
                     │ Connection String
                     ▼
┌─────────────────────────────────────────────────────────────┐
│                    BASE DE DONNÉES                           │
│                     MySQL 8.0                                │
│                   Port 3306 (local)                          │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Tables:                                              │  │
│  │  • Utilisateurs     • CommandeItems                  │  │
│  │  • Offres           • Billets                        │  │
│  │  • Commandes        • __EFMigrationsHistory          │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

### 2.2 Architecture Backend (Clean Architecture)

```
JO2024.API (Présentation)
    ↓ Dépend de
JO2024.Core (Domaine)
    ↑ Utilisé par
JO2024.Infrastructure (Données)
```

**Principe :** Les dépendances pointent vers l'intérieur (Core indépendant)

---

## 3. Technologies Utilisées

### 3.1 Backend

| Technologie                          | Version | Rôle |
|--------------------------------------|---------|------|
| **.NET**                             | 8.0 | Framework backend |
| **ASP.NET Core**                     | 8.0 | API Web RESTful |
| **Entity Framework Core**            | 8.0 | ORM (Mapping objet-relationnel) |
| **Pomelo.EntityFrameworkCore.MySql** | 8.0 | Provider MySQL pour EF Core |
| **BCrypt.Net-Next**                  | 4.0.3 | Hachage des mots de passe |
| **JWT Bearer** | - | Authentification par tokens |
| **Swashbuckle (Swagger)** | - | Documentation API interactive |
| **AutoMapper** | - | Mapping DTO ↔ Entités |

### 3.2 Frontend

| Technologie | Rôle |
|-------------|------|
| **HTML5** | Structure des pages |
| **CSS3** | Stylisation |
| **JavaScript (Vanilla)** | Interactions et appels API |
| **Nginx** | Serveur web de fichiers statiques |

### 3.3 Base de Données

| Technologie | Version | Rôle |
|-------------|---------|------|
| **MySQL** | 8.0 | SGBD relationnel |
| **phpMyAdmin** | Latest | Interface de gestion MySQL |

### 3.4 DevOps

| Technologie | Rôle |
|-------------|------|
| **Docker** | Containerisation |
| **Docker Compose** | Orchestration multi-containers |
| **GitLab CI/CD** | Intégration et déploiement continus |
| **GitLab Runner** | Exécuteur de pipelines |
| **Render** | Plateforme de déploiement cloud |

---

## 4. Structure du Projet

### 4.1 Arborescence Complète

```
JO2024_Project/
│
├── .env                                    # Variables d'environnement (NON versionné)
├── .env.example                            # Template des variables
├── .gitignore                              # Fichiers à exclure de Git
├── docker-compose.yml                      # Orchestration Docker
├── nginx.conf                              # Configuration Nginx
├── README.md                               # Documentation utilisateur
├── GUIDE_DEPLOIEMENT.md                    # Guide de déploiement
├── DEMARRAGE_RAPIDE.md                     # Quick start
├── COMMANDES_RAPIDES.sh                    # Script utilitaire
│
├── JO2024_Backend/                         # Solution .NET
│   │
│   ├── JO2024.API/                         # Couche Présentation
│   │   ├── Controllers/                    # Contrôleurs API
│   │   │   ├── AuthController.cs           # Authentification
│   │   │   ├── OffresController.cs         # Gestion des offres
│   │   │   ├── CommandesController.cs      # Gestion des commandes
│   │   │   ├── BilletsController.cs        # Gestion des billets
│   │   │   └── AdminController.cs          # Administration
│   │   │
│   │   ├── Middleware/                     # Middlewares personnalisés
│   │   │   └── ErrorHandlingMiddleware.cs  # Gestion centralisée des erreurs
│   │   │
│   │   ├── Dockerfile                      # Build de l'API
│   │   ├── Program.cs                      # Point d'entrée
│   │   ├── appsettings.json                # Configuration application
│   │   ├── appsettings.Development.json    # Config développement
│   │   └── JO2024.API.csproj               # Projet .NET
│   │
│   ├── JO2024.Core/                        # Couche Domaine (Business Logic)
│   │   ├── Entities/                       # Entités métier
│   │   │   ├── Utilisateur.cs              # Entité Utilisateur
│   │   │   ├── Offre.cs                    # Entité Offre
│   │   │   ├── Commande.cs                 # Entité Commande
│   │   │   ├── CommandeItem.cs             # Ligne de commande
│   │   │   └── Billet.cs                   # Entité Billet
│   │   │
│   │   ├── Interfaces/                     # Contrats (abstractions)
│   │   │   ├── IRepository.cs              # Repository générique
│   │   │   ├── IUtilisateurRepository.cs   # Repository utilisateur
│   │   │   ├── IOffreRepository.cs         # Repository offre
│   │   │   ├── ICommandeRepository.cs      # Repository commande
│   │   │   ├── IBilletRepository.cs        # Repository billet
│   │   │   ├── IAuthService.cs             # Service auth
│   │   │   ├── IOffreService.cs            # Service offre
│   │   │   ├── ICommandeService.cs         # Service commande
│   │   │   ├── IBilletService.cs           # Service billet
│   │   │   ├── IQRCodeService.cs           # Service QR code
│   │   │   └── IAdminService.cs            # Service admin
│   │   │
│   │   ├── Services/                       # Implémentations services
│   │   │   ├── AuthService.cs
│   │   │   ├── OffreService.cs
│   │   │   ├── CommandeService.cs
│   │   │   ├── BilletService.cs
│   │   │   ├── QRCodeService.cs
│   │   │   └── AdminService.cs
│   │   │
│   │   ├── DTOs/                           # Data Transfer Objects
│   │   │   ├── Auth/
│   │   │   ├── Offres/
│   │   │   ├── Commandes/
│   │   │   └── Billets/
│   │   │
│   │   └── JO2024.Core.csproj              # Projet Core
│   │
│   └── JO2024.Infrastructure/              # Couche Infrastructure (Données)
│       ├── Data/                           # Contexte et configuration DB
│       │   ├── ApplicationDbContext.cs     # DbContext EF Core
│       │   └── DbInitializer.cs            # Initialisation et seed
│       │
│       ├── Repositories/                   # Implémentations repositories
│       │   ├── Repository.cs               # Repository générique
│       │   ├── UtilisateurRepository.cs
│       │   ├── OffreRepository.cs
│       │   ├── CommandeRepository.cs
│       │   └── BilletRepository.cs
│       │
│       ├── Migrations/                     # Migrations EF Core
│       │   ├── XXXXXX_InitialCreate.cs     # Migration initiale
│       │   ├── XXXXXX_InitialCreate.Designer.cs
│       │   └── ApplicationDbContextModelSnapshot.cs
│       │
│       └── JO2024.Infrastructure.csproj    # Projet Infrastructure
│
├── frontend/                               # Application Frontend
│   ├── html/                               # Pages HTML
│   │   ├── index.html                      # Page d'accueil
│   │   ├── offres.html                     # Catalogue offres
│   │   ├── panier.html                     # Panier
│   │   ├── login.html                      # Connexion
│   │   ├── register.html                   # Inscription
│   │   ├── compte.html                     # Espace utilisateur
│   │   └── admin.html                      # Interface admin
│   │
│   ├── css/                                # Feuilles de style
│   │   ├── style.css                       # Style global
│   │   ├── offres.css                      # Style offres
│   │   └── admin.css                       # Style admin
│   │
│   ├── js/                                 # Scripts JavaScript
│   │   ├── api.js                          # Client API
│   │   ├── auth.js                         # Gestion auth
│   │   ├── offres.js                       # Logique offres
│   │   ├── panier.js                       # Logique panier
│   │   └── admin.js                        # Logique admin
│   │
│   └── assets/                             # Ressources statiques
│       ├── images/                         # Images
│       ├── icons/                          # Icônes
│       └── fonts/                          # Polices
│
└── gitlab-runner-config/                   # Configuration CI/CD
    └── config.toml                         # Config GitLab Runner
```

### 4.2 Responsabilités des Couches

#### JO2024.API (Présentation)
- ✅ Exposition des endpoints HTTP
- ✅ Validation des requêtes
- ✅ Gestion de l'authentification JWT
- ✅ Transformation des réponses (DTOs)
- ✅ Documentation Swagger

#### JO2024.Core (Domaine)
- ✅ Entités métier
- ✅ Logique métier (business rules)
- ✅ Interfaces (contrats)
- ✅ **Aucune dépendance** vers l'infrastructure

#### JO2024.Infrastructure (Données)
- ✅ Accès aux données (EF Core)
- ✅ Implémentation des repositories
- ✅ Configuration des entités
- ✅ Migrations de base de données

---

## 5. Base de Données

### 5.1 Modèle de Données (MCD)

```
┌──────────────────┐          ┌──────────────────┐
│   Utilisateur    │          │      Offre       │
├──────────────────┤          ├──────────────────┤
│ PK Id            │          │ PK Id            │
│    Prenom        │          │    Type          │
│    Nom           │          │    Nom           │
│    Email (UK)    │          │    Description   │
│    MotDePasseHash│          │    Prix          │
│    Role          │          │    NombrePersonnes│
│    DateCreation  │          │    Caracteristiques│
│    EstActif      │          │    EstActif      │
└────────┬─────────┘          └────────┬─────────┘
         │                             │
         │ 1                           │
         │                             │
         │ N                           │ N
         │        ┌──────────────┐     │
         └────────│   Commande   │─────┘
                  ├──────────────┤
                  │ PK Id        │
                  │ FK UtilisateurId
                  │    Numero (UK)
                  │    DateAchat │
                  │    MontantHT │
                  │    MontantTVA│
                  │    MontantTotal
                  │    Statut    │
                  │    MethodePaiement
                  └──────┬───────┘
                         │ 1
                         │
                         │ N
                  ┌──────┴───────┐
                  │ CommandeItem │
                  ├──────────────┤
                  │ PK Id        │
                  │ FK CommandeId│
                  │ FK OffreId   │
                  │    Quantite  │
                  │    PrixUnitaire
                  │    PrixTotal │
                  │    Sport     │
                  └──────────────┘

┌────────────────┐
│     Billet     │
├────────────────┤
│ PK Id          │
│ FK CommandeId  │
│ FK UtilisateurId
│    Numero (UK) │
│    Titre       │
│    Sport       │
│    Lieu        │
│    DateEpreuve │
│    Place       │
│    Statut      │
│    CodeQR      │
│    DateScan    │
│    DateCreation│
└────────────────┘
```

### 5.2 Tables et Colonnes

#### Table: `Utilisateurs`

| Colonne | Type | Contraintes | Description |
|---------|------|-------------|-------------|
| `Id` | INT | PK, AUTO_INCREMENT | Identifiant unique |
| `Prenom` | VARCHAR(100) | NOT NULL | Prénom de l'utilisateur |
| `Nom` | VARCHAR(100) | NOT NULL | Nom de l'utilisateur |
| `Email` | VARCHAR(255) | NOT NULL, UNIQUE | Email (login) |
| `MotDePasseHash` | TEXT | NOT NULL | Hash BCrypt du mot de passe |
| `Role` | VARCHAR(50) | NOT NULL, DEFAULT 'Utilisateur' | Rôle (Utilisateur/Admin) |
| `DateCreation` | DATETIME | DEFAULT CURRENT_TIMESTAMP | Date de création |
| `DerniereConnexion` | DATETIME | NULL | Date de dernière connexion |
| `EstActif` | BOOLEAN | DEFAULT TRUE | Compte actif/inactif |
| `CleSecurite` | VARCHAR(255) | NULL | Clé pour réinitialisation |
| `TokenReinitialisation` | VARCHAR(255) | NULL | Token de réinitialisation |
| `TokenReinitExpiration` | DATETIME | NULL | Expiration du token |

**Index :**
- `IX_Utilisateurs_Email` (UNIQUE)

#### Table: `Offres`

| Colonne | Type | Contraintes | Description |
|---------|------|-------------|-------------|
| `Id` | INT | PK, AUTO_INCREMENT | Identifiant unique |
| `Type` | VARCHAR(50) | NOT NULL | Type (solo/duo/famille) |
| `Nom` | VARCHAR(200) | NOT NULL | Nom de l'offre |
| `Description` | VARCHAR(1000) | NULL | Description détaillée |
| `Prix` | DECIMAL(10,2) | NOT NULL | Prix en euros |
| `NombrePersonnes` | INT | NOT NULL | Nombre de places |
| `Caracteristiques` | VARCHAR(500) | NULL | JSON des caractéristiques |
| `EstActif` | BOOLEAN | DEFAULT TRUE | Offre active/inactive |
| `DateCreation` | DATETIME | DEFAULT CURRENT_TIMESTAMP | Date de création |

#### Table: `Commandes`

| Colonne | Type | Contraintes | Description |
|---------|------|-------------|-------------|
| `Id` | INT | PK, AUTO_INCREMENT | Identifiant unique |
| `Numero` | VARCHAR(50) | NOT NULL, UNIQUE | Numéro de commande |
| `UtilisateurId` | INT | FK → Utilisateurs | Référence utilisateur |
| `DateAchat` | DATETIME | DEFAULT CURRENT_TIMESTAMP | Date d'achat |
| `MontantHT` | DECIMAL(10,2) | NOT NULL | Montant HT |
| `MontantTVA` | DECIMAL(10,2) | NOT NULL | Montant TVA |
| `MontantTotal` | DECIMAL(10,2) | NOT NULL | Montant TTC |
| `Statut` | VARCHAR(50) | DEFAULT 'Payée' | Statut commande |
| `MethodePaiement` | VARCHAR(100) | NULL | Mode de paiement |

**Index :**
- `IX_Commandes_Numero` (UNIQUE)
- `IX_Commandes_UtilisateurId`

#### Table: `CommandeItems`

| Colonne | Type | Contraintes | Description |
|---------|------|-------------|-------------|
| `Id` | INT | PK, AUTO_INCREMENT | Identifiant unique |
| `CommandeId` | INT | FK → Commandes | Référence commande |
| `OffreId` | INT | FK → Offres | Référence offre |
| `Quantite` | INT | NOT NULL | Quantité commandée |
| `PrixUnitaire` | DECIMAL(10,2) | NOT NULL | Prix unitaire |
| `PrixTotal` | DECIMAL(10,2) | NOT NULL | Prix total ligne |
| `Sport` | VARCHAR(100) | NULL | Sport sélectionné |

**Index :**
- `IX_CommandeItems_CommandeId`
- `IX_CommandeItems_OffreId`

#### Table: `Billets`

| Colonne | Type | Contraintes | Description |
|---------|------|-------------|-------------|
| `Id` | INT | PK, AUTO_INCREMENT | Identifiant unique |
| `Numero` | VARCHAR(50) | NOT NULL, UNIQUE | Numéro de billet |
| `CommandeId` | INT | FK → Commandes | Référence commande |
| `UtilisateurId` | INT | FK → Utilisateurs | Référence utilisateur |
| `Titre` | VARCHAR(200) | NOT NULL | Titre de l'épreuve |
| `Sport` | VARCHAR(100) | NOT NULL | Sport |
| `Lieu` | VARCHAR(200) | NOT NULL | Lieu de l'épreuve |
| `DateEpreuve` | DATETIME | NOT NULL | Date et heure |
| `Place` | VARCHAR(50) | NULL | Numéro de place |
| `Statut` | VARCHAR(50) | DEFAULT 'Actif' | Statut (Actif/Scanné/Annulé) |
| `CodeQR` | TEXT | NOT NULL | Code QR (base64 ou URL) |
| `DateScan` | DATETIME | NULL | Date de scan |
| `DateCreation` | DATETIME | DEFAULT CURRENT_TIMESTAMP | Date de création |

**Index :**
- `IX_Billets_Numero` (UNIQUE)
- `IX_Billets_CommandeId`
- `IX_Billets_UtilisateurId`

### 5.3 Données Initiales (Seed)

#### Offres Créées Automatiquement

```sql
-- Offre Solo
INSERT INTO Offres VALUES (1, 'solo', 'Offre Solo', 
  'Accès pour 1 personne à une épreuve olympique', 
  75.00, 1, '{"avantages":["1 billet","Accès standard"]}', 
  TRUE, NOW());

-- Offre Duo
INSERT INTO Offres VALUES (2, 'duo', 'Offre Duo', 
  'Accès pour 2 personnes - Économie de 20€', 
  130.00, 2, '{"avantages":["2 billets","Places côte à côte"]}', 
  TRUE, NOW());

-- Offre Famille
INSERT INTO Offres VALUES (3, 'famille', 'Offre Famille', 
  'Accès pour 4 personnes - Économie de 80€', 
  220.00, 4, '{"avantages":["4 billets","Places groupées"]}', 
  TRUE, NOW());
```

#### Utilisateurs Créés Automatiquement

```sql
-- Utilisateur Test
INSERT INTO Utilisateurs VALUES (1, 'Test', 'Utilisateur', 
  'test@jo2024.fr', '[BCrypt Hash]', 'Utilisateur', 
  NOW(), NULL, TRUE, NULL, NULL, NULL);

-- Administrateur
INSERT INTO Utilisateurs VALUES (2, 'Admin', 'JO2024', 
  'admin@jo2024.fr', '[BCrypt Hash]', 'Admin', 
  NOW(), NULL, TRUE, NULL, NULL, NULL);
```

**Mots de passe :**
- test@jo2024.fr : `Test@123`
- admin@jo2024.fr : `Admin@123`

### 5.4 Relations et Contraintes

```sql
-- Contraintes de clés étrangères
ALTER TABLE Commandes 
  ADD CONSTRAINT FK_Commandes_Utilisateurs 
  FOREIGN KEY (UtilisateurId) 
  REFERENCES Utilisateurs(Id) 
  ON DELETE RESTRICT;

ALTER TABLE CommandeItems 
  ADD CONSTRAINT FK_CommandeItems_Commandes 
  FOREIGN KEY (CommandeId) 
  REFERENCES Commandes(Id) 
  ON DELETE CASCADE;

ALTER TABLE CommandeItems 
  ADD CONSTRAINT FK_CommandeItems_Offres 
  FOREIGN KEY (OffreId) 
  REFERENCES Offres(Id) 
  ON DELETE RESTRICT;

ALTER TABLE Billets 
  ADD CONSTRAINT FK_Billets_Commandes 
  FOREIGN KEY (CommandeId) 
  REFERENCES Commandes(Id) 
  ON DELETE CASCADE;

ALTER TABLE Billets 
  ADD CONSTRAINT FK_Billets_Utilisateurs 
  FOREIGN KEY (UtilisateurId) 
  REFERENCES Utilisateurs(Id) 
  ON DELETE RESTRICT;
```

**Stratégies de suppression :**
- `CASCADE` : Suppression en cascade (items/billets supprimés si commande supprimée)
- `RESTRICT` : Empêche la suppression si des références existent

---

## 6. Backend API

### 6.1 Configuration Entity Framework Core

#### ApplicationDbContext.cs

```csharp
public class ApplicationDbContext : DbContext
{
    public DbSet<Utilisateur> Utilisateurs { get; set; }
    public DbSet<Offre> Offres { get; set; }
    public DbSet<Commande> Commandes { get; set; }
    public DbSet<CommandeItem> CommandeItems { get; set; }
    public DbSet<Billet> Billets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configuration des entités via Fluent API
        // - Index uniques
        // - Relations (FK)
        // - Contraintes
        // - Valeurs par défaut
    }
}
```

#### DbInitializer.cs

**Responsabilités :**
1. Attente de la disponibilité de MySQL (retry logic)
2. Application des migrations (`MigrateAsync()`)
3. Vérification des données existantes
4. Insertion des données initiales (offres, utilisateurs)

**Processus d'initialisation :**

```
Démarrage API
    ↓
Program.cs appelle DbInitializer.Initialize()
    ↓
Tentative de connexion MySQL (max 12 × 5s = 60s)
    ↓
Application des migrations EF Core
    ↓
Vérification si données existent (COUNT Offres)
    ↓
Si vide → Insertion seed data
    ↓
Logs de confirmation
    ↓
API prête
```

### 6.2 Endpoints API

#### Authentification (`/api/auth`)

| Méthode | Endpoint | Description | Auth |
|---------|----------|-------------|------|
| POST | `/register` | Inscription utilisateur | Non |
| POST | `/login` | Connexion et génération JWT | Non |
| POST | `/refresh` | Rafraîchir le token JWT | Non |
| POST | `/forgot-password` | Demande réinitialisation | Non |
| POST | `/reset-password` | Réinitialiser mot de passe | Non |

**Exemple Login :**

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "test@jo2024.fr",
  "password": "Test@123"
}

Response 200:
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "expiration": "2024-08-05T10:00:00Z",
  "user": {
    "id": 1,
    "prenom": "Test",
    "nom": "Utilisateur",
    "email": "test@jo2024.fr",
    "role": "Utilisateur"
  }
}
```

#### Offres (`/api/offres`)

| Méthode | Endpoint | Description | Auth |
|---------|----------|-------------|------|
| GET | `/` | Liste toutes les offres actives | Non |
| GET | `/{id}` | Détails d'une offre | Non |
| POST | `/` | Créer une offre | Admin |
| PUT | `/{id}` | Modifier une offre | Admin |
| DELETE | `/{id}` | Supprimer une offre | Admin |

**Exemple GET Offres :**

```http
GET /api/offres

Response 200:
[
  {
    "id": 1,
    "type": "solo",
    "nom": "Offre Solo",
    "description": "Accès pour 1 personne",
    "prix": 75.00,
    "nombrePersonnes": 1,
    "estActif": true
  },
  ...
]
```

#### Commandes (`/api/commandes`)

| Méthode | Endpoint | Description | Auth |
|---------|----------|-------------|------|
| GET | `/` | Liste des commandes utilisateur | JWT |
| GET | `/{id}` | Détails d'une commande | JWT |
| POST | `/` | Créer une commande | JWT |
| GET | `/{id}/pdf` | Télécharger PDF commande | JWT |

**Exemple POST Commande :**

```http
POST /api/commandes
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
Content-Type: application/json

{
  "items": [
    {
      "offreId": 1,
      "quantite": 2,
      "sport": "Athlétisme"
    }
  ],
  "methodePaiement": "Carte bancaire"
}

Response 201:
{
  "id": 1,
  "numero": "CMD-20240805-00001",
  "montantTotal": 150.00,
  "statut": "Payée",
  "items": [...],
  "billets": [...]
}
```

#### Billets (`/api/billets`)

| Méthode | Endpoint | Description | Auth |
|---------|----------|-------------|------|
| GET | `/` | Liste des billets utilisateur | JWT |
| GET | `/{id}` | Détails d'un billet | JWT |
| GET | `/{id}/qrcode` | Générer QR code | JWT |
| POST | `/{id}/scan` | Scanner un billet | Admin |
| POST | `/{id}/annuler` | Annuler un billet | Admin |

#### Administration (`/api/admin`)

| Méthode | Endpoint | Description | Auth |
|---------|----------|-------------|------|
| GET | `/utilisateurs` | Liste tous les utilisateurs | Admin |
| GET | `/commandes` | Liste toutes les commandes | Admin |
| GET | `/statistiques` | Statistiques globales | Admin |
| PUT | `/utilisateurs/{id}/activer` | Activer/désactiver compte | Admin |

### 6.3 Authentification JWT

#### Configuration (Program.cs)

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "JO2024.API",
        ValidAudience = "JO2024.Client",
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey)
        ),
        ClockSkew = TimeSpan.Zero
    };
});
```

#### Génération de Token (AuthService.cs)

```csharp
private string GenerateJwtToken(Utilisateur utilisateur)
{
    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, utilisateur.Id.ToString()),
        new Claim(ClaimTypes.Email, utilisateur.Email),
        new Claim(ClaimTypes.Name, $"{utilisateur.Prenom} {utilisateur.Nom}"),
        new Claim(ClaimTypes.Role, utilisateur.Role)
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    
    var token = new JwtSecurityToken(
        issuer: _jwtIssuer,
        audience: _jwtAudience,
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(_jwtExpirationMinutes),
        signingCredentials: creds
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

#### Utilisation dans les Contrôleurs

```csharp
[Authorize] // Nécessite un token valide
[ApiController]
[Route("api/[controller]")]
public class CommandesController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMesCommandes()
    {
        // Récupérer l'utilisateur depuis le token
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var commandes = await _commandeService.GetCommandesByUtilisateur(userId);
        return Ok(commandes);
    }

    [Authorize(Roles = "Admin")] // Nécessite rôle Admin
    [HttpGet("all")]
    public async Task<IActionResult> GetAllCommandes()
    {
        var commandes = await _commandeService.GetAllCommandes();
        return Ok(commandes);
    }
}
```

### 6.4 Gestion des Erreurs

#### ErrorHandlingMiddleware.cs

```csharp
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Une erreur s'est produite");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = exception switch
        {
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            ArgumentException => StatusCodes.Status400BadRequest,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = new
        {
            error = exception.Message,
            statusCode = statusCode
        };

        return context.Response.WriteAsJsonAsync(response);
    }
}
```

### 6.5 Services Métier

#### Structure d'un Service

```csharp
public class CommandeService : ICommandeService
{
    private readonly ICommandeRepository _commandeRepository;
    private readonly IOffreRepository _offreRepository;
    private readonly IBilletService _billetService;
    private readonly ILogger<CommandeService> _logger;

    public CommandeService(
        ICommandeRepository commandeRepository,
        IOffreRepository offreRepository,
        IBilletService billetService,
        ILogger<CommandeService> logger)
    {
        _commandeRepository = commandeRepository;
        _offreRepository = offreRepository;
        _billetService = billetService;
        _logger = logger;
    }

    public async Task<Commande> CreerCommande(CreateCommandeDto dto, int utilisateurId)
    {
        // Logique métier
        // 1. Validation des données
        // 2. Calcul des montants
        // 3. Création de la commande
        // 4. Génération des billets
        // 5. Persistance en base
    }
}
```

### 6.6 Sécurité des Mots de Passe

#### Hachage avec BCrypt

```csharp
// Enregistrement
public async Task<Utilisateur> Register(RegisterDto dto)
{
    // Vérifier si l'email existe déjà
    var existingUser = await _utilisateurRepository.GetByEmail(dto.Email);
    if (existingUser != null)
        throw new ArgumentException("Cet email est déjà utilisé");

    // Hacher le mot de passe
    var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

    var utilisateur = new Utilisateur
    {
        Prenom = dto.Prenom,
        Nom = dto.Nom,
        Email = dto.Email,
        MotDePasseHash = passwordHash,
        Role = "Utilisateur",
        DateCreation = DateTime.UtcNow,
        EstActif = true
    };

    await _utilisateurRepository.Add(utilisateur);
    return utilisateur;
}

// Connexion
public async Task<LoginResponseDto> Login(LoginDto dto)
{
    var utilisateur = await _utilisateurRepository.GetByEmail(dto.Email);
    if (utilisateur == null)
        throw new UnauthorizedAccessException("Email ou mot de passe incorrect");

    // Vérifier le mot de passe
    if (!BCrypt.Net.BCrypt.Verify(dto.Password, utilisateur.MotDePasseHash))
        throw new UnauthorizedAccessException("Email ou mot de passe incorrect");

    if (!utilisateur.EstActif)
        throw new UnauthorizedAccessException("Compte désactivé");

    // Mettre à jour la dernière connexion
    utilisateur.DerniereConnexion = DateTime.UtcNow;
    await _utilisateurRepository.Update(utilisateur);

    // Générer le token JWT
    var token = GenerateJwtToken(utilisateur);

    return new LoginResponseDto
    {
        Token = token,
        Expiration = DateTime.UtcNow.AddMinutes(_jwtExpirationMinutes),
        User = MapToUserDto(utilisateur)
    };
}
```

---

## 7. Frontend

### 7.1 Architecture Frontend

```
frontend/
├── html/          # Pages HTML statiques
├── css/           # Styles CSS
├── js/            # Logique JavaScript
└── assets/        # Ressources (images, fonts)
```

### 7.2 Communication avec l'API

#### Configuration API Client (api.js)

```javascript
const API_BASE_URL = 'http://localhost:5000/api';

class ApiClient {
    constructor() {
        this.baseUrl = API_BASE_URL;
    }

    getAuthHeaders() {
        const token = localStorage.getItem('jwt_token');
        return {
            'Content-Type': 'application/json',
            ...(token && { 'Authorization': `Bearer ${token}` })
        };
    }

    async request(endpoint, options = {}) {
        const url = `${this.baseUrl}${endpoint}`;
        const config = {
            ...options,
            headers: {
                ...this.getAuthHeaders(),
                ...options.headers
            }
        };

        try {
            const response = await fetch(url, config);
            
            if (response.status === 401) {
                // Token expiré
                localStorage.removeItem('jwt_token');
                window.location.href = '/login.html';
                return;
            }

            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.message || 'Erreur API');
            }

            return await response.json();
        } catch (error) {
            console.error('Erreur API:', error);
            throw error;
        }
    }

    // Méthodes spécifiques
    async getOffres() {
        return this.request('/offres');
    }

    async login(email, password) {
        const data = await this.request('/auth/login', {
            method: 'POST',
            body: JSON.stringify({ email, password })
        });
        
        // Sauvegarder le token
        localStorage.setItem('jwt_token', data.token);
        localStorage.setItem('user', JSON.stringify(data.user));
        
        return data;
    }

    async getMesCommandes() {
        return this.request('/commandes');
    }

    async creerCommande(commandeData) {
        return this.request('/commandes', {
            method: 'POST',
            body: JSON.stringify(commandeData)
        });
    }
}

const api = new ApiClient();
```

### 7.3 Gestion de l'Authentification (auth.js)

```javascript
class AuthManager {
    constructor() {
        this.checkAuth();
    }

    isAuthenticated() {
        const token = localStorage.getItem('jwt_token');
        if (!token) return false;

        // Vérifier si le token est expiré
        try {
            const payload = JSON.parse(atob(token.split('.')[1]));
            const exp = payload.exp * 1000; // Convertir en millisecondes
            return Date.now() < exp;
        } catch (e) {
            return false;
        }
    }

    getCurrentUser() {
        const userStr = localStorage.getItem('user');
        return userStr ? JSON.parse(userStr) : null;
    }

    isAdmin() {
        const user = this.getCurrentUser();
        return user && user.role === 'Admin';
    }

    logout() {
        localStorage.removeItem('jwt_token');
        localStorage.removeItem('user');
        window.location.href = '/index.html';
    }

    checkAuth() {
        // Protéger les pages nécessitant l'authentification
        const protectedPages = ['/compte.html', '/admin.html'];
        const currentPage = window.location.pathname;

        if (protectedPages.some(page => currentPage.includes(page))) {
            if (!this.isAuthenticated()) {
                window.location.href = '/login.html';
            }
        }

        // Protéger les pages admin
        if (currentPage.includes('/admin.html') && !this.isAdmin()) {
            window.location.href = '/index.html';
        }
    }
}

const auth = new AuthManager();
```

### 7.4 Gestion du Panier

```javascript
class PanierManager {
    constructor() {
        this.panier = this.loadPanier();
    }

    loadPanier() {
        const panierStr = localStorage.getItem('panier');
        return panierStr ? JSON.parse(panierStr) : [];
    }

    savePanier() {
        localStorage.setItem('panier', JSON.stringify(this.panier));
        this.updateBadge();
    }

    ajouterOffre(offre, quantite, sport) {
        const item = {
            id: Date.now(),
            offreId: offre.id,
            offreNom: offre.nom,
            prix: offre.prix,
            quantite: quantite,
            sport: sport,
            nombrePersonnes: offre.nombrePersonnes
        };

        this.panier.push(item);
        this.savePanier();
    }

    supprimerItem(itemId) {
        this.panier = this.panier.filter(item => item.id !== itemId);
        this.savePanier();
    }

    viderPanier() {
        this.panier = [];
        this.savePanier();
    }

    getTotal() {
        return this.panier.reduce((total, item) => 
            total + (item.prix * item.quantite), 0
        );
    }

    updateBadge() {
        const badge = document.querySelector('.panier-badge');
        if (badge) {
            const count = this.panier.length;
            badge.textContent = count;
            badge.style.display = count > 0 ? 'block' : 'none';
        }
    }

    async validerCommande() {
        if (!auth.isAuthenticated()) {
            window.location.href = '/login.html?redirect=panier';
            return;
        }

        const commandeData = {
            items: this.panier.map(item => ({
                offreId: item.offreId,
                quantite: item.quantite,
                sport: item.sport
            })),
            methodePaiement: 'Carte bancaire'
        };

        try {
            const commande = await api.creerCommande(commandeData);
            this.viderPanier();
            window.location.href = `/confirmation.html?commande=${commande.id}`;
        } catch (error) {
            alert('Erreur lors de la validation : ' + error.message);
        }
    }
}

const panier = new PanierManager();
```

### 7.5 Configuration CORS

Pour permettre au frontend d'appeler l'API depuis un domaine différent :

#### Backend (Program.cs)

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:80",
                "https://jo2024-frontend.onrender.com" // Production
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Dans le pipeline HTTP
app.UseCors("AllowFrontend");
```

---

## 8. Containerisation Docker

### 8.1 Architecture Docker

```
┌─────────────────────────────────────────────────────┐
│              Docker Network: jo2024_network          │
│                                                      │
│  ┌─────────────┐  ┌─────────────┐  ┌────────────┐  │
│  │   MySQL     │  │     API     │  │  Frontend  │  │
│  │   (3306)    │←─│   (5000)    │←─│   (3000)   │  │
│  └─────────────┘  └─────────────┘  └────────────┘  │
│         ↑                                            │
│         │                                            │
│  ┌─────────────┐                                    │
│  │ phpMyAdmin  │                                    │
│  │   (8080)    │                                    │
│  └─────────────┘                                    │
└─────────────────────────────────────────────────────┘
```

### 8.2 Dockerfile Backend

**Emplacement :** `JO2024_Backend/JO2024.API/Dockerfile`

```dockerfile
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copier les fichiers de projet et restaurer
COPY ["JO2024.API/JO2024.API.csproj", "JO2024.API/"]
COPY ["JO2024.Core/JO2024.Core.csproj", "JO2024.Core/"]
COPY ["JO2024.Infrastructure/JO2024.Infrastructure.csproj", "JO2024.Infrastructure/"]

RUN dotnet restore "JO2024.API/JO2024.API.csproj"

# Copier le code source
COPY . .

# Build
WORKDIR "/src/JO2024.API"
RUN dotnet build "JO2024.API.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "JO2024.API.csproj" -c Release -o /app/publish

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Installer curl pour healthcheck
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Créer dossier logs
RUN mkdir -p /app/logs

EXPOSE 80

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "JO2024.API.dll"]
```

**Optimisations :**
- ✅ Multi-stage build (réduction de la taille)
- ✅ Layer caching (restauration séparée)
- ✅ Image runtime légère (aspnet vs sdk)

### 8.3 docker-compose.yml

**Emplacement :** Racine du projet

```yaml
version: '3.8'

services:
  # Base de données MySQL
  mysql:
    image: mysql:8.0
    container_name: jo2024_mysql
    restart: always
    environment:
      MYSQL_ROOT_PASSWORD: ${MYSQL_ROOT_PASSWORD}
      MYSQL_DATABASE: ${MYSQL_DATABASE}
      MYSQL_USER: ${MYSQL_USER}
      MYSQL_PASSWORD: ${MYSQL_PASSWORD}
    ports:
      - "3306:3306"
    volumes:
      - mysql_data:/var/lib/mysql
    networks:
      - jo2024_network
    healthcheck:
      test: ["CMD", "mysqladmin", "ping", "-h", "localhost"]
      timeout: 20s
      retries: 10
      interval: 10s
      start_period: 40s
    command: --default-authentication-plugin=mysql_native_password

  # Interface phpMyAdmin
  phpmyadmin:
    image: phpmyadmin:latest
    container_name: jo2024_phpmyadmin
    restart: always
    environment:
      PMA_HOST: mysql
      PMA_PORT: 3306
    ports:
      - "8080:80"
    depends_on:
      mysql:
        condition: service_healthy
    networks:
      - jo2024_network

  # Backend API .NET
  api:
    build:
      context: ./JO2024_Backend
      dockerfile: JO2024.API/Dockerfile
    container_name: jo2024_api
    restart: always
    environment:
      - ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT}
      - ASPNETCORE_URLS=http://+:80
      - ConnectionStrings__DefaultConnection=Server=mysql;Port=3306;Database=${MYSQL_DATABASE};User=${MYSQL_USER};Password=${MYSQL_PASSWORD};
      - Jwt__Key=${JWT_KEY}
      - Jwt__Issuer=JO2024.API
      - Jwt__Audience=JO2024.Client
    ports:
      - "5000:80"
    depends_on:
      mysql:
        condition: service_healthy
    networks:
      - jo2024_network
    volumes:
      - ./JO2024_Backend/logs:/app/logs
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:80/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 60s

  # Frontend Nginx
  frontend:
    image: nginx:alpine
    container_name: jo2024_frontend
    restart: always
    ports:
      - "3000:80"
    volumes:
      - ./frontend/html:/usr/share/nginx/html:ro
      - ./frontend/css:/usr/share/nginx/html/css:ro
      - ./frontend/js:/usr/share/nginx/html/js:ro
      - ./frontend/assets:/usr/share/nginx/html/assets:ro
      - ./nginx.conf:/etc/nginx/nginx.conf:ro
    depends_on:
      - api
    networks:
      - jo2024_network

volumes:
  mysql_data:
    driver: local

networks:
  jo2024_network:
    driver: bridge
```

### 8.4 Variables d'Environnement

**Fichier `.env` :**

```bash
# Base de données
MYSQL_ROOT_PASSWORD=RootPassword123!
MYSQL_DATABASE=jo2024_db
MYSQL_USER=jo2024_user
MYSQL_PASSWORD=JO2024Pass123!

# JWT
JWT_KEY=VotreCleSecreteTresLongueEtSecurisee123456789ABCDEF

# Environnement
ASPNETCORE_ENVIRONMENT=Development
```

**⚠️ Sécurité :**
- Fichier `.env` dans `.gitignore`
- Fournir `.env.example` comme template
- Mots de passe forts en production

### 8.5 Healthchecks

#### MySQL Healthcheck

```yaml
healthcheck:
  test: ["CMD", "mysqladmin", "ping", "-h", "localhost"]
  timeout: 20s
  retries: 10
  interval: 10s
  start_period: 40s
```

**Signification :**
- `start_period: 40s` : Temps d'initialisation avant les checks
- `interval: 10s` : Vérification toutes les 10 secondes
- `retries: 10` : 10 tentatives avant de considérer le service down

#### API Healthcheck

```yaml
healthcheck:
  test: ["CMD", "curl", "-f", "http://localhost:80/health"]
  interval: 30s
  timeout: 10s
  retries: 3
  start_period: 60s
```

**Endpoint `/health` :**

```csharp
app.MapGet("/health", async (ApplicationDbContext context) => 
{
    try
    {
        var canConnect = await context.Database.CanConnectAsync();
        var offresCount = await context.Offres.CountAsync();
        
        return Results.Ok(new 
        { 
            status = "healthy",
            database = canConnect ? "connected" : "disconnected",
            offres = offresCount,
            timestamp = DateTime.UtcNow 
        });
    }
    catch (Exception ex)
    {
        return Results.Json(new 
        { 
            status = "unhealthy",
            error = ex.Message 
        }, statusCode: 503);
    }
});
```

### 8.6 Ordre de Démarrage

```
1. MySQL démarre (healthcheck: 40s)
   ↓
2. phpMyAdmin attend MySQL healthy
   ↓
3. API attend MySQL healthy (retry logic dans DbInitializer)
   ↓
4. API applique migrations + seed
   ↓
5. Frontend démarre (attend API)
   ↓
6. Tout est opérationnel
```

---

## 9. Déploiement

### 9.1 Déploiement Local (Développement)

#### Prérequis
- Docker Desktop installé et lancé
- .NET 8.0 SDK (pour créer les migrations)
- Git

#### Étapes

**1. Cloner le projet**

```bash
git clone https://gitlab.com/votre-projet/jo2024.git
cd jo2024
```

**2. Configurer l'environnement**

```bash
cp .env.example .env
# Éditer .env avec vos valeurs
```

**3. Créer la migration initiale**

```bash
cd JO2024_Backend/JO2024.API
dotnet ef migrations add InitialCreate \
    --project ../JO2024.Infrastructure \
    --startup-project .
cd ../..
```

**4. Démarrer les containers**

```bash
docker-compose up -d --build
```

**5. Vérifier le déploiement**

```bash
# Attendre 30 secondes
sleep 30

# Tester l'API
curl http://localhost:5000/health

# Logs
docker-compose logs -f api
```

**6. Accéder aux services**

- Frontend: http://localhost:3000
- API: http://localhost:5000
- Swagger: http://localhost:5000/swagger
- phpMyAdmin: http://localhost:8080

### 9.2 Déploiement sur Render (Production)

#### Architecture Production

```
┌───────────────────────────────────────────────┐
│           Render Cloud Platform                │
│                                               │
│  ┌─────────────────┐    ┌──────────────────┐ │
│  │  Static Site    │    │   Web Service    │ │
│  │   (Frontend)    │───▶│   (Backend API)  │ │
│  │                 │    │   Docker         │ │
│  └─────────────────┘    └────────┬─────────┘ │
│                                   │           │
│                           ┌───────▼────────┐  │
│                           │  MySQL Database│  │
│                           │   (Managed)    │  │
│                           └────────────────┘  │
└───────────────────────────────────────────────┘
```

#### Étape 1: Base de Données MySQL

1. Dans Render Dashboard: **New** → **MySQL**
2. Configuration:
   - **Name**: `jo2024-mysql`
   - **Database**: `jo2024_db`
   - **User**: Auto-généré
   - **Region**: Frankfurt (EU) ou Oregon (US)

3. Noter les informations de connexion:
   - Internal Database URL
   - Host
   - Port
   - Database
   - Username
   - Password

#### Étape 2: Backend API

1. **New** → **Web Service**
2. Connecter le repository GitLab
3. Configuration:
   - **Name**: `jo2024-api`
   - **Environment**: Docker
   - **Dockerfile Path**: `JO2024_Backend/JO2024.API/Dockerfile`
   - **Instance Type**: Free (ou Standard pour production)

4. **Variables d'environnement**:

```bash
ASPNETCORE_ENVIRONMENT=Production

ConnectionStrings__DefaultConnection=Server=MYSQL_HOST;Port=3306;Database=jo2024_db;User=MYSQL_USER;Password=MYSQL_PASSWORD;SslMode=Required;

Jwt__Key=VotreCleSecreteProductionMinimum32Caracteres123456789ABCDEF
Jwt__Issuer=JO2024.API
Jwt__Audience=JO2024.Client
Jwt__ExpirationMinutes=1440

Cors__AllowedOrigins__0=https://jo2024-frontend.onrender.com
```

5. **Health Check Path**: `/health`

#### Étape 3: Frontend

1. **New** → **Static Site**
2. Connecter le repository
3. Configuration:
   - **Name**: `jo2024-frontend`
   - **Build Command**: (laisser vide)
   - **Publish Directory**: `frontend/html`

4. Mettre à jour `frontend/js/api.js`:

```javascript
const API_BASE_URL = 'https://jo2024-api.onrender.com/api';
```

#### Étape 4: Déploiement Automatique

Créer `.gitlab-ci.yml` à la racine:

```yaml
stages:
  - build
  - deploy

variables:
  DOCKER_DRIVER: overlay2

build:
  stage: build
  image: mcr.microsoft.com/dotnet/sdk:8.0
  script:
    - cd JO2024_Backend
    - dotnet restore
    - dotnet build --configuration Release
  only:
    - main
    - develop

deploy:
  stage: deploy
  script:
    - echo "Déploiement sur Render via webhook"
    - curl -X POST $RENDER_DEPLOY_HOOK
  only:
    - main
```

### 9.3 Vérifications Post-Déploiement

#### Checklist Production

```bash
# 1. Tester l'API
curl https://jo2024-api.onrender.com/health

# Réponse attendue:
# {
#   "status": "healthy",
#   "database": "connected",
#   "offres": 3
# }

# 2. Tester une requête authentifiée
curl -H "Authorization: Bearer TOKEN" \
     https://jo2024-api.onrender.com/api/offres

# 3. Vérifier les logs Render
# Dashboard → Service → Logs

# 4. Tester le frontend
# https://jo2024-frontend.onrender.com
```

#### Problèmes Courants

**Problème 1: API lente au premier appel**

**Cause :** Free tier Render met l'API en veille après 15 min d'inactivité

**Solution :** 
- Utiliser un service de ping (UptimeRobot)
- Ou passer à un plan payant

**Problème 2: CORS errors**

**Cause :** URL frontend pas dans la whitelist CORS

**Solution :**
```bash
#