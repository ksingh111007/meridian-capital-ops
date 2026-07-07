# Meridian Capital Ops

Front-end for a **private-credit fund operations platform**: the money-movement
lifecycle for direct-lending funds — calling capital from LPs, moving each call
through a due-diligence approval pipeline, drawing on credit facilities, paying
distributions via the waterfall, tracking wires, reconciling to the custodian,
monitoring deal performance — plus the admin surfaces that configure all of it
and an external self-service **Investor Portal** for LPs.

Built from the Claude Design wireframe handoff in `../project/` (28 screens),
with three product improvements agreed during review:

1. **Needs-attention inbox** — home-screen strip + top-bar bell: approvals
   waiting on *you*, overdue LP wires, wire exceptions, recon breaks, expiring
   integration certs, calls due ≤ 7 days.
2. **Overdue call tracking** — an `Overdue` wire status with day-count aging on
   the blotter; the new-call wizard defaults to **pro-rata by unfunded
   commitment** (with a basis selector).
3. **Distribution payouts tab + actionable exceptions** — per-investor payout
   tab on Distributions (with the wire-instructions gate), and
   retry/resolve/assign/clear actions on wire exceptions and recon breaks.

## Stack

- **Next.js 16** (App Router, Turbopack) · **React 19** · **TypeScript (strict)** · **Tailwind CSS v4**
- No component library — a small bespoke UI kit in `src/components/ui/`
- Data comes from the **.NET backend in `../backend/`** by default
  (`DATA_SOURCE=api`); set `DATA_SOURCE=mock` to serve the JSON mocks instead
  (no backend needed — the mock files stay in the repo for testing).

## Run

```bash
npm install
npm run dev     # http://localhost:3000  (Portfolio home)
npm run build   # production build
```

Start the backend first (default mode fetches it per request):

```bash
cd ../backend && ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/Meridian.Api --urls http://localhost:8080
```

Environment (see `.env.example`; all optional — these are the defaults):

| Var | Default | Meaning |
| --- | --- | --- |
| `MERIDIAN_API_URL` | `http://localhost:8080` | Backend base URL |
| `MERIDIAN_API_USER` | `u-admin` | Staff user id sent as `X-User-Id` on internal endpoints (Administrator — full capability matrix) |
| `MERIDIAN_PORTAL_CONTACT` | `pc-1` | Portal contact id sent as `X-User-Id` on `/api/portal/*` (Karen Doyle, Redwood Pension) |
| `DATA_SOURCE` | `api` | `api` = fetch the backend · `mock` = serve `src/mocks/*.json` |

The internal app opens at `/portfolio`. The external LP portal is at `/portal`
(logged in as Redwood Pension). Business "today" is **2026-07-05**.

## Where things live

```
src/
  app/(internal)/…       # staff-facing routes (shell: tree nav + top bar)
  app/portal/…           # external LP portal (separate shell — separate auth surface in prod)
  app/api/[...endpoint]/ # mock API — one registry entry per endpoint
  screens/               # client screen components (one per screen)
  components/ui/         # UI kit: DataTable, Pill, Modal, Toast, Pipeline, states…
  components/shell/      # TreeNav, TopBar, PortalNav, ScreenHeader
  lib/types.ts           # entity types — the backend contract
  lib/data.ts            # data layer: one function per endpoint (swap point for the backend)
  lib/format.ts          # money/percent/date formatting (amounts are USD millions)
  mocks/*.json           # one JSON per API call — the sample dataset
docs/                    # start here for backend work — see below
```

## Documentation (read before backend work)

| Doc | Contents |
| --- | --- |
| [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) | Route map, screen pattern, shells & auth boundaries, how to swap in the backend |
| [docs/API.md](docs/API.md) | Every endpoint: method, path, shape, mutation semantics, RBAC/audit requirements |
| [docs/DATA_MODEL.md](docs/DATA_MODEL.md) | Entities, fields, relationships, id conventions |
| [docs/BUSINESS_RULES.md](docs/BUSINESS_RULES.md) | Waterfall math, allocation basis, DD pipeline, RBAC, wire lifecycle, recon, portal access, auditability |
| [docs/BACKEND_TODO.md](docs/BACKEND_TODO.md) | What is mocked vs. real, acceptance criteria per flow, known gaps |
| [docs/CONVENTIONS.md](docs/CONVENTIONS.md) | Frontend conventions for adding screens |
| [docs/DESIGN.md](docs/DESIGN.md) | Design tokens, component inventory, screen-state & feedback contracts |

## Screen inventory

**Internal** — Portfolio `/portfolio` (home) · Deal detail `/portfolio/deals/:id` ·
Capital Calls blotter `/capital-calls` · Call detail + DD pipeline `/capital-calls/:id` ·
New-call wizard `/capital-calls/new` · Distributions `/distributions` ·
Drawdowns `/drawdowns` · Wire Status `/wires` · Cash Position `/cash` ·
Reconciliation `/reconciliation` · Admin: Users & Roles, Approval Workflows,
Funds & Entities, Investor Registry, Reference Data, Integrations,
Notifications, Investor Access, Audit Log under `/admin/*`.

**Portal (external)** — Overview `/portal` · My Investments · Capital Activity ·
Statements · Tax Documents · Contact IR.
