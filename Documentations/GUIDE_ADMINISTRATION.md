# Guide d'Administration - JO 2024

## Système de Rôles

### Hiérarchie des rôles

```
SuperAdmin (Accès total)
    ↓
Admin (Gestion utilisateurs, offres, commandes)
    ↓
User (Utilisateur standard)
```

### Permissions par rôle

| Fonctionnalité                  | User | Admin | SuperAdmin |
|---------------------------------|----- |-------|------------|
| Acheter des billets             |  ✅  |  ✅  |    ✅     |
| Voir ses commandes              |  ✅  |  ✅  |    ✅     |
| Gérer son profil                |  ✅  |  ✅  |    ✅     |
| Voir tous les utilisateurs      |  ❌  |  ✅  |    ✅     |
| Activer/Désactiver utilisateurs |  ❌  |  ✅  |    ✅     |
| Gérer les offres                |  ❌  |  ✅  |    ✅     |
| Voir toutes les commandes       |  ❌  |  ✅  |    ✅     |
| Annuler des billets             |  ❌  |  ✅  |    ✅     |
| Modifier les rôles              |  ❌  |  ❌  |    ✅     |
| Voir les statistiques           |  ❌  |  ✅  |    ✅     |
| Exporter les données            |  ❌  |  ✅  |    ✅     |

## 👤 Compte Administrateur par Défaut

### Informations de connexion

```
Email: admin@jo2024.fr
Mot de passe: Admin@2024
Rôle: SuperAdmin
```

⚠️ **IMPORTANT**: Changez ce mot de passe immédiatement après la première connexion !

### Changer le mot de passe admin

```bash
# Via API
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@jo2024.fr",
    "password": "Admin@2024"
  }'

# Puis avec le token obtenu
curl -X POST http://localhost:5000/api/compte/change-password \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer VOTRE_TOKEN" \
  -d '{
    "currentPassword": "Admin@2024",
    "newPassword": "VotreNouveauMotDePasseSecurise@2024"
  }'
```

## API Admin - Endpoints

### Base URL
```
/api/admin
```

### Authentification requise
Tous les endpoints admin nécessitent :
- Header: `Authorization: Bearer VOTRE_TOKEN_JWT`
- Rôle: `Admin` ou `SuperAdmin`

---

## Gestion des Utilisateurs

### Liste des utilisateurs (paginée)

```http
GET /api/admin/users?page=1&pageSize=20
Authorization: Bearer {token}
```

**Réponse:**
```json
{
  "items": [
    {
      "id": 1,
      "prenom": "Jean",
      "nom": "Dupont",
      "email": "jean.dupont@example.com",
      "role": "User",
      "estActif": true,
      "dateCreation": "2024-01-15T10:30:00Z",
      "derniereConnexion": "2024-10-10T08:15:00Z",
      "nombreCommandes": 3,
      "nombreBillets": 8,
      "totalDepense": 390.00
    }
  ],
  "totalCount": 156,
  "pageNumber": 1,
  "pageSize": 20,
  "totalPages": 8
}
```

### Détails d'un utilisateur

```http
GET /api/admin/users/{id}
Authorization: Bearer {token}
```

### Activer/Désactiver un utilisateur

```http
PUT /api/admin/users/{id}/toggle-status
Authorization: Bearer {token}
```

### Modifier le rôle d'un utilisateur (SuperAdmin uniquement)

```http
PUT /api/admin/users/{id}/role
Authorization: Bearer {token}
Content-Type: application/json

{
  "role": "Admin"
}
```

Rôles possibles: `User`, `Admin`, `SuperAdmin`

---

## Gestion des Offres

### Créer une offre

```http
POST /api/admin/offres
Authorization: Bearer {token}
Content-Type: application/json

{
  "type": "vip",
  "nom": "Offre VIP",
  "description": "Accès privilégié avec services premium",
  "prix": 350.00,
  "nombrePersonnes": 2
}
```

### Modifier une offre

```http
PUT /api/admin/offres/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "nom": "Offre VIP Premium",
  "description": "Accès VIP avec services exceptionnels",
  "prix": 400.00,
  "estActif": true
}
```

### Désactiver une offre

```http
DELETE /api/admin/offres/{id}
Authorization: Bearer {token}
```

Note: Ceci effectue un "soft delete" (désactivation) pour préserver l'historique.

---

## Gestion des Commandes

### Liste de toutes les commandes

```http
GET /api/admin/commandes?page=1&pageSize=20
Authorization: Bearer {token}
```

### Modifier le statut d'une commande

```http
PUT /api/admin/commandes/{id}/status
Authorization: Bearer {token}
Content-Type: application/json

{
  "status": "Remboursée"
}
```

Statuts possibles: `Payée`, `Utilisée`, `Annulée`, `Remboursée`

---

## Gestion des Billets

