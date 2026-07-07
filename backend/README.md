# Meridian Capital Ops — backend

ASP.NET Core 8 (LTS) API for the fund-operations platform. Implements the
**full contract** in [`../meridian-capital-ops/docs/API.md`](../meridian-capital-ops/docs/API.md):
the capital-call approval pipeline, distributions, portfolio + deal drill-downs,
fund ops (drawdowns, wires, cash, reconciliation), the admin surface, the
investor portal, the computed needs-attention inbox, the hash-chained audit log,
background automation, and an OData query surface. Persistence is Azure SQL
(schema + seed owned by the [`../database`](../database) dacpac project) or a
self-seeded in-memory SQLite store for dev/tests.

```
backend/
├── src/
│   ├── Meridian.Domain/          # entities + pure business rules, zero dependencies
│   ├── Meridian.Application/     # use-case services, DTOs, ports (abstractions)
│   ├── Meridian.Infrastructure/  # EF Core + Dapper, seeding, Quartz jobs, external-service adapters
│   └── Meridian.Api/             # controllers, auth/RBAC, OData, Swagger, ProblemDetails
├── tests/
│   ├── Meridian.Domain.Tests/            # unit tests for the business rules
│   └── Meridian.Api.IntegrationTests/    # the emphasis: full-pipeline tests via WebApplicationFactory
├── http/                         # .http request files (VS Code REST Client / VS / Rider)
├── Dockerfile · docker-compose.yml
└── MeridianCapital.sln
```

## Run it

```bash
cd backend
dotnet test                                   # 93 tests: domain unit + API integration
dotnet run --project src/Meridian.Api        # http://localhost:8080 (Swagger at /swagger)
# or
docker compose up --build
```

Run as above and the API self-hosts an in-memory SQLite store. To run against
**Azure SQL** (schema + seed deployed by the [`../database`](../database)
dacpac project) set two settings and start it the same way:

```bash
Database__Provider=SqlServer
ConnectionStrings__Default="Server=tcp:<server>.database.windows.net,1433;Database=meridian;Authentication=Active Directory Default;Encrypt=True;"
```

Authenticate requests with `X-User-Id: <seeded staff user>` (see below), or a
portal contact id (e.g. `pc-1`) for the `/api/portal/*` endpoints. Try the
request files in `http/` — e.g. `http/capital-calls.http` walks create → approve →
reject, `http/admin-ops.http` fires background jobs on demand.

In `Development` the **business date is pinned to 2026-07-05** (`BusinessDate` in
`appsettings.Development.json`) so the seeded story matches the frontend mocks —
same trick as `TODAY` in the frontend's `format.ts`.

## Architecture

Clean architecture with dependencies pointing inward
(`Api → Infrastructure → Application → Domain`):

- **Domain** — entities and the pure rules from
  [`docs/BUSINESS_RULES.md`](../meridian-capital-ops/docs/BUSINESS_RULES.md):
  `AllocationCalculator` (pro-rata by unfunded/commitment with largest-remainder
  rounding so allocations always reconcile exactly), `ApprovalWorkflow` (stage
  transitions, comment-required, auto-advance stages, terminal completion),
  `EscalationEvaluator` (> $20M → CIO + Compliance sign-off with a stage gate),
  `AuditSealer` (SHA-256 hash chain, tamper-evident).
- **Application** — one service per feature, DTOs shaped 1:1 to the frontend's
  `src/lib/types.ts`, and **ports** for everything that leaves the process:
  `IWireGateway`, `ICustodianFeed`, `IDocumentStorage`, `INotificationService`,
  plus `IAppDbContext`/`IDbConnectionFactory`/`IClock`.
- **Infrastructure** — adapter implementations. External integrations live in
  `ExternalServices/` as stubs behind the ports; swapping in a real SWIFT
  gateway or custodian feed is a DI registration, nothing above moves.
- **Api** — controller-based endpoints only (no minimal APIs), RFC 7807
  ProblemDetails errors, Swagger, health checks at `/healthz`.

### Data: Azure SQL (dacpac-owned) or in-memory SQLite, behind one EF model

Persistence is EF Core + Dapper with a config-selected provider
(`Database:Provider`):

