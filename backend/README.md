# Meridian Capital Ops — backend

ASP.NET Core 8 (LTS) API for the fund-operations platform. Implements the
contract in [`../meridian-capital-ops/docs/API.md`](../meridian-capital-ops/docs/API.md)
starting with the highest-value vertical slice — the **capital-call approval
pipeline** — plus distributions reads, the computed needs-attention inbox, the
hash-chained audit log, background automation, and an OData query surface.

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
├── infra/                        # Bicep for Azure App Service + example GitHub Actions workflow
├── Dockerfile · docker-compose.yml
└── MeridianCapital.sln
```

## Run it

```bash
cd backend
dotnet test                                   # 68 tests: domain unit + API integration
dotnet run --project src/Meridian.Api        # http://localhost:8080 (Swagger at /swagger)
# or
docker compose up --build
```

Authenticate requests with `X-User-Id: <seeded staff user>` (see below). Try the
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

### Data: EF Core + Dapper over in-memory SQLite

Persistence is **EF Core on shared-cache in-memory SQLite** — a real relational
database with the lifetime of the process, so there is no migration baggage
while the schema is still moving. The dedicated database project can later point
`AppDbContext` at SQL Server/PostgreSQL and add migrations; `IAppDbContext`
consumers won't change. Two dev-only conveniences to know about:

- decimals are stored as `REAL` (SQLite has no decimal affinity); amounts are
  2-dp USD millions so this is lossless — production maps native `decimal`.
- `EnsureCreated()` replaces migrations until the DB project exists.

**Dapper** is used where hand-written SQL beats the ORM: the needs-attention
inbox (`NeedsAttentionService`) aggregates across calls/allocations/stages per
caller. Writes always go through EF Core so change tracking + audit stay intact.

Seed data mirrors the frontend mock story 1:1 (`StorySeed`: all 7 deals, all 6
capital calls — #C-2041 at Legal, #C-2039 returned with two overdue wires,
#C-2043/#C-2044 gated on CIO + Compliance sign-off — all 4 distributions incl.
#D-119's Blocked/Exception payouts, the staff roster and role matrix) plus
deterministic **Bogus** volume data (`FakeDataSeed`, fixed seed → identical
rows in every run and test host). Keep it consistent with
`meridian-capital-ops/src/mocks/*` when editing — the frontend swap depends on
both sources telling the same story.

### AuthN / AuthZ

- **Authentication** is a dev stand-in: `X-User-Id` header → seeded staff user
  (`HeaderAuthenticationHandler`). Only `Active` staff authenticate — Invited
  and Disabled users are rejected. It is one isolated handler behind the
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
  (`seal_n = H(seal_{n-1} ‖ event_n)`, culture-invariant, length-prefixed
  fields); `GET /api/admin/audit` re-verifies the chain on read
  (`kpis.chainValid`). Appends are transactional: `IAuditTrail` flushes the
  scoped unit of work, so a mutation and its audit event(s) commit atomically —
  services stage changes and never `SaveChanges` before appending.
- Workflow mutations carry an optimistic-concurrency version: concurrent
  approvals of the same call yield 409, never duplicate stage events. Capital
  calls also queue investor notices (outbox rows) and **notify the next
  approver** through the notification port.
- Business errors are typed (`DomainException` Validation/NotFound/Forbidden/
  Conflict) and map to 400/404/403/409 ProblemDetails.
- No `DateTime.Now` in business code — `IClock` everywhere.

### Background automation (Quartz)

Three durable jobs, cron-scheduled from config (`Jobs:*`) and runnable on demand
via `POST /api/ops/jobs/{name}/run` (Admin:edit):

| Job | What it does |
| --- | --- |
| `overdue-allocation-sweep` | Flips unpaid allocations past due date to `Overdue` (regardless of pipeline status — wire state is independent), audits + notifies in one atomic commit (BUSINESS_RULES § Overdue calls) |
| `approval-sla-monitor` | Fires "Approval overdue" notifications for stages past their SLA (scaffold: no dedupe store yet) |
| `custodian-feed-sync` | Pulls the custodian snapshot through the `ICustodianFeed` port — the recon auto-match engine plugs in here |

### OData

`/odata/Deals` is a full OData entity set (`$filter`, `$orderby`, `$top`,
`$select`, `$count`; queries compose into SQL through EF Core) — the
server-side filtering/pagination story for production-volume books. Pattern
generalizes to more entity sets by adding them to the EDM in `Program.cs`.

## Endpoints implemented (of docs/API.md)

`GET /api/me` · `GET/POST /api/capital-calls` · `GET /api/capital-calls/{id}` ·
`POST /api/capital-calls/{id}/approve|reject` · `GET /api/workflows/capital-calls` ·
`GET /api/distributions[/{id}]` · `GET /api/needs-attention` ·
`GET /api/admin/audit` · plus additive `GET|POST /api/ops/jobs*` and `/odata/Deals`.

Response shapes mirror `src/lib/types.ts` exactly (camelCase, `"In Review"`
statuses, `"Jul 02"` stage dates, amounts in USD millions); extra fields
(`basis`, `pendingEscalations`, `atUtc`, `chainValid`) are additive.

## Testing

`dotnet test` runs both projects. The weight is deliberately on
**integration tests** (45 of 68): each test class boots the real host via
`WebApplicationFactory` with its own isolated in-memory database and exercises
HTTP → auth → RBAC → services → EF/Dapper/SQLite → Quartz end-to-end. Covered:
the full 9-stage approval pipeline, escalation gating, creation validation
(reconciliation, unknown deal/investor, past due date), RBAC denials, the
Dapper inbox, waterfall invariants, audit-chain verification after mutations,
and the overdue sweep fired through the ops endpoint and observed via the API.
Domain unit tests pin the pure rules (rounding, transitions, seal chaining).

## Deployment

- **Docker**: multi-stage `Dockerfile`, non-root runtime user, port 8080.
- **Azure**: `infra/main.bicep` provisions ACR + Linux App Service (Web App for
  Containers) + Log Analytics + App Insights, with managed-identity ACR pulls,
  HTTPS-only/TLS 1.2/FTPS-disabled and the `/healthz` probe. See the header of
  `infra/main.bicep` for the two-command deploy.
- **CI/CD**: `infra/github/deploy-backend.yml` is a ready GitHub Actions
  workflow (build → test → `az acr build` → point the web app at the new tag);
  move it to `.github/workflows/` to activate it.

## Roadmap (per docs/BACKEND_TODO.md build order)

1. Real staff SSO + LP portal auth (replace the header scheme).
2. Remaining read models: portfolio summary, deals detail, drawdowns, wires,
   cash position, recon, admin, portal — then swap the frontend's
   `src/lib/data.ts` to fetch from here.
3. Waterfall computation engine from share-class terms (distributions are
   seeded reads today; invariants already enforced in tests).
4. Wires lifecycle + retry/resolve through `IWireGateway`; recon ingest +
   auto-match via `custodian-feed-sync`.
5. Dedicated database project (SQL Server/PostgreSQL + migrations) replacing
   the in-memory SQLite dev store.
