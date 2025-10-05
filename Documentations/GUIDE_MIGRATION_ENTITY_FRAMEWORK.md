# Guide complet des Migrations Entity Framework Core

## C'est quoi une migration ?

Une **migration** est un fichier C# qui contient les instructions pour **créer ou modifier** la structure de votre base de données.

### Analogie simple

Imaginez que vous construisez une maison :
- **Vos Models C#** = Les plans de la maison 
- **La Migration** = Les instructions pour les ouvriers 
- **La Base de données** = La maison construite 

**Quand vous modifiez les plans**, vous créez de nouvelles instructions pour les ouvriers !

---

## Structure du dossier Migrations/

Quand vous créez une migration, Entity Framework génère ces fichiers :

```
Migrations/
│
├── 20241004120530_InitialCreate.cs          # La migration principale
├── 20241004120530_InitialCreate.Designer.cs  # Métadonnées EF
└── ApplicationDbContextModelSnapshot.cs      # État actuel du modèle
```

### Décryptage du nom de fichier

```
20241004120530_InitialCreate.cs
│           │  │
│           │  └─ Nom donné par vous
│           │
│           └─ Timestamp (horodatage)
│              2024-10-04 12:05:30
│
└─ Permet de trier chronologiquement
```

---

## Contenu d'un fichier de migration

### Exemple : `20241004120530_InitialCreate.cs`

```csharp
using Microsoft.EntityFrameworkCore.Migrations;

namespace JeuxOlympiques.Migrations
{
    public partial class InitialCreate : Migration
    {
        // Ce qui se passe quand on APPLIQUE la migration
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Créer la table Utilisateurs
            migrationBuilder.CreateTable(
                name: "Utilisateurs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", 
                            MySqlValueGenerationStrategy.IdentityColumn),
                    Prenom = table.Column<string>(maxLength: 100, nullable: false),
                    Nom = table.Column<string>(maxLength: 100, nullable: false),
                    Email = table.Column<string>(maxLength: 255, nullable: false),
                    MotDePasseHash = table.Column<string>(nullable: false),
                    CleUtilisateur = table.Column<string>(nullable: false),
                    Role = table.Column<string>(maxLength: 20, nullable: false),
                    DateCreation = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilisateurs", x => x.Id);
                });

            // Créer un index unique sur Email
            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_Email",
                table: "Utilisateurs",
                column: "Email",
                unique: true);

            // Créer la table Offres
            migrationBuilder.CreateTable(
                name: "Offres",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", 
                            MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<string>(maxLength: 50, nullable: false),
                    Nom = table.Column<string>(maxLength: 200, nullable: false),
                    Description = table.Column<string>(nullable: false),
                    Prix = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    NombrePersonnes = table.Column<int>(nullable: false),
                    Actif = table.Column<bool>(nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Offres", x => x.Id);
                });

            // Créer la table Commandes avec clé étrangère
            migrationBuilder.CreateTable(
                name: "Commandes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", 
                            MySqlValueGenerationStrategy.IdentityColumn),
                    Numero = table.Column<string>(maxLength: 50, nullable: false),
                    UtilisateurId = table.Column<int>(nullable: false),
                    DateAchat = table.Column<DateTime>(nullable: false),
                    MontantTotal = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Statut = table.Column<string>(maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commandes", x => x.Id);
                    // Clé étrangère vers Utilisateurs
                    table.ForeignKey(
                        name: "FK_Commandes_Utilisateurs_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // etc... pour toutes les autres tables
        }

        // Ce qui se passe quand on ANNULE la migration
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Supprimer les tables dans l'ordre inverse (à cause des clés étrangères)
            migrationBuilder.DropTable(name: "Billets");
            migrationBuilder.DropTable(name: "Commandes");
            migrationBuilder.DropTable(name: "Offres");
            migrationBuilder.DropTable(name: "Utilisateurs");
        }
    }
}
```

---

## Pourquoi les migrations ?

### SANS migrations (l'ancienne méthode)

```sql
-- Vous devez écrire manuellement le SQL
CREATE TABLE Utilisateurs (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Prenom VARCHAR(100) NOT NULL,
    ...
);

-- Si vous modifiez votre code, vous devez :
-- 1. Écrire à nouveau du SQL
-- 2. L'exécuter sur la base
-- 3. Le faire pour chaque environnement (dev, test, prod)
-- 4. Risque d'oublier des colonnes
```

### AVEC migrations (Entity Framework)

```csharp
// 1. Vous modifiez vos models C#
public class Utilisateur {
    public string Telephone { get; set; }  // ⭐ Nouveau champ
}

// 2. Vous créez une migration
dotnet ef migrations add AjoutTelephone

// 3. EF génère AUTOMATIQUEMENT le SQL
// 4. Vous appliquez sur toutes les bases
dotnet ef database update
```

**Avantages** :
- Pas de SQL manuel
- Historique des changements
- Versionné avec Git
- Appliqué automatiquement
- Compatible tous types de DB (MySQL, PostgreSQL, SQL Server)

