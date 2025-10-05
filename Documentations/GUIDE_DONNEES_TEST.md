# Guide complet - Utilisation des données de test

### Avantages

✅ **Rapidité** : Pas besoin de créer manuellement des comptes et commandes
✅ **Reproductibilité** : Mêmes données à chaque test
✅ **Isolation** : Pas de pollution de la base de production
✅ **Versionning** : Le fichier JSON est versionné avec Git
✅ **Collaboration** : Toute l'équipe travaille avec les mêmes données
✅ **Débogage** : Données prévisibles pour identifier les bugs

### Cas d'usage

- **Développement local** : Tester rapidement sans inscription manuelle
- **Tests automatisés** : Base de données propre avant chaque test
- **Démonstrations** : Données cohérentes pour les présentations
- **Onboarding** : Nouveaux développeurs ont directement des données

## Structure du projet

```
JeuxOlympiques/
│
├── Data/
│   ├── seed-data.json           # données de test
│   ├── ApplicationDbContext.cs
│   └── DbSeeder.cs              # Service de chargement
│
├── appsettings.json             # Configuration (LoadTestData: true)
├── Program.cs                   # Initialisation au démarrage
└── ...
```

## Utilisation de base

### 1. Première utilisation

Quand vous lancez l'application pour la première fois :

```bash
dotnet run
```

**Ce qui se passe :**

```
Application des migrations...
Migrations appliquées
Chargement des données de test depuis seed-data.json...
   Chargement de 3 utilisateurs...
   Utilisateurs chargés
   Chargement de 3 offres...
   Offres chargées
   Chargement de 3 commandes...
   Commandes chargées
   Chargement de 3 billets...
    Billets chargés
✅ Données de test chargées !
```

### 2. Données disponibles après le chargement

#### Comptes de test

| Email | Mot de passe | Rôle | Usage |
|-------|-------------|------|-------|
| admin@jo2024.fr | Admin2024! | Admin | Tests admin |
| jean.dupont@email.com | Test2024! | User | Tests utilisateur 1 |
| marie.martin@email.com | Test2024! | User | Tests utilisateur 2 |

#### Offres disponibles

- **Solo** : 75€ (1 personne)
- **Duo** : 130€ (2 personnes)
- **Famille** : 220€ (4 personnes)

#### Commandes pré-créées

- Jean Dupont a **2 commandes** (Solo + Duo)
- Marie Martin a **1 commande** (Famille)

#### Billets

- **3 billets** générés avec QR codes
- Statuts variés : Actif / Scanné

## Configuration

### appsettings.json

```json
{
  "LoadTestData": true,      // Activer/désactiver le chargement
  "ForceReseed": false       // Réinitialiser à chaque démarrage
}
```

### Modes de fonctionnement

#### Mode 1 : Chargement initial uniquement (recommandé pour dev)

```json
{
  "LoadTestData": true,
  "ForceReseed": false
}
```

- Charge les données **une seule fois**
- Conserve vos modifications entre les redémarrages
- Idéal pour le développement quotidien

#### Mode 2 : Réinitialisation à chaque démarrage

```json
{
  "LoadTestData": true,
  "ForceReseed": true
}
```

- **SUPPRIME** toutes les données existantes
- Recharge les données du JSON
- Utile pour les tests automatisés

#### Mode 3 : Désactivé (production)

```json
{
  "LoadTestData": false,
  "ForceReseed": false
}
```

- Aucun chargement de données de test
- Base de données vide ou avec vraies données
- À utiliser en production

## Modifier les données de test

### Exemple : Ajouter un nouvel utilisateur

Éditez `Data/seed-data.json` :

```json
{
  "utilisateurs": [
    // ... utilisateurs existants ...
    {
      "id": 4,
      "prenom": "Sophie",
      "nom": "Durand",
      "email": "sophie.durand@email.com",
      "motDePasse": "Test2024!",
      "role": "User"
    }
  ]
}
```

Relancez l'app avec `ForceReseed: true` ou utilisez l'API :

```bash
curl -X POST http://localhost:5000/api/dev/reset-database
```

### Exemple : Ajouter une nouvelle offre