- **`SqlServer` — Azure SQL.** The schema is owned by the
  [`../database`](../database) SQL project (dacpac): purpose schemas
  (`ref`/`ops`/`admin`/`audit`/`portal`, never dbo), every table system-versioned
  (temporal, history in `hist`) with audit columns and an `IsActive` flag, seed
  data deployed post-deployment. The API only reads/writes — it never creates,
  migrates, or seeds objects on this provider. Native `decimal(18,2)` columns.
- **SQLite (default) — dev & tests.** Shared-cache in-memory store created via
  `EnsureCreated()` and seeded on boot, so `dotnet run` works with nothing else
  installed. Decimals are stored as `REAL` (lossless for 2-dp USD millions);
  schemas are ignored (SQLite has none) but table names match the SQL project.

The EF model is the single source of truth for both: the database project's
table files are generated from `AppDbContext`
(`database/tools/generate-tables.py`), so the two stores cannot drift. Every
table also carries shadow **audit columns** (`CreatedAtUtc/CreatedBy/
ModifiedAtUtc/ModifiedBy`, plus `IsActive`) stamped by a save interceptor with
the authenticated principal.

**Dapper** is used where hand-written SQL beats the ORM: the needs-attention
inbox (`NeedsAttentionService`) aggregates across calls/allocations/stages/wires/
recon/integrations per caller; `IDbConnectionFactory.Table(schema, name)` keeps
that SQL portable across the two providers. Writes always go through EF Core so
change tracking + audit stay intact.

SQLite seed data = the frontend mock story (`StorySeed` + `MockDataSeed`, the
latter reading embedded copies of `src/mocks/*.json`: #C-2041 at Legal, #C-2039
returned with two overdue wires, #D-119 with Blocked/Exception payouts, wires/
recon/treasury/portal read models, the staff roster and role matrix) plus
deterministic **Bogus** volume data (`FakeDataSeed`, fixed seed → identical rows
in every run and test host). The Azure SQL seed is generated from the same mock
JSONs (`database/tools/generate-seed.mjs`), so both stores serve the same story.

### AuthN / AuthZ

- **Authentication** is a dev stand-in: `X-User-Id` header → seeded staff user
  (`HeaderAuthenticationHandler`). It is one isolated handler behind the
  standard ASP.NET authentication pipeline; replacing it with OIDC/Entra SSO is
  step 1 of [`docs/BACKEND_TODO.md`](../meridian-capital-ops/docs/BACKEND_TODO.md)
  and touches nothing else. **Do not ship the header scheme.**
- **Authorization is real**: the role → capability matrix (Blotter/Approvals/
  Wires/Recon/Ref Data/Admin → none < view < edit < approve < full) is enforced
  server-side on every endpoint via `[RequireCapability(module, level)]`
  policies, with a fallback authenticated-user policy so nothing is open by
  accident. Stage-approver matching and escalation sign-offs are enforced again
  inside the workflow domain logic.

Seeded users: `u-jchen` (Ops Analyst) · `u-mreyes` (Deal Lead) · `u-spatel`
(CIO) · `u-jokafor` (Counsel) · `u-talvarez` (Ops Manager) · `u-dwhitfield`
(Fund Accountant) · `u-pnair` (Compliance) · `u-admin` (Administrator).

### Cross-cutting guarantees

- **Every mutation appends** to the global **hash-chained audit log**
  (`seal_n = H(seal_{n-1} ‖ event_n)`); `GET /api/admin/audit` re-verifies the
  chain on read (`kpis.chainValid`). Each event records the seal it chains from
  under a unique index, so concurrent API instances (or a retried commit) on the
  shared Azure SQL database cannot silently fork the chain.
- Approvals **notify the next approver** through the notification port (default
  adapter writes an outbox row + log).
- Business errors are typed (`DomainException` Validation/NotFound/Forbidden/
  Conflict) and map to 400/404/403/409 ProblemDetails.
- No `DateTime.Now` in business code — `IClock` everywhere.

### Background automation (Quartz)

Three durable jobs, cron-scheduled from config (`Jobs:*`) and runnable on demand
via `POST /api/ops/jobs/{name}/run` (Admin:edit):

