# Meridian Capital Ops — infrastructure

Azure deployment for the whole stack — frontend (`../meridian-capital-ops`,
Next.js container), backend (`../backend`, .NET API container), and database
(`../database`, dacpac → Azure SQL) — in **two isolated environments, `dev`
and `prod`**, each in its own resource group. This folder supersedes the old
per-component `backend/infra/` and `database/infra/`.

```
infra/
├── main.bicep                 # the whole environment: ACR, plan, 2 web apps, SQL, monitoring
├── modules/
│   ├── web-app.bicep          # Linux Web App for Containers (used for api + web)
│   └── sql.bicep              # SQL server + "meridian" database (Entra-only auth)
├── env/
│   ├── dev.parameters.json    # B1 plan · serverless auto-pause SQL (cost-optimized)
│   └── prod.parameters.json   # P1v3 plan · provisioned SQL · geo-redundant backups
├── deploy.sh                  # provision one environment (idempotent)
├── scripts/
│   ├── deploy-backend.sh      # ACR cloud-build the API image + roll the web app
│   ├── deploy-frontend.sh     # same for the Next.js image
│   ├── deploy-database.sh     # dacpac publish (schema + seed)
│   ├── grant-api-db-access.sh # one-time: let the API's managed identity into the DB
│   └── lib.sh                 # shared naming (must match main.bicep)
└── github/                    # CI/CD workflows — move to .github/workflows/ to activate
```

## What gets provisioned (per environment)

| Resource | dev | prod |
| --- | --- | --- |
| Resource group | `rg-meridian-dev` | `rg-meridian-prod` |
| Container registry | `acrmeridiandev` (Basic) | `acrmeridianprod` (Basic) |
| App Service plan (Linux) | `plan-meridian-dev` — B1 | `plan-meridian-prod` — P1v3, Always On |
| Frontend web app | `app-meridian-web-dev` | `app-meridian-web-prod` |
| Backend web app | `app-meridian-api-dev` | `app-meridian-api-prod` |
| SQL server + `meridian` DB | `sql-meridian-dev` — GP_S_Gen5_1 serverless, auto-pause 1h, local backups | `sql-meridian-prod` — GP_Gen5_2 provisioned, geo backups |
| Log Analytics + App Insights | 30-day retention | 90-day retention |

Wiring done for you: the frontend gets `MERIDIAN_API_URL` pointed at the API
app; the API gets `Database__Provider=SqlServer`, the Entra-auth connection
string, App Insights, and `BusinessDate=2026-07-05` (demo parity with the
seeded story — set the `businessDate` parameter to `''` to use the real
clock). Both apps pull images from ACR with their managed identities; the API
reaches SQL with its managed identity (no passwords anywhere).

## Azure prerequisites (one-time)

1. **An Azure subscription and the az CLI** (`az login`, then
   `az account set --subscription <id>`). You also need the **dotnet 8 SDK**
   and **sqlpackage** (`dotnet tool install -g microsoft.sqlpackage`) for the
   database deploy. Local Docker and Node are *not* needed — images build in
   Azure via `az acr build`.
2. **Permissions**: `Owner` on the subscription (or `Contributor` **+**
   `User Access Administrator` / `Role Based Access Control Administrator`) —
   the template creates role assignments (AcrPull for the web apps), which
   plain Contributor cannot.
3. **An Entra ID group to administer SQL** (both servers are Entra-only —
   no SQL logins). Create one and put yourself in it:
   ```bash
   az ad group create --display-name meridian-sql-admins --mail-nickname meridian-sql-admins
   az ad group member add --group meridian-sql-admins \
     --member-id $(az ad signed-in-user show --query id -o tsv)
   az ad group show --group meridian-sql-admins --query id -o tsv   # → sqlAdminObjectId
   ```
   Put the group name and that object id into `env/dev.parameters.json` and
   `env/prod.parameters.json` (`deploy.sh` refuses to run until you do).
4. **Resource providers registered** on the subscription (usually already
   done; first-timers run once):
   ```bash
   az provider register -n Microsoft.Web -n Microsoft.Sql \
     -n Microsoft.ContainerRegistry -n Microsoft.OperationalInsights -n Microsoft.Insights
   ```
5. **Globally unique names**: `baseName=meridian` derives the ACR
   (`acrmeridiandev`), web app, and SQL server names, all of which are global
   namespaces. If a deployment fails with a name conflict, change `baseName`
   in both `env/*.parameters.json` files and pass `BASE_NAME=<yours>` to the
   scripts.

## First deploy (per environment)

```bash
cd infra
./deploy.sh dev                          # 1. provision rg-meridian-dev (≈5 min; SQL is the slow part)
scripts/deploy-backend.sh  dev           # 2. build + push meridian-api, roll the app
scripts/deploy-frontend.sh dev           # 3. build + push meridian-web, roll the app
scripts/deploy-database.sh dev --allow-my-ip   # 4. dacpac: schema + seed (adds a firewall rule for your IP)
scripts/grant-api-db-access.sh dev       # 5. contained DB user for the API's managed identity
```

Then open `https://app-meridian-web-dev.azurewebsites.net/portfolio` (staff)
or `/portal` (LP view). Repeat with `prod` for the second environment.

Notes:

- Until step 2/3 push the first images, the web apps show a startup error —
  expected, the ACR is empty at provision time.
- Step 4 runs as *you* (a member of the SQL-admin group). Step 5 must run
  after step 2 created the web app **and** step 4 created the schema; it's
  idempotent, as are all five steps.
- Re-running `./deploy.sh <env>` applies template changes incrementally;
  day-to-day code deploys are just steps 2–4 (or CI below).

## CI/CD (GitHub Actions)

Copy `github/*.yml` to `.github/workflows/`. Pushes to `main` deploy **dev**
automatically (path-filtered per component); **prod** deploys are manual
`workflow_dispatch` runs — add a required-reviewer protection rule on the
`prod` GitHub environment to gate them. `infra` (Bicep) deploys are
manual-only for both.

One-time GitHub setup:

1. An Entra **app registration with OIDC federated credentials** for this
   repo (no stored secrets):
   ```bash
   az ad app create --display-name meridian-github-deploy
   az ad sp create --id <appId>
   az role assignment create --assignee <appId> --role Contributor \
     --scope /subscriptions/<subId>/resourceGroups/rg-meridian-dev   # and ...-prod
   az ad app federated-credential create --id <appId> --parameters '{
     "name": "meridian-main",
     "issuer": "https://token.actions.githubusercontent.com",
     "subject": "repo:<owner>/<repo>:environment:dev",
     "audiences": ["api://AzureADTokenExchange"]
   }'   # repeat with environment:prod
   ```
   For the database workflow, also add the service principal to the
   `meridian-sql-admins` group (or create it as a DB user with `db_owner`) so
   sqlpackage can publish. For the infra workflow, the identity needs the
   role-assignment rights from prerequisite 2.
2. Two **GitHub environments**, `dev` and `prod`, each with:
   - secrets `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`
   - variables `AZURE_RESOURCE_GROUP`, `AZURE_LOCATION`, `ACR_NAME`,
     `API_WEBAPP_NAME`, `WEB_WEBAPP_NAME`, `SQL_SERVER_FQDN`, `SQL_DATABASE`
     (values per the table above).

## Security

- **The API still uses the `X-User-Id` dev stand-in for authentication**
  (backend/README.md § AuthN) — anyone who can reach it can impersonate any
  user. Fine for a dev/demo environment; **do not put real data on `prod`
  until real SSO lands** (BACKEND_TODO step 1). Interim hardening options:
  App Service access restrictions on the API app, or built-in App Service
  authentication (Easy Auth) in front of both apps.
- SQL is Entra-only (no passwords), TLS 1.2+, firewall open to Azure services
  only (plus the temporary `deployer` rule `deploy-database.sh --allow-my-ip`
  creates — delete it when done). For production isolation, move to private
  endpoints + VNet integration.
- The web apps are HTTPS-only, FTPS-disabled, and pull from ACR via managed
  identity; ACR admin credentials are disabled.

## Teardown

An environment is exactly one resource group:

```bash
az group delete --name rg-meridian-dev
```
