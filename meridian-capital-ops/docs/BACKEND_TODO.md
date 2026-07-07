# Backend handoff — what's real, what's mocked, what to build

> **Update 2026-07-07**: the backend now implements the full API.md contract
> against Azure SQL (`../database` dacpac) / in-memory SQLite, and the frontend
> fetches it by default — see `STATUS.md` § "2026-07-07 session". Done from the
> build order below: **2** (read models + data.ts swap), the wire-retry and
> recon-assign halves of **4**, the reads of **6** (portal scoping incl.
> Tax-only/Disabled) and **7** (audit log, integrations health; needs-attention
> now computes wire exceptions, recon breaks, and integration warnings from
> live tables). Still open: **1** (real SSO — the `X-User-Id` header scheme is
> a dev stand-in for both staff and portal contacts), **3** partially (create/
> approve/reject/overdue work; notices are stubs), **5** (waterfall engine),
> admin CRUD mutations, document downloads, IR messaging threads.

## Current state (as of the initial frontend-only build)

Everything renders and the key flows are interactive, but **nothing persists**:

| Works today (frontend) | Mocked (backend to build) |
| --- | --- |
| All 25+ screens, both shells, tree nav (persisted collapse) | Authentication (staff SSO + separate LP portal auth) |
| 2b approve/reject with required comment; optimistic stage advance + audit entry + toast | Real stage transitions, notifications, escalation-rule injection |
| 2c wizard: pro-rata by unfunded/commitment, editable, reconcile gate | `POST /capital-calls` — create, notice generation, calendar validation |
| Blotter grouping, overdue aging, filters, sorting, search | Server-side filtering/pagination for large books |
| Distributions waterfall ledger + payouts tab with Blocked/Exception handling | Waterfall computation from share-class terms |
| Wire retry/resolve, recon assign/clear (optimistic + toast) | Real wire orchestration (rails, SWIFT), recon matching engine |
| Needs-attention inbox (strip + bell) | Computing attention items from live state per user |
| Admin CRUD dialogs (invite user, edit role, new rule, …) | All admin persistence + RBAC enforcement |
| Portal screens incl. download buttons, Contact IR form | Signed document downloads, IR ticketing/messaging service |

## Build order (suggested)

1. **Auth + RBAC**: staff session, capability matrix enforcement (middleware on
   `/api/*` and route guards), portal session scoped to a PortalContact.
2. **Read models**: implement the GET endpoints in docs/API.md against the real
   store; swap `src/lib/data.ts` to async fetches (see ARCHITECTURE.md §Swap).
3. **Capital-call workflow**: create → pipeline transitions (approve/reject with
   comments, escalations, SLAs) → notices → allocations & wire tracking →
   overdue detection job.
4. **Money movement**: wires (lifecycle, exceptions, retry), drawdown links,
   cash position aggregation, recon ingest + auto-match.
5. **Distributions**: waterfall engine from share-class terms; payout
   generation gated by wire instructions; payment execution.
6. **Portal**: capital accounts, documents (signed URLs gated by exposed-type
   config), activity ledger, tax availability, IR ticketing.
7. **Cross-cutting**: audit log (append-only, hash-chained), notification rules
   engine, integrations health.

## Acceptance criteria (key flows)

- **Approve/reject a call**: only the current stage's approver (or `full`) can
  act; comment required; approve advances + notifies next approver; reject
  returns per workflow; both append audit events; escalation thresholds inject
  approvers (>$20M → CIO + Compliance, etc.).
- **New-call wizard**: allocations default pro-rata by *unfunded* commitment,
  stay editable, must equal the call amount before submit; server re-validates;
  call lands at stage 1 with notices queued.
- **Distribution**: tiers computed from the fund's share-class terms reconcile
  exactly (`LP + GP = distributable`); payouts pro-rata; an LP without wire
  instructions yields a `Blocked` payout, never a wire.
- **Wires**: no wire to an LP without instructions on file; failures become
  actionable `Exception`s (retry re-checks instructions); resolve requires
  manual-settlement confirmation; all audited.
- **Reconciliation**: diff ≠ 0 → Break; missing counterpart → Unmatched; both
  assignable/clearable with audit.
- **Overdue**: allocation unpaid past due date → `Overdue` with aging; appears
  in needs-attention and fires notifications.
- **Portal access**: LP sees only own funds/accounts; Tax-only sees only Tax;
  Disabled cannot sign in; downloads gated by exposed document types; pending
  tax docs not downloadable.
- **Cross-cutting**: every list/detail screen implements the 5 shared states
  (components exist in `src/components/ui/states.tsx`); every mutation toasts
  and audits; destructive/workflow actions confirm first.

## Known gaps / decisions deferred (from the design handoff + review)

- **Call purpose breakdown**: design binds one deal per call; schema should
  support investment + mgmt-fee + expense components per call notice.
- **Preferred-return accrual**: time-based (day-count, compounding) per LPA —
  the mock shows flat illustrative numbers.
- **Recallable distributions** restoring unfunded commitment — model the flag.
- **Default interest / LPA remedies** for overdue LPs — schema should anticipate.
- IR **messaging service** (threading/inbox) for Contact IR — only the form +
  ticket list are specified.
- Sort/filter/search/pagination are client-side on mock-sized data — spec
  server-side behavior for production volumes.
- **Responsive/mobile**, multi-currency display, i18n — out of scope in design.
- Numbers, names, and dates in mocks are **illustrative**, not authoritative.