---

## Commandes principales

### 1. Créer une migration

```bash
dotnet ef migrations add NomDeLaMigration
```

**Quand l'utiliser ?**
- Après avoir créé/modifié vos Models
- Avant de créer/modifier la base de données

**Exemples de noms** :
```bash
dotnet ef migrations add InitialCreate          # Première migration
dotnet ef migrations add AjoutTablePanier       # Ajouter une table
dotnet ef migrations add ModifColonnePrix       # Modifier une colonne
dotnet ef migrations add SuppressionChampObsolete  # Supprimer une colonne
```

**Ce qui est créé** :
```
Migrations/
├── 20241004120530_NomDeLaMigration.cs
├── 20241004120530_NomDeLaMigration.Designer.cs
└── ApplicationDbContextModelSnapshot.cs (mis à jour)
```

---

### 2. Appliquer les migrations

```bash
dotnet ef database update
```

**Quand l'utiliser ?**
- Après avoir créé une migration
- Pour mettre à jour la base de données

**Ce qui se passe** :
1. EF regarde quelles migrations ne sont pas encore appliquées
2. Il exécute la méthode `Up()` de chaque migration
3. Il enregistre dans la table `__EFMigrationsHistory` que c'est fait

**Vérifier l'historique** :
```sql
SELECT * FROM __EFMigrationsHistory;
```

Résultat :
```
MigrationId                         | ProductVersion
------------------------------------|---------------
20241004120530_InitialCreate        | 8.0.0
20241005093000_AjoutTablePanier     | 8.0.0
```

---

### 3. Appliquer jusqu'à une migration spécifique

```bash
# Appliquer jusqu'à une migration précise
dotnet ef database update NomDeLaMigration

# Revenir à une migration antérieure
dotnet ef database update InitialCreate
```

---

### 4. Annuler la dernière migration (pas encore appliquée)

```bash
dotnet ef migrations remove
```

** Attention** : Fonctionne SEULEMENT si la migration n'a PAS été appliquée !

---

### 5. Annuler une migration appliquée

```bash
# Revenir à la migration précédente
dotnet ef database update MigrationPrecedente

# Puis supprimer la migration
dotnet ef migrations remove
```

---

### 6. Lister toutes les migrations

```bash
dotnet ef migrations list
```

Résultat :
```
20241004120530_InitialCreate (Applied)
20241005093000_AjoutTablePanier (Applied)
20241006140000_ModifColonnePrix (Pending)
```

---

### 7. Générer un script SQL (sans appliquer)

```bash
# Générer le SQL de toutes les migrations
dotnet ef migrations script -o migrations.sql

# Générer le SQL d'une migration spécifique
dotnet ef migrations script MigrationA MigrationB -o update.sql
```

**Usage** : Pour appliquer manuellement sur un serveur de production

---

## 🔄 Workflow complet

### Scénario : Ajouter une nouvelle table "Panier"

#### Étape 1 : Créer le Model

```csharp
// Models/Panier.cs
public class Panier
{
    public int Id { get; set; }
    public int UtilisateurId { get; set; }
    public Utilisateur Utilisateur { get; set; }
    public DateTime DateCreation { get; set; }
}
```

#### Étape 2 : Ajouter dans DbContext

```csharp
// Data/ApplicationDbContext.cs
public class ApplicationDbContext : DbContext
{
    public DbSet<Utilisateur> Utilisateurs { get; set; }
    public DbSet<Panier> Paniers { get; set; }  //  Nouveau
}
```

#### Étape 3 : Créer la migration

```bash
dotnet ef migrations add AjoutTablePanier
```

**EF génère automatiquement** :
```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.CreateTable(
        name: "Paniers",
        columns: table => new { ... },
        constraints: table => { ... }
    );
}
```

#### Étape 4 : Vérifier la migration

Ouvrez le fichier `Migrations/xxxxx_AjoutTablePanier.cs` et vérifiez que tout est correct.

#### Étape 5 : Appliquer

```bash
dotnet ef database update
```

**Résultat** : La table `Paniers` est créée dans MySQL !

#### Étape 6 : Vérifier

```sql
-- Dans MySQL
SHOW TABLES;
-- Doit afficher : Paniers

DESCRIBE Paniers;
-- Doit afficher les colonnes
```

---

## Cas d'usage courants

### Cas 1 : Modifier une colonne existante

```csharp
// AVANT
public class Utilisateur {
    public string Nom { get; set; }
}

// APRÈS : Augmenter la longueur max
public class Utilisateur {
    [MaxLength(200)]  // Au lieu de 100
    public string Nom { get; set; }
}
```

```bash
dotnet ef migrations add AugmentationLongueurNom
dotnet ef database update
```

---

### Cas 2 : Ajouter une colonne

```csharp
// Ajouter un champ
public class Utilisateur {
    public string Telephone { get; set; }  // Nouveau
}
```

```bash
dotnet ef migrations add AjoutTelephone
dotnet ef database update
```

---

### Cas 3 : Supprimer une colonne

