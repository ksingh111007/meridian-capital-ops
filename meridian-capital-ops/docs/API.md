# API contract

Every endpoint below is served today by the mock route
`src/app/api/[...endpoint]/route.ts`, backed 1:1 by a JSON file in
`src/mocks/`. Response shapes are the TypeScript types in `src/lib/types.ts`
(authoritative). All amounts are **USD millions** (`number`), dates are ISO
`YYYY-MM-DD` strings unless noted.

Cross-cutting requirements for the real backend:

- **RBAC**: enforce the role→capability matrix server-side on every endpoint
  (module → none/view/edit/approve/full). UI affordances are hints, not security.
- **Audit**: every mutation appends an `AuditEvent` (append-only, hash-chained
  `seal`). No exceptions.
- **Portal scoping**: `/portal/*` endpoints derive the investor from the session,
  never from a parameter.

## Session

| Method & path | Mock file | Returns |
| --- | --- | --- |
| `GET /api/me` | `me.json` | `CurrentUser` — drives approval affordances (stage approver match) |

## Portfolio & deals

| Method & path | Mock file | Returns |
| --- | --- | --- |
| `GET /api/portfolio/summary` | `portfolio-summary.json` | `PortfolioSummary` (KPIs, 8-quarter trend, exposure mix) |
| `GET /api/deals` | `deals.json` | `Deal[]` |
| `GET /api/deals/:id` | `deal-details.json` | `DealDetail` (terms, cashflows, risk, LP exposure, documents) |
| `GET /api/needs-attention` | `needs-attention.json` | `AttentionItem[]` — backend should **compute** this from live state (pending approvals for the caller, overdue allocations, wire exceptions, recon breaks, integration warnings, calls due ≤ 7d), not store it |

## Capital calls

| Method & path | Mock file | Notes |
| --- | --- | --- |
| `GET /api/capital-calls` | `capital-calls.json` | `CapitalCall[]` — allocations, stage events, docs, audit embedded |
| `GET /api/capital-calls/:id` | 〃 | `CapitalCall` |
| `GET /api/workflows/capital-calls` | `workflows.json` | `{ stages: WorkflowStage[], escalationRules: EscalationRule[] }` |
| `POST /api/capital-calls` | — | Create (wizard 2c). Body: `{ dealId, amount, dueDate, basis: "unfunded"\|"commitment", allocations: [{ investorId, amount }] }`. Server must re-validate reconciliation (Σ allocations = amount), create at stage 1, generate notices. |
| `POST /api/capital-calls/:id/approve` | — | Body `{ comment }` (**required**). Only the current stage's approver role (or full). Advances one stage; applies escalation rules; notifies next approver; audit event. |
| `POST /api/capital-calls/:id/reject` | — | Body `{ comment }` (**required**). Returns to the prior stage, status `Returned`; audit event. |

## Distributions & fund ops

| Method & path | Mock file | Notes |
| --- | --- | --- |
| `GET /api/distributions` | `distributions.json` | `Distribution[]` — tiers + per-investor payouts embedded |
| `GET /api/distributions/:id` | 〃 | `Distribution` |
| `GET /api/drawdowns` | `drawdowns.json` | `{ kpis, drawdowns: Drawdown[] }` — draws link to their deal/call (`linkedCallId`) |
| `GET /api/wires` | `wires.json` | `{ asOf, kpis, wires: Wire[] }` |
| `POST /api/wires/:id/retry` | — | Re-queue an `Exception` wire. Must re-check wire instructions on file; audit event. |
| `GET /api/cash/position` | `cash-position.json` | `CashPosition` (KPIs, 13-week forecast, 4-week liquidity table, accounts) |
| `GET /api/reconciliation` | `reconciliation.json` | `{ asOf, source, kpis, items: ReconItem[] }` |
| `POST /api/reconciliation/:id/assign` | — | Body `{ assignee }`. Assign a Break/Unmatched item; audit event. |

## Admin

| Method & path | Mock file | Notes |
| --- | --- | --- |
| `GET /api/admin/users` | `users.json` | `{ kpis, users: StaffUser[], roles: Role[] }` — roles carry the capability matrix |
| `GET /api/admin/funds` | `funds.json` | `{ kpis, funds, entities, shareClasses }` — share-class terms drive the waterfall |
| `GET /api/admin/investors` | `investors.json` | `{ kpis, investors: Investor[] }` — `wireInstructionsOnFile` gates payouts; `commitments[].called` gives the unfunded basis |
| `GET /api/admin/reference` | `reference-data.json` | Borrowers, currencies, settlement calendars |
| `GET /api/admin/integrations` | `integrations.json` | Feed health; warnings surface in needs-attention |
| `GET /api/admin/notification-rules` | `notification-rules.json` | `{ rules, channels }` |
| `GET /api/admin/audit` | `audit-log.json` | `{ kpis, events: AuditEvent[] }` — append-only, hash-chained |
| `GET /api/admin/investor-access` | `investor-access.json` | `InvestorAccessConfig` — portal contacts, capability toggles, exposed doc types |

CRUD for admin entities (invite user, edit role, new rule, connect integration,
add fund/LP/borrower, edit investor access) is drawn in the UI as dialogs but
only toasts in the mock — define REST endpoints per entity when building the
backend (suggested: `POST/PATCH` under the same `/api/admin/...` paths).

## Investor portal (session-scoped to the LP)

| Method & path | Mock file | Returns |
| --- | --- | --- |
| `GET /api/portal/account` | `portal-account.json` | `PortalAccount` (stats + per-fund cards) |
| `GET /api/portal/investments` | `portal-investments.json` | positions by fund + quarterly rollforward |
| `GET /api/portal/activity` | `portal-activity.json` | lifetime call/distribution ledger |
| `GET /api/portal/statements` | `portal-statements.json` | document library (filter client-side today; server-side later) |
| `GET /api/portal/tax` | `portal-tax.json` | K-1s by year; `Pending` rows carry `expectedDate` and must not be downloadable |
| `GET /api/portal/contact` | `portal-contact.json` | IR team + recent ticketed requests |
| `POST /api/portal/messages` | — | Body `{ subject, regarding, message }`. Creates a ticketed IR request; the messaging backend/threading is unspecified (known gap). |

Document downloads (`↓ PDF` buttons) are toasts in the mock; the backend should
serve signed URLs gated by the investor-access document-type configuration.