```json
{
  "offres": [
    // ... offres existantes ...
    {
      "id": 4,
      "type": "VIP",
      "nom": "Billet VIP",
      "description": "Accès privilégié avec services premium",
      "prix": 500.00,
      "nombrePersonnes": 2,
      "actif": true
    }
  ]
}
```

### Exemple : Créer une commande complète

```json
{
  "commandes": [
    {
      "id": 4,
      "numero": "CMD-2024-004",
      "utilisateurId": 4,
      "dateAchat": "2024-08-01T10:00:00",
      "montantTotal": 500.00,
      "statut": "Payée",
      "cleTransaction": "xyz-123-abc-456"
    }
  ],
  "billets": [
    {
      "id": 4,
      "numero": "JO2024-VIP-004",
      "commandeId": 4,
      "offreId": 4,
      "titre": "Cérémonie d'ouverture",
      "dateEpreuve": "2024-07-26T20:00:00",
      "lieu": "Stade de France",
      "place": "Tribune VIP - Secteur A",
      "statut": "Actif",
      "codeQR": "data:image/png;base64,...",
      "cleFinal": "unique-key-here",
      "dateEmission": "2024-08-01T10:00:30",
      "dateScan": null
    }
  ]
}
```

## API de développement

L'application expose des endpoints pour gérer les données de test (développement uniquement).

### Réinitialiser la base de données

**Endpoint** : `POST /api/dev/reset-database`

```bash
# Avec curl
curl -X POST http://localhost:5000/api/dev/reset-database

# Avec PowerShell
Invoke-WebRequest -Method POST -Uri "http://localhost:5000/api/dev/reset-database"
```

**Résultat** :
- Supprime toutes les données
- Recharge depuis `seed-data.json`
- Base fraîche et propre

### Exporter les données actuelles

**Endpoint** : `GET /api/dev/export-database`

```bash
# Avec curl
curl http://localhost:5000/api/dev/export-database

# Avec PowerShell
Invoke-WebRequest -Uri "http://localhost:5000/api/dev/export-database"
```

**Résultat** :
- Crée un fichier `export-YYYYMMDD-HHmmss.json` dans `Data/`
- Contient toutes les données actuelles de la base

**Usage** : Sauvegarder vos données de test personnalisées


### Scénario 1 : Développement

```bash
# 1. Premier lancement (charge les données)
dotnet run

# 2. Travailler normalement, créer des données
# ...

# 3. Les données persistent entre les redémarrages
# (ForceReseed = false)

# 4. Si besoin de reset
curl -X POST http://localhost:5000/api/dev/reset-database
```

### Scénario 2 : Tests automatisés

```csharp
[TestInitialize]
public async Task Setup()
{
    // Réinitialiser avant chaque test
    using var client = new HttpClient();
    await client.PostAsync("http://localhost:5000/api/dev/reset-database", null);
}

[TestMethod]
public async Task TestAchatBillet()
{
    // Les données sont prévisibles
    var user = "jean.dupont@email.com";
    // ...
}
```

### Scénario 3 : Démo / Présentation

```json
// appsettings.json
{
  "LoadTestData": true,
  "ForceReseed": true  // Reset à chaque démarrage
}
```

Résultat : Données fraîches à chaque présentation !

## Créer différents scénarios de test

### Fichier : seed-data-vide.json

```json
{
  "utilisateurs": [
    {
      "id": 1,
      "prenom": "Admin",
      "nom": "JO2024",
      "email": "admin@jo2024.fr",
      "motDePasse": "Admin2024!",
      "role": "Admin"
    }
  ],
  "offres": [],
  "commandes": [],
  "billets": []
}
```

### Fichier : seed-data-complet.json

Avec beaucoup de données pour tester la performance.

### Changer de fichier

Dans `DbSeeder.cs`, modifier :

```csharp
private readonly string _jsonFilePath = Path.Combine(
    AppDomain.CurrentDomain.BaseDirectory, 
    "Data", 
    "seed-data-complet.json"  // Changez ici
);
```

Ou passer en paramètre :

```csharp
public DbSeeder(ApplicationDbContext context, ILogger<DbSeeder> logger, IConfiguration config)
{
    _jsonFilePath = config.GetValue<string>("SeedDataFile", "seed-data.json");
}
```

## Précautions importantes

### À NE PAS faire

1. **Ne pas mettre de vraies données sensibles** dans le JSON
   - Pas de vrais emails
   - Pas de vrais numéros de téléphone
   - Pas de vraies cartes bancaires

