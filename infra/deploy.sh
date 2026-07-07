#!/usr/bin/env bash
# Provision (or update — it's idempotent) one environment of the Meridian stack.
#
#   ./deploy.sh dev            # rg-meridian-dev in eastus2
#   ./deploy.sh prod westus3   # rg-meridian-prod in westus3
#
# Prerequisites (see README.md): az CLI logged in to the right subscription,
# and env/<env>.parameters.json edited with your Entra SQL-admin group.
# Override the defaults with RESOURCE_GROUP=... and/or BASE_NAME=... env vars.
set -euo pipefail
cd "$(dirname "$0")"

ENVIRONMENT="${1:?usage: deploy.sh <dev|prod> [location]}"
LOCATION="${2:-eastus2}"
BASE_NAME="${BASE_NAME:-meridian}"
RESOURCE_GROUP="${RESOURCE_GROUP:-rg-${BASE_NAME}-${ENVIRONMENT}}"
PARAMS="env/${ENVIRONMENT}.parameters.json"

[[ -f "$PARAMS" ]] || { echo "Unknown environment '${ENVIRONMENT}' — no ${PARAMS}." >&2; exit 1; }
if grep -q '00000000-0000-0000-0000-000000000000' "$PARAMS"; then
  echo "Edit ${PARAMS} first: set sqlAdminLogin / sqlAdminObjectId to your Entra ID" >&2
  echo "admin group (az ad group show --group <name> --query id -o tsv)." >&2
  exit 1
fi

echo "==> Resource group ${RESOURCE_GROUP} (${LOCATION})"
az group create --name "$RESOURCE_GROUP" --location "$LOCATION" --output none

echo "==> Deploying main.bicep with ${PARAMS}"
az deployment group create \
  --resource-group "$RESOURCE_GROUP" \
  --name "meridian-${ENVIRONMENT}" \
  --template-file main.bicep \
  --parameters "@${PARAMS}" \
  --query 'properties.outputs' --output json

cat <<EOF

Provisioned. First-time next steps for '${ENVIRONMENT}' (details in README.md):
  1. scripts/deploy-backend.sh  ${ENVIRONMENT}   # build + push the API image, point the app at it
  2. scripts/deploy-frontend.sh ${ENVIRONMENT}   # same for the Next.js frontend
  3. scripts/deploy-database.sh ${ENVIRONMENT} --allow-my-ip   # dacpac: schema + seed
  4. scripts/grant-api-db-access.sh ${ENVIRONMENT}             # let the API's managed identity into the DB
EOF
