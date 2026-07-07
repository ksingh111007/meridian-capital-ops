#!/usr/bin/env bash
# Build the backend API image in ACR (cloud build — no local Docker needed)
# and point the environment's web app at the new tag.
#
#   scripts/deploy-backend.sh <dev|prod>
set -euo pipefail
source "$(dirname "$0")/lib.sh"

TAG="${TAG:-$(git rev-parse --short HEAD)}"

echo "==> az acr build ${ACR_NAME} meridian-api:${TAG} (backend/)"
az acr build \
  --registry "$ACR_NAME" \
  --image "meridian-api:${TAG}" \
  --image meridian-api:latest \
  "${REPO_ROOT}/backend/"

echo "==> Pointing ${API_APP_NAME} at meridian-api:${TAG}"
az webapp config container set \
  --resource-group "$RESOURCE_GROUP" \
  --name "$API_APP_NAME" \
  --container-image-name "${ACR_NAME}.azurecr.io/meridian-api:${TAG}" \
  --output none
az webapp restart --resource-group "$RESOURCE_GROUP" --name "$API_APP_NAME" --output none

echo "Deployed: https://${API_APP_NAME}.azurewebsites.net (health: /healthz, Swagger: /swagger)"