### Liste de tous les billets

```http
GET /api/admin/billets?page=1&pageSize=20
Authorization: Bearer {token}
```

### Annuler un billet

```http
PUT /api/admin/billets/{id}/cancel
Authorization: Bearer {token}
```

Note: Impossible d'annuler un billet déjà scanné.

---

## Statistiques

### Dashboard général

```http
GET /api/admin/stats/dashboard
Authorization: Bearer {token}
```

**Réponse:**
```json
{
  "totalUtilisateurs": 156,
  "utilisateursActifs": 142,
  "totalCommandes": 543,
  "totalBillets": 1287,
  "chiffreAffaireTotal": 75690.00,
  "chiffreAffaireMoisActuel": 12450.00,
  "ventesParOffre": {
    "Offre Solo": 234,
    "Offre Duo": 187,
    "Offre Famille": 122
  },
  "ventesParSport": {
    "Natation": 345,
    "Athlétisme": 421,
    "Basketball": 287,
    "Surf": 234
  }
}
```

### Statistiques de ventes

```http
GET /api/admin/stats/sales?startDate=2024-01-01&endDate=2024-12-31
Authorization: Bearer {token}
```

**Réponse:**
```json
[
  {
    "date": "2024-10-01",
    "nombreVentes": 15,
    "montant": 1950.00
  },
  {
    "date": "2024-10-02",
    "nombreVentes": 23,
    "montant": 2890.00
  }
]
```

---

## Exports

### Exporter la liste des utilisateurs (CSV)

```http
GET /api/admin/export/users
Authorization: Bearer {token}
```

Télécharge un fichier CSV contenant tous les utilisateurs.

### Exporter les commandes (CSV)

```http
GET /api/admin/export/commandes?startDate=2024-01-01&endDate=2024-12-31
Authorization: Bearer {token}
```

Télécharge un fichier CSV des commandes pour la période spécifiée.

---

## Sécurité

### Bonnes pratiques

1. **Changer les mots de passe par défaut**
   - Changez immédiatement le mot de passe admin après installation

2. **Limiter les comptes admin**
   - Créez uniquement les comptes admin nécessaires
   - Utilisez le rôle `Admin` plutôt que `SuperAdmin` quand possible

3. **Surveiller les activités**
   - Consultez régulièrement les logs
   - Surveillez les connexions admin

4. **Tokens JWT**
   - Les tokens expirent après 24h par défaut
   - Conservez-les de manière sécurisée
   - Ne partagez jamais vos tokens

5. **HTTPS en production**
   - Utilisez toujours HTTPS en production
   - Configurez un certificat SSL valide

### Logs d'activité

Les activités admin sont loggées automatiquement :

```bash
# Voir les logs
docker-compose logs -f api

# Filtrer les logs admin
docker-compose logs api | grep "Admin"
```

---

## Cas d'usage courants

### Créer un nouvel administrateur

1. L'utilisateur doit d'abord créer un compte normal via `/api/auth/register`
2. Un SuperAdmin change son rôle via `/api/admin/users/{id}/role`

```bash
# 1. L'utilisateur s'inscrit
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "prenom": "Marie",
    "nom": "Martin",
    "email": "marie.martin@jo2024.fr",
    "password": "SecurePass@123"
  }'

# 2. SuperAdmin se connecte et récupère le token
# 3. SuperAdmin change le rôle
curl -X PUT http://localhost:5000/api/admin/users/5/role \
  -H "Authorization: Bearer SUPER_ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"role": "Admin"}'
```

### Désactiver un utilisateur frauduleux

```bash
curl -X PUT http://localhost:5000/api/admin/users/42/toggle-status \
  -H "Authorization: Bearer ADMIN_TOKEN"
```

### Rembourser une commande

```bash
# 1. Changer le statut de la commande
curl -X PUT http://localhost:5000/api/admin/commandes/123/status \
  -H "Authorization: Bearer ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"status": "Remboursée"}'

# 2. Annuler les billets associés
curl -X PUT http://localhost:5000/api/admin/billets/456/cancel \
  -H "Authorization: Bearer ADMIN_TOKEN"
```

### Créer une offre spéciale temporaire

```bash
curl -X POST http://localhost:5000/api/admin/offres \
  -H "Authorization: Bearer ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "type": "flash",
    "nom": "Offre Flash Weekend",
    "description": "Offre spéciale valable ce weekend uniquement",
    "prix": 99.00,
    "nombrePersonnes": 2
  }'

# Plus tard, la désactiver
curl -X DELETE http://localhost:5000/api/admin/offres/4 \
  -H "Authorization: Bearer ADMIN_TOKEN"
```

---

## Dépannage

### Erreur 401 Unauthorized

**Problème**: Le token JWT n'est pas valide ou a expiré.

