#!/usr/bin/env bash
# Build the dacpac and publish schema + seed to the environment's Azure SQL
# database. Repeatable: the publish profile blocks destructive changes and the
# seed only inserts into empty tables.
#
#   scripts/deploy-database.sh <dev|prod> [--allow-my-ip]
#
# --allow-my-ip adds a temporary server firewall rule for this machine's
# public IP (sqlpackage connects directly; the Bicep default only lets Azure
# services through). Requires: dotnet 8 SDK, sqlpackage
# (dotnet tool install -g microsoft.sqlpackage), and an identity with access
# to the database — the Entra admin group from env/<env>.parameters.json works.
set -euo pipefail
source "$(dirname "$0")/lib.sh"

if [[ "${2:-}" == "--allow-my-ip" ]]; then
  MY_IP="$(curl -fsS https://api.ipify.org)"
  echo "==> Firewall rule 'deployer' for ${MY_IP} on ${SQL_SERVER_NAME}"
  az sql server firewall-rule create \
    --resource-group "$RESOURCE_GROUP" --server "$SQL_SERVER_NAME" \
    --name deployer --start-ip-address "$MY_IP" --end-ip-address "$MY_IP" \
    --output none
fi

echo "==> Building the dacpac"
dotnet build "${REPO_ROOT}/database/Meridian.Database.sqlproj" -c Release

echo "==> Publishing to ${SQL_SERVER_FQDN}/meridian"
sqlpackage /Action:Publish \
  /SourceFile:"${REPO_ROOT}/database/bin/Release/Meridian.Database.dacpac" \
  /Profile:"${REPO_ROOT}/database/profiles/Azure.publish.xml" \
  /TargetConnectionString:"Server=tcp:${SQL_SERVER_FQDN},1433;Database=meridian;Authentication=Active Directory Default;Encrypt=True;"

echo "Published. If this is a fresh database, run scripts/grant-api-db-access.sh ${ENVIRONMENT} next."