| Job | What it does |
| --- | --- |
| `overdue-allocation-sweep` | Flips unpaid allocations past due date to `Overdue`, audits + notifies (BUSINESS_RULES § Overdue calls) |
| `approval-sla-monitor` | Fires "Approval overdue" notifications for stages past their SLA (scaffold: no dedupe store yet) |
| `custodian-feed-sync` | Pulls the custodian snapshot through the `ICustodianFeed` port — the recon auto-match engine plugs in here |

### OData

`/odata/Deals` is a full OData entity set (`$filter`, `$orderby`, `$top`,
`$select`, `$count`; queries compose into SQL through EF Core) — the
server-side filtering/pagination story for production-volume books. Pattern
generalizes to more entity sets by adding them to the EDM in `Program.cs`.

## Endpoints implemented (of docs/API.md)

The **full contract**:

`GET /api/me` · `GET /api/portfolio/summary` · `GET /api/deals[/{id}]` ·
`GET /api/needs-attention` · `GET/POST /api/capital-calls` ·
`GET /api/capital-calls/{id}` · `POST /api/capital-calls/{id}/approve|reject` ·
`GET /api/workflows/capital-calls` · `GET /api/distributions[/{id}]` ·
`GET /api/drawdowns` · `GET /api/wires` · `POST /api/wires/{id}/retry` ·
`GET /api/cash/position` · `GET /api/reconciliation` ·
`POST /api/reconciliation/{id}/assign` · `GET /api/admin/users|funds|investors|
reference|integrations|notification-rules|audit|investor-access` ·
`GET /api/portal/session|account|investments|activity|statements|tax|contact` ·
`POST /api/portal/messages` · plus additive `GET|POST /api/ops/jobs*` and
`/odata/Deals`.

Portal endpoints require a portal-contact session and are always scoped to that
LP (disabled contacts can't sign in; Tax-only contacts see tax + IR only).

Response shapes mirror `src/lib/types.ts` exactly (camelCase, `"In Review"`
statuses, `"Jul 02"` stage dates, amounts in USD millions); extra fields
(`basis`, `pendingEscalations`, `atUtc`, `chainValid`) are additive.

## Testing

`dotnet test` runs both projects. The weight is deliberately on
**integration tests** (70 of 93): each test class boots the real host via
`WebApplicationFactory` with its own isolated in-memory database and exercises
HTTP → auth → RBAC → services → EF/Dapper/SQLite → Quartz end-to-end. Covered:
the full 9-stage approval pipeline, escalation gating, creation validation
(reconciliation, unknown deal/investor, past due date), RBAC denials, the
Dapper inbox (approvals, overdue, wire exceptions, recon breaks, integration
warnings), all the swap read models, wire retry + recon assignment, portal
scoping (disabled/tax-only/staff rejection), waterfall invariants, audit-chain
verification after mutations, and the overdue sweep fired through the ops
endpoint and observed via the API. Domain unit tests pin the pure rules
(rounding, transitions, seal chaining).

## Deployment

- **Docker**: multi-stage `Dockerfile`, non-root runtime user, port 8080.
- **Azure**: the whole stack (this API + the frontend + Azure SQL, dev and
  prod environments) is provisioned and deployed by the top-level
  [`../infra`](../infra) folder — Bicep templates, per-environment deploy
  scripts, and GitHub Actions workflows. Start at `../infra/README.md`.

## Roadmap (per docs/BACKEND_TODO.md build order)

1. Real staff SSO + LP portal auth (replace the header scheme — the portal
   contact header is the same dev stand-in as the staff one).
2. Waterfall computation engine from share-class terms (distributions are
   seeded reads today; invariants already enforced in tests).
3. Wires lifecycle beyond retry (resolve/manual settlement) through
   `IWireGateway`; recon ingest + auto-match via `custodian-feed-sync`.
4. Admin CRUD mutations (invite user, edit role, rules, funds/LPs/borrowers) —
   reads are done; the UI dialogs currently only toast.
5. Signed document downloads through `IDocumentStorage`, gated by the
   investor-access document-type configuration.
