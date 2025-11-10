#!/usr/bin/env bash
set -euo pipefail

# Emplacement des logs
LOGFILE=/var/log/scheduler/cron.log
mkdir -p "$(dirname "$LOGFILE")" || true

# Variables attendues (peuvent venir du docker-compose / .env)
API_URL=${API_URL:-http://api:80}
NEWSLETTER_API_TOKEN=${NEWSLETTER_API_TOKEN:-}
TZ=${TZ:-Europe/Paris}

export TZ

# Fonction pour envoyer immédiatement la newsletter (utilise send_now.sh)
send_now() {
  echo "[$(date -Is)] Lancement d'un envoi immédiat..." | tee -a "$LOGFILE"
  /usr/local/bin/send_now.sh "$API_URL" "$NEWSLETTER_API_TOKEN" 2>&1 | tee -a "$LOGFILE"
  echo "[$(date -Is)] Fin de l'envoi immédiat." | tee -a "$LOGFILE"
}

# Si on passe l'argument "send-now", on exécute l'envoi immédiat et on quitte
if [ "${1:-}" = "send-now" ] || [ "${1:-}" = "--send-now" ]; then
  send_now
  exit 0
fi

# Démarrer crond en foreground en mode verbose pour logs
echo "[$(date -Is)] Démarrage de cron (foreground). Logs -> $LOGFILE"
# Lancer crond (en tant que processus principal) ; la config cron est dans /etc/crontabs/root
# On redirige la sortie vers le fichier de log.
crond -f -l 8 >> "$LOGFILE" 2>&1
