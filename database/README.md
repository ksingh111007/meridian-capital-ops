# Meridian Capital Ops — database

SDK-style SQL project ([Microsoft.Build.Sql](https://github.com/microsoft/DacFx))
that builds to a **dacpac** and deploys to **Azure SQL Database**. It owns the
schema and the seed data; the API (`../backend`) points at the deployed database
with `Database:Provider=SqlServer` and never creates or migrates objects itself.

```
database/
├── Meridian.Database.sqlproj      # dotnet build → bin/<cfg>/Meridian.Database.dacpac
├── src/
│   ├── Schemas/                   # ref · ops · admin · audit · portal · hist
│   └── <schema>/Tables/*.sql      # one system-versioned table per file (generated)
├── scripts/
│   ├── Script.PostDeployment.sql  # :r includes, SQLCMD
│   └── seed/*.sql                 # idempotent seed = the frontend mock story (generated)
├── tools/
│   ├── SchemaExport/              # prints the EF model as a CREATE script (source of truth)
│   ├── generate-tables.py         # CREATE script → temporal table files under src/
│   └── generate-seed.mjs          # ../meridian-capital-ops/src/mocks/*.json → scripts/seed/
├── profiles/Azure.publish.xml     # conservative sqlpackage publish profile
└── infra/
    ├── azure-sql.bicep            # Azure SQL server + serverless database (Entra-only auth)
    └── github/deploy-database.yml # CI: build → drift check → sqlpackage publish
```

## Design

- **No dbo.** Objects live in purpose schemas:
  `ref` (funds, deals, investors, borrowers, currencies), `ops` (capital calls,
  distributions, wires, recon, treasury, workflow, KPI snapshots), `admin`
  (staff, roles, integrations, notification rules), `audit` (the hash-chained
  event log), `portal` (LP contacts, capital accounts, documents), and `hist`
  (temporal history, managed by SQL Server).
- **Every table is temporal** (`SYSTEM_VERSIONING = ON`) with a named history
  table `hist.<schema>_<Table>`. The `ValidFrom`/`ValidTo` period columns are
  `HIDDEN`, so `SELECT *`, Dapper materialization, and the seed inserts are
  unaffected; point-in-time reads use `FOR SYSTEM_TIME AS OF`.
- **Audit columns on every table**: `CreatedAtUtc`, `CreatedBy`, `ModifiedAtUtc`,
  `ModifiedBy` — stamped by the API's EF save interceptor for application
  writes, with SQL defaults covering seed/out-of-band inserts.
- **`IsActive` flag on every table** (default 1) to soft-signal whether the row
  is an active entity.
- KPI strips whose figures are published reporting outputs (not sums over the
  seeded rows — the mock story is deliberately illustrative) live in
  `ops.KpiSnapshots` (screen × metric) and per-screen snapshot tables
  (`ops.PortfolioSnapshots`, `ops.CashPositionSnapshots`).

## The schema is generated from the EF model

The API's `AppDbContext` is the single source of truth for tables, columns,
keys, and indexes — the SQL project adds the temporal/systemic parts EF doesn't
model. After changing the EF model:

```bash
cd database/tools
dotnet run --project SchemaExport > /tmp/efcreate.sql
python3 generate-tables.py /tmp/efcreate.sql
node generate-seed.mjs          # column lists come from src/, so drift fails loudly
cd .. && dotnet build           # validate the project still compiles to a dacpac
```

Check the diff of `src/` and `scripts/` before committing.

## Seed data

`scripts/seed/*.sql` is generated from `../meridian-capital-ops/src/mocks/*.json`
— the same internally consistent sample story the frontend's mock mode uses
(business "today" = **2026-07-05**; set `BusinessDate` on the API to match for
demos). Everything is idempotent (each table seeds only when empty), and the
`audit.Events` seals are computed with the backend's exact SHA-256 chain, so
`GET /api/admin/audit` reports `chainValid: true` against seeded data. One
addition over the mocks: staff user `u-admin` (Administrator), the frontend's
default dev principal.

Do **not** edit `src/**/Tables/*.sql` or `scripts/seed/*.sql` by hand — change
the EF model or the mocks and regenerate.

## Build & deploy

```bash
cd database
dotnet build Meridian.Database.sqlproj -c Release   # → bin/Release/Meridian.Database.dacpac

# one-time infra (creates sql-<name>-<env> + the serverless "meridian" database)
az deployment group create -g rg-meridian-dev -f infra/azure-sql.bicep \
  -p baseName=meridian sqlAdminLogin=<entra-admin-group> sqlAdminObjectId=<objectId>

# deploy schema + seed (repeatable; publish profile blocks destructive changes)
sqlpackage /Action:Publish \
  /SourceFile:bin/Release/Meridian.Database.dacpac \
  /Profile:profiles/Azure.publish.xml \
  /TargetConnectionString:"Server=tcp:<sqlServerFqdn>,1433;Database=meridian;Authentication=Active Directory Default;Encrypt=True;"
```

`infra/github/deploy-database.yml` is the CI version (build → seed drift check →
publish); move it to `.github/workflows/` to activate.

## Point the API at it

```bash
Database__Provider=SqlServer
ConnectionStrings__Default="Server=tcp:<sqlServerFqdn>,1433;Database=meridian;Authentication=Active Directory Default;Encrypt=True;"
BusinessDate=2026-07-05    # optional: pin the story's "today" for demo parity
```

Without these the backend keeps its self-seeded in-memory SQLite store (dev &
tests), which serves the same story via `StorySeed` + `MockDataSeed`.