**Solutions**:
1. Reconnectez-vous pour obtenir un nouveau token
2. Vérifiez que le header Authorization est correct: `Bearer {token}`
3. Vérifiez que l'utilisateur a le bon rôle (Admin ou SuperAdmin)

### Erreur 403 Forbidden

**Problème**: L'utilisateur n'a pas les permissions nécessaires.

**Solutions**:
1. Vérifiez le rôle de l'utilisateur
2. Certaines actions nécessitent le rôle SuperAdmin

### Impossible de modifier un rôle

**Problème**: Seul SuperAdmin peut modifier les rôles.

**Solution**: Connectez-vous avec le compte SuperAdmin.

---

# #########################################################
# Evolution possible de l'appplication
# ###########################################################
## Interface d'administration (Future)

Une interface web d'administration peut être créée pour faciliter la gestion. Elle devrait inclure :

- ✅ Dashboard avec statistiques en temps réel
- ✅ Gestion CRUD des utilisateurs
- ✅ Gestion CRUD des offres
- ✅ Vue détaillée des commandes
- ✅ Recherche et filtres avancés
- ✅ Exports de données
- ✅ Graphiques et analytics
- ✅ Gestion des logs

### Technologies recommandées

- **React** ou **Vue.js** pour le frontend
- **Tailwind CSS** pour le styling
- **Chart.js** pour les graphiques
- **React Query** pour la gestion du cache

---

## 🔄 Migration des données

### Ajouter le champ Role aux utilisateurs existants

Si vous avez déjà des utilisateurs dans la base sans le champ `Role`:

```bash
# Créer une nouvelle migration
cd JO2024.API
dotnet ef migrations add AddRoleToUtilisateur --project ../JO2024.Infrastructure

# Appliquer la migration
dotnet ef database update --project ../JO2024.Infrastructure
```

### Script SQL pour mise à jour manuelle

```sql
-- Ajouter la colonne Role si elle n'existe pas
ALTER TABLE Utilisateurs ADD COLUMN Role VARCHAR(50) DEFAULT 'User';

-- Mettre à jour tous les utilisateurs existants
UPDATE Utilisateurs SET Role = 'User' WHERE Role IS NULL;

-- Définir l'admin principal
UPDATE Utilisateurs SET Role = 'SuperAdmin' WHERE Email = 'admin@jo2024.fr';
```

---

## 📋 Checklist de mise en production

### Avant le déploiement

- [ ] Changer le mot de passe admin par défaut
- [ ] Créer les comptes admin nécessaires
- [ ] Tester tous les endpoints admin
- [ ] Configurer les logs centralisés
- [ ] Activer HTTPS uniquement
- [ ] Configurer les sauvegardes automatiques
- [ ] Limiter les tentatives de connexion (rate limiting)
- [ ] Activer l'authentification à deux facteurs (2FA)
- [ ] Documenter les procédures d'urgence
- [ ] Former les administrateurs

### Monitoring en production

```bash
# Surveiller les connexions admin
docker-compose logs api | grep "Admin" | tail -f

# Surveiller les erreurs
docker-compose logs api | grep "Error" | tail -f

# Surveiller les performances
docker stats jo2024_api
```

---

## Procédures d'urgence

### Réinitialiser le mot de passe admin

Si vous avez perdu le mot de passe admin, utilisez ce script SQL:

```sql
-- Générer un nouveau hash BCrypt pour le mot de passe "Admin@2024"
-- Hash: $2a$11$... (générer avec BCrypt online tool)

UPDATE Utilisateurs 
SET MotDePasseHash = '$2a$11$VotreHashBCrypt' 
WHERE Email = 'admin@jo2024.fr';
```

Ou via code C#:

```csharp
using BCrypt.Net;

var newPassword = "Admin@2024";
var hash = BCrypt.Net.BCrypt.HashPassword(newPassword);
Console.WriteLine(hash);
// Copiez ce hash dans la requête SQL ci-dessus
```

### Désactiver tous les utilisateurs frauduleux

```sql
-- Désactiver plusieurs utilisateurs
UPDATE Utilisateurs 
SET EstActif = FALSE 
WHERE Email IN ('fraud1@example.com', 'fraud2@example.com');
```

### Annuler toutes les commandes d'un utilisateur

```sql
-- Annuler les commandes
UPDATE Commandes 
SET Statut = 'Annulée' 
WHERE UtilisateurId = {id_utilisateur};

-- Annuler les billets
UPDATE Billets 
SET Statut = 'Annulé' 
WHERE UtilisateurId = {id_utilisateur};
```

---

## Rapports et Analytics

### Rapport mensuel des ventes

```bash
curl -X GET "http://localhost:5000/api/admin/stats/sales?startDate=2024-10-01&endDate=2024-10-31" \
  -H "Authorization: Bearer ADMIN_TOKEN" \
  > rapport_octobre_2024.json
```

### Top 10 des clients

