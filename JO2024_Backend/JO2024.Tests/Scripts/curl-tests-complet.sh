#!/bin/bash

## Configuration de Base

```bash
# Variable d'environnement
export BASE_URL="http://localhost:5000/api"
```

---

##  1. INSCRIPTION SIMPLE (Sans Newsletter)

```bash
curl -X POST "$BASE_URL/Auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "prenom": "Jean",
    "nom": "Dupont",
    "email": "jean.dupont@test.com",
    "password": "Password123!"
  }' | jq '.'
```

**Résultat attendu :**
```json
{
  "success": true,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "message": "Inscription réussie ! Un email de bienvenue vous a été envoyé.",
  "user": {
    "id": 1,
    "prenom": "Jean",
    "nom": "Dupont",
    "email": "jean.dupont@test.com"
  }
}
```

---

## 2. INSCRIPTION AVEC NEWSLETTER COMPLÈTE

```bash
curl -X POST "$BASE_URL/Auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "prenom": "Marie",
    "nom": "Martin",
    "email": "marie.martin@test.com",
    "password": "Password123!",
    "newsletterPreferences": {
      "subscribed": true,
      "categories": {
        "sport": true,
        "evenements": true,
        "billets": true
      },
      "sports": [
        {"id": "natation", "name": "Natation"},
        {"id": "athletisme", "name": "Athlétisme"},
        {"id": "basketball", "name": "Basketball"}
      ]
    }
  }' | jq '.'
```

**Résultat attendu :**
```json
{
  "success": true,
  "token": "eyJhbGc...",
  "message": "Félicitations Marie ! Votre compte a été créé avec succès. 📧 Vous êtes abonné à la newsletter (Sports: Natation, Athlétisme, Basketball). 📨 Un email de bienvenue vous a été envoyé. Redirection en cours...",
  "user": {
    "id": 2,
    "prenom": "Marie",
    "nom": "Martin",
    "email": "marie.martin@test.com"
  }
}
```

---

##  3. INSCRIPTION AVEC TOUS LES SPORTS

```bash
curl -X POST "$BASE_URL/Auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "prenom": "Emma",
    "nom": "Dubois",
    "email": "emma.dubois@test.com",
    "password": "Password123!",
    "newsletterPreferences": {
      "subscribed": true,
      "categories": {
        "sport": true,
        "evenements": false,
        "billets": false
      },
      "sports": [
        {"id": "natation", "name": "Natation"},
        {"id": "athletisme", "name": "Athlétisme"},
        {"id": "basketball", "name": "Basketball"},
        {"id": "surf", "name": "Surf"},
        {"id": "gymnastique", "name": "Gymnastique"}
      ]
    }
  }' | jq '.'
```

---

## 4. INSCRIPTION NEWSLETTER PARTIELLE (Sport + Billets uniquement)

```bash
curl -X POST "$BASE_URL/Auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "prenom": "Sophie",
    "nom": "Bernard",
    "email": "sophie.bernard@test.com",
    "password": "Password123!",
    "newsletterPreferences": {
      "subscribed": true,
      "categories": {
        "sport": true,
        "evenements": false,
        "billets": true
      },
      "sports": [
        {"id": "surf", "name": "Surf"}
      ]
    }
  }' | jq '.'
```

---

## 5. INSCRIPTION UNIQUEMENT SPORT NATATION

```bash
curl -X POST "$BASE_URL/Auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "prenom": "Thomas",
    "nom": "Petit",
    "email": "thomas.petit@test.com",
    "password": "Password123!",
    "newsletterPreferences": {
      "subscribed": true,
      "categories": {
        "sport": true,
        "evenements": false,
        "billets": false
      },
      "sports": [
        {"id": "natation", "name": "Natation"}
      ]
    }
  }' | jq '.'
```

---

## 6. INSCRIPTION ÉVÉNEMENTS + BILLETS (Sans Sports)

```bash
curl -X POST "$BASE_URL/Auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "prenom": "Lucas",
    "nom": "Robert",
    "email": "lucas.robert@test.com",
    "password": "Password123!",
    "newsletterPreferences": {
      "subscribed": true,
      "categories": {
        "sport": false,
        "evenements": true,
        "billets": true
      },
      "sports": []
    }
  }' | jq '.'
```

---

## 7. INSCRIPTION NON ABONNÉ

```bash
curl -X POST "$BASE_URL/Auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "prenom": "Antoine",
    "nom": "Leroy",
    "email": "antoine.leroy@test.com",
    "password": "Password123!",
    "newsletterPreferences": {
      "subscribed": false,
      "categories": {
        "sport": false,
        "evenements": false,
        "billets": false
      },
      "sports": []
    }
  }' | jq '.'
```

---

##  8. CONNEXION

```bash
curl -X POST "$BASE_URL/Auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "marie.martin@test.com",
    "password": "Password123!"
  }' | jq '.'
```

**Sauvegarder le token :**
```bash
TOKEN=$(curl -X POST "$BASE_URL/Auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "marie.martin@test.com",
    "password": "Password123!"
  }' -s | jq -r '.token')

echo "Token sauvegardé : $TOKEN"
```

---

## 9. RÉCUPÉRER L'UTILISATEUR ACTUEL (Authentifié)

```bash
curl -X GET "$BASE_URL/Auth/current" \
  -H "Authorization: Bearer $TOKEN" | jq '.'
```

---

## 10. TESTS D'ERREUR

### Email déjà existant
```bash
curl -X POST "$BASE_URL/Auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "prenom": "Duplicate",
    "nom": "User",
    "email": "marie.martin@test.com",
    "password": "Password123!"
  }' | jq '.'
```

**Résultat attendu :** Status 400
```json
{
  "success": false,
  "message