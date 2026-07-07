#!/usr/bin/env bash
# Create a contained database user for the API web app's system-assigned
# managed identity and grant it read/write. Run once per environment, as a
# member of the Entra SQL-admin group (schema changes stay with the dacpac,
# so the API needs no DDL rights).
#
#   scripts/grant-api-db-access.sh <dev|prod>
#
# Uses sqlcmd (go-sqlcmd or the classic ODBC one) if installed; otherwise
# prints the T-SQL to paste into the Azure portal query editor.
set -euo pipefail
source "$(dirname "$0")/lib.sh"

SQL="
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = '${API_APP_NAME}')
    CREATE USER [${API_APP_NAME}] FROM EXTERNAL PROVIDER;
ALTER ROLE db_datareader ADD MEMBER [${API_APP_NAME}];
ALTER ROLE db_datawriter ADD MEMBER [${API_APP_NAME}];
"

if command -v sqlcmd >/dev/null 2>&1; then
  echo "==> Granting ${API_APP_NAME} access on ${SQL_SERVER_FQDN}/meridian"
  sqlcmd -S "tcp:${SQL_SERVER_FQDN},1433" -d meridian \
      --authentication-method ActiveDirectoryDefault -Q "$SQL" 2>/dev/null \
    || sqlcmd -S "tcp:${SQL_SERVER_FQDN},1433" -d meridian -G -Q "$SQL"
  echo "Done."
else
  echo "sqlcmd not found. Run this against the 'meridian' database as the Entra admin"
  echo "(e.g. Azure portal → SQL database → Query editor):"
  echo "$SQL"
fi
