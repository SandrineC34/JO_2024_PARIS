#!/bin/bash

API_URL=${API_URL:-http://api:80}
TOKEN=${NEWSLETTER_API_TOKEN:-changeme}
LOG_FILE="/var/log/scheduler/report.log"
DATE=$(date '+%Y-%m-%d %H:%M:%S')

echo "===== [$DATE] Lancement du job newsletter =====" >> "$LOG_FILE"

# Simulation d'appel API
response=$(curl -s -o /dev/null -w "%{http_code}" -H "Authorization: Bearer $TOKEN" "$API_URL/newsletter/send")

if [ "$response" -eq 200 ]; then
  echo "[$DATE] Envoi réussi" >> "$LOG_FILE"
else
  echo "[$DATE] Échec de l'envoi (code HTTP: $response)" >> "$LOG_FILE"
fi

# Exemple : compter le nombre de mails "envoyés" (simulation locale)
# Tu peux adapter selon la réponse JSON de ton API
MAIL_COUNT=$(curl -s "$API_URL/newsletter/count" | grep -o '[0-9]*' || echo 0)
echo "[$DATE] Nombre de mails envoyés : $MAIL_COUNT" >> "$LOG_FILE"

echo "[$DATE] Fin du job newsletter." >> "$LOG_FILE"
echo "" >> "$LOG_FILE"


# Lancer un envoi sans attendre CRON
# docker exec jo2024_scheduler /app/send-newsletter.sh
# Visualisation rapport : ./scheduler_logs/report.log
# Les logs cron : ./scheduler_logs/cron.log