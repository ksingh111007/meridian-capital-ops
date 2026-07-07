#!/usr/bin/env bash
# Build the Next.js frontend image in ACR (cloud build — no local Docker
# needed) and point the environment's web app at the new tag.
#
#   scripts/deploy-frontend.sh <dev|prod>
set -euo pipefail
source "$(dirname "$0")/lib.sh"

TAG="${TAG:-$(git rev-parse --short HEAD)}"

echo "==> az acr build ${ACR_NAME} meridian-web:${TAG} (meridian-capital-ops/)"
az acr build \
  --registry "$ACR_NAME" \
  --image "meridian-web:${TAG}" \
  --image meridian-web:latest \
  "${REPO_ROOT}/meridian-capital-ops/"

echo "==> Pointing ${WEB_APP_NAME} at meridian-web:${TAG}"
az webapp config container set \
  --resource-group "$RESOURCE_GROUP" \
  --name "$WEB_APP_NAME" \
  --container-image-name "${ACR_NAME}.azurecr.io/meridian-web:${TAG}" \
  --output none
az webapp restart --resource-group "$RESOURCE_GROUP" --name "$WEB_APP_NAME" --output none

echo "Deployed: https://${WEB_APP_NAME}.azurewebsites.net (staff: /portfolio, LP portal: /portal)"