```sql
SELECT 
    u.Id,
    u.Prenom,
    u.Nom,
    u.Email,
    COUNT(c.Id) as NombreCommandes,
    SUM(c.MontantTotal) as TotalDepense
FROM Utilisateurs u
LEFT JOIN Commandes c ON u.Id = c.UtilisateurId
GROUP BY u.Id, u.Prenom, u.Nom, u.Email
ORDER BY TotalDepense DESC
LIMIT 10;
```

### Sports les plus populaires

```sql
SELECT 
    Sport,
    COUNT(*) as NombreBillets,
    COUNT(DISTINCT UtilisateurId) as NombreClients
FROM Billets
GROUP BY Sport
ORDER BY NombreBillets DESC;
```

### Taux de conversion (Inscriptions → Achats)

```sql
SELECT 
    (SELECT COUNT(*) FROM Utilisateurs WHERE Role = 'User') as TotalUtilisateurs,
    (SELECT COUNT(DISTINCT UtilisateurId) FROM Commandes) as UtilisateursAyantAchete,
    ROUND(
        (SELECT COUNT(DISTINCT UtilisateurId) FROM Commandes) * 100.0 / 
        (SELECT COUNT(*) FROM Utilisateurs WHERE Role = 'User'),
        2
    ) as TauxConversion;
```

---

## Formation des administrateurs

### Compétences requises

1. **Techniques**
   - Comprendre les API REST
   - Utiliser Swagger pour tester
   - Lire les logs
   - Notions de SQL

2. **Métier**
   - Comprendre le processus de vente
   - Gérer les réclamations clients
   - Politique de remboursement
   - Gestion des fraudes

### Tutoriel rapide pour nouveaux admins

#### 1. Se connecter

```bash
# Obtenir un token
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "votre.email@jo2024.fr",
    "password": "VotreMotDePasse"
  }'

# Sauvegarder le token
TOKEN="eyJhbGciOiJIUzI1NiIs..."
```

#### 2. Consulter le dashboard

```bash
curl -X GET http://localhost:5000/api/admin/stats/dashboard \
  -H "Authorization: Bearer $TOKEN"
```

#### 3. Chercher un utilisateur

```bash
# Par pagination
curl -X GET "http://localhost:5000/api/admin/users?page=1&pageSize=20" \
  -H "Authorization: Bearer $TOKEN"

# Détails d'un utilisateur spécifique
curl -X GET http://localhost:5000/api/admin/users/5 \
  -H "Authorization: Bearer $TOKEN"
```

#### 4. Gérer une réclamation

```bash
# 1. Trouver la commande
curl -X GET "http://localhost:5000/api/admin/commandes?page=1" \
  -H "Authorization: Bearer $TOKEN"

# 2. Rembourser
curl -X PUT http://localhost:5000/api/admin/commandes/123/status \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"status": "Remboursée"}'
```

---

## Support et ressources

### Documentation API complète

Accédez à Swagger pour la documentation interactive:
- **Dev**: http://localhost:5000/swagger
- **Prod**: https://api.jo2024.fr/swagger

### Contacts

- **Support technique**: support-tech@jo2024.fr
- **Questions métier**: admin@jo2024.fr
- **Urgences**: +33 1 XX XX XX XX (24/7)

### Ressources utiles

- [Documentation ASP.NET Core](https://docs.microsoft.com/aspnet/core)
- [Guide JWT](https://jwt.io/introduction)
- [BCrypt Online Tool](https://bcrypt-generator.com/)
- [MySQL Workbench](https://www.mysql.com/products/workbench/)
- [Postman](https://www.postman.com/) - Pour tester les API

---



### Test rapide

```bash
# 1. Se connecter en tant qu'admin
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@jo2024.fr",
    "password": "Admin@2024"
  }'

# 2. Utiliser le token pour accéder au dashboard admin
curl -X GET http://localhost:5000/api/admin/stats/dashboard \
  -H "Authorization: Bearer VOTRE_TOKEN"
```

---

## ✅ Fonctionnalités admin complètes

- ✅ **Système de rôles** (User, Admin, SuperAdmin)
- ✅ **Compte admin par défaut**
- ✅ **Gestion des utilisateurs** (liste, détails, activation/désactivation)
- ✅ **Gestion des rôles** (SuperAdmin uniquement)
- ✅ **Gestion des offres** (CRUD complet)
- ✅ **Gestion des commandes** (liste, modification du statut)
- ✅ **Gestion des billets** (liste, annulation)
- ✅ **Dashboard statistiques** (utilisateurs, ventes, CA)
- ✅ **Statistiques de ventes** (par période)
- ✅ **Exports CSV** (utilisateurs, commandes)
- ✅ **Autorisation basée sur les rôles** (JWT Claims)
- ✅ **Logs d'activité**
- ✅ **API RESTful complète**

---