2. **Ne pas activer en production**
   ```json
   // DANGER en production
   {
     "LoadTestData": true  
   }
   ```

3. **Ne pas commiter des mots de passe forts**
   - Utilisez des mots de passe simples pour les tests
   - Les vrais mots de passe doivent être hashés en production

### Bonnes pratiques

1. **Utiliser des données fictives mais réalistes**
   ```json
   {
     "prenom": "Jean",
     "nom": "Dupont",
     "email": "jean.dupont@example.com"  // Domaine example.com
   }
   ```

2. **Documenter vos données de test**
   ```json
   {
     "_comment": "Utilisateur pour tester les commandes multiples",
     "email": "jean.dupont@example.com"
   }
   ```

3. **Versionner le fichier JSON**
   ```bash
   git add Data/seed-data.json
   git commit -m "Ajout données test pour scenario X"
   ```

4. **Créer des backups**
   ```bash
   # Exporter avant de modifier
   curl http://localhost:5000/api/dev/export-database
   ```

## Dépannage

### Problème : Les données ne se chargent pas

**Vérifications :**

1. Le fichier existe-t-il ?
   ```bash
   ls Data/seed-data.json
   ```

2. Le JSON est-il valide ?
   ```bash
   # Valider sur jsonlint.com ou avec un outil
   ```

3. Les logs disent quoi ?
   ```bash
   dotnet run | grep "seed"
   ```

4. `LoadTestData` est-il à `true` ?
   ```json
   {
     "LoadTestData": true  // Vérifiez ici
   }
   ```

### Problème : Erreur de contrainte de clé étrangère

```
Cannot add or update a child row: foreign key constraint fails
```

**Solution** : Vérifier que les IDs correspondent

```json
{
  "commandes": [
    {
      "utilisateurId": 2  // Doit exister dans utilisateurs
    }
  ],
  "billets": [
    {
      "commandeId": 1,    // Doit exister dans commandes
      "offreId": 1        // Doit exister dans offres
    }
  ]
}
```

### Problème : Données dupliquées

```
Duplicate entry 'admin@jo2024.fr' for key 'email'
```

**Solution 1** : Utiliser `ForceReseed: true`

**Solution 2** : Supprimer manuellement la base
```bash
dotnet ef database drop
dotnet ef database update
```

## Performances

### Chargement rapide

Pour **beaucoup de données** (1000+ enregistrements) :

```csharp
// Dans DbSeeder.cs
// Au lieu de Add() un par un
_context.Utilisateurs.AddRange(utilisateurs);
await _context.SaveChangesAsync();
```

### Désactiver temporairement

Si le chargement est trop lent en dev :

```bash
# Via variable d'environnement
export LoadTestData=false
dotnet run
```

Ou dans `launchSettings.json` :

```json
{
  "environmentVariables": {
    "LoadTestData": "false"
  }
}
```

## Exemples d'utilisation avancée

### Générer des données aléatoires

Utilisez une bibliothèque comme **Bogus** :

```bash
dotnet add package Bogus
```

```csharp
using Bogus;

public List<UtilisateurSeed> GenererUtilisateursAleatoires(int count)
{
    var faker = new Faker<UtilisateurSeed>()
        .RuleFor(u => u.Prenom, f => f.Name.FirstName())
        .RuleFor(u => u.Nom, f => f.Name.LastName())
        .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.Prenom, u.Nom))
        .RuleFor(u => u.MotDePasse, f => "Test2024!")
        .RuleFor(u => u.Role, f => "User");
    
    return faker.Generate(count);
}
```

### Charger depuis plusieurs fichiers

```csharp
public async Task SeedFromMultipleFiles()
{
    await LoadFile("seed-utilisateurs.json");
    await LoadFile("seed-offres.json");
    await LoadFile("seed-commandes.json");
}
```

## Résumé

| Fonctionnalité | Commande/Config |
|---------------|-----------------|
| Activer le chargement | `LoadTestData: true` |
| Reset à chaque démarrage | `ForceReseed: true` |
| Reset manuel | `POST /api/dev/reset-database` |
| Exporter les données | `GET /api/dev/export-database` |
| Modifier les données | Éditer `Data/seed-data.json` |