```csharp
// Supprimer un champ
public class Utilisateur {
    // public string AncienChamp { get; set; }  // Supprimé
}
```

```bash
dotnet ef migrations add SuppressionAncienChamp
dotnet ef database update
```

---

### Cas 4 : Renommer une colonne

**Attention** : EF va créer une nouvelle colonne et supprimer l'ancienne !

**Solution manuelle** :

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // Ne pas utiliser le code auto-généré
    // Écrire manuellement :
    migrationBuilder.RenameColumn(
        name: "OldName",
        table: "Utilisateurs",
        newName: "NewName");
}
```

---

## Problèmes courants

### Problème 1 : "Build failed"

**Erreur** :
```
Build failed. Use dotnet build to see the errors.
```

**Solution** :
```bash
# Compiler d'abord
dotnet build

# Corriger les erreurs de compilation
# Puis relancer
dotnet ef migrations add ...
```

---

### Problème 2 : "Your startup project 'X' doesn't reference Microsoft.EntityFrameworkCore.Design"

**Solution** :
```bash
dotnet add package Microsoft.EntityFrameworkCore.Design
```

---

### Problème 3 : "No DbContext was found"

**Solution** : Vérifier que votre `ApplicationDbContext` hérite bien de `DbContext` :

```csharp
public class ApplicationDbContext : DbContext  // Important
{
    // ...
}
```

---

### Problème 4 : "The migration '...' has already been applied"

**Erreur** : Vous essayez d'appliquer une migration déjà appliquée.

**Solution** :
```bash
# Voir l'état
dotnet ef migrations list

# La migration est déjà appliquée, rien à faire !
```

---

### Problème 5 : "Unable to connect to the database"

**Causes** :
- MySQL n'est pas démarré
- Mauvaise connexion string
- Port occupé

**Solutions** :
```bash
# Vérifier Docker
docker ps

# Vérifier la connexion
docker exec jo_mysql_db mysql -u jo_user -pJO2024Password! -e "SHOW DATABASES;"

# Vérifier appsettings.json
```

---

## Bonnes pratiques

### À FAIRE

1. **Créer une migration après chaque modification de model**
   ```bash
   # Après avoir modifié User.cs
   dotnet ef migrations add DescriptionDeLaModification
   ```

2. **Nommer clairement vos migrations**
   ```bash
   ✅ dotnet ef migrations add AjoutColonneTelephone
   ❌ dotnet ef migrations add Update1
   ```

3. **Vérifier le contenu avant d'appliquer**
   ```bash
   # Créer la migration
   dotnet ef migrations add ...
   
   # Ouvrir et vérifier le fichier
   # Migrations/xxxxx_....cs
   
   # Puis appliquer
   dotnet ef database update
   ```

4. **Versionner les migrations avec Git**
   ```bash
   git add Migrations/
   git commit -m "Migration: Ajout table Panier"
   ```

---

### ❌ À NE PAS FAIRE

1. **Modifier une migration déjà appliquée**
   ```bash
   # ❌ Jamais modifier un fichier de migration appliqué
   # ✅ Créer une nouvelle migration pour corriger
   ```

2. **Supprimer le dossier Migrations/**
   ```bash
   # ❌ Ne JAMAIS supprimer ce dossier si migrations appliquées
   # Vous perdrez l'historique !
   ```

3. **Appliquer directement du SQL manuel**
   ```sql
   -- ❌ Éviter
   ALTER TABLE Utilisateurs ADD COLUMN Telephone VARCHAR(20);
   
   -- ✅ Préférer
   -- Créer une migration EF
   ```

---

## 📊 Résumé visuel

```
Vos Models C#              Migrations/                  Base de données
─────────────              ───────────                  ───────────────
                                                        
User.cs          ──────►   xxxxx_Initial.cs    ──────►  Table Users
Offre.cs                   │                            Table Offres
Billet.cs                  │  Up() {                    Table Billets
                           │    CreateTable...
                           │  }
                           │
                           │  Down() {
                           │    DropTable...
                           │  }
                           │
                           └──► __EFMigrationsHistory
                                (historique des migrations appliquées)


Commandes:
──────────

1. dotnet ef migrations add NomMigration
   └─► Génère le fichier de migration

2. dotnet ef database update
   └─► Applique sur la DB

3. dotnet ef migrations remove
   └─► Supprime la dernière (si pas appliquée)
```

---

## Résumé rapide

| Commande | Usage | Quand |
|----------|-------|-------|
| `dotnet ef migrations add Nom` | Créer une migration | Après modif des Models |
| `dotnet ef database update` | Appliquer les migrations | Créer/Modifier la DB |
| `dotnet ef migrations list` | Lister les migrations | Voir l'historique |
| `dotnet ef migrations remove` | Supprimer la dernière | Annuler (si pas appliquée) |
| `dotnet ef migrations script` | Générer le SQL | Pour prod manuelle |

---

Les migrations sont le **pont automatique** entre votre code C# et votre base de données MySQL/PostgreSQL.