#!/usr/bin/env bash
# Shared naming for the per-component deploy scripts. Must match main.bicep:
# every name derives from BASE_NAME + the environment. Override BASE_NAME
# and/or RESOURCE_GROUP via env vars if you changed baseName in env/*.json.

ENVIRONMENT="${1:?usage: $(basename "$0") <dev|prod>}"
case "$ENVIRONMENT" in dev|prod) ;; *) echo "environment must be dev or prod" >&2; exit 1;; esac

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
BASE_NAME="${BASE_NAME:-meridian}"
RESOURCE_GROUP="${RESOURCE_GROUP:-rg-${BASE_NAME}-${ENVIRONMENT}}"
ACR_NAME="acr${BASE_NAME}${ENVIRONMENT}"
API_APP_NAME="app-${BASE_NAME}-api-${ENVIRONMENT}"
WEB_APP_NAME="app-${BASE_NAME}-web-${ENVIRONMENT}"
SQL_SERVER_NAME="sql-${BASE_NAME}-${ENVIRONMENT}"
SQL_SERVER_FQDN="${SQL_SERVER_NAME}.database.windows.net"
