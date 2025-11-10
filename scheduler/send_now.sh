#!/usr/bin/env bash
set -euo pipefail

API_URL=${1:-http://api:80}
API_TOKEN=${2:-}

# Endpoint par défaut : adapter selon ton API réelle
# Exemple d'endpoint attendu : POST /api/newsletter/send or /newsletter/send
ENDPOINT="${API_URL%/}/api/newsletter/send"

# Corps JSON minimal (adapter selon ton API)
PAYLOAD='{"subject":"Test newsletter","body":"Envoi de test (envoyé via scheduler)","test":true}'

# Construction de la commande curl avec ou sans token
if [ -n "$API_TOKEN" ]; then
  curl -sS -X POST "$ENDPOINT" \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer $API_TOKEN" \
    -d "$PAYLOAD"
else
  curl -sS -X POST "$ENDPOINT" \
    -H "Content-Type: application/json" \
    -d "$PAYLOAD"
fi
