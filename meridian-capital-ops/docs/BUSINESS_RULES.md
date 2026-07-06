# Business rules

The domain logic the backend must implement. The UI renders the *results* of
these rules; it never computes them authoritatively.

## Distribution waterfall (European whole-fund)

Order, from a `distributable` pool:

1. **Return of Capital** — 100% LP until contributed capital is returned.
2. **Preferred Return** — 100% LP until the pref hurdle (e.g. 8%) on
   contributed capital is met. *Note: production pref is time-based (accrual
   with a day-count and compounding convention per the LPA) — the mock's flat
   figure is illustrative. Make the accrual convention a fund/share-class
   attribute.*
3. **GP Catch-up** — 100% GP until GP has received `carryPct` (e.g. 20%) of
   profit distributed so far.
4. **Carried Interest** — residual split (e.g. 80/20 LP/GP).

Invariants: tiers exhaust the pool exactly; `lpTotal + gpTotal =
distributable`; per-investor payouts are pro-rata by LP ownership and sum to
`lpTotal`. Terms (mgmt fee, carry, pref) come from the fund's **share class**
(admin 5c) — never hardcode. `Fund.waterfallType` may also be `American`
(deal-by-deal) — same tiers computed per deal realization.

Recallable distributions (if the LPA allows) restore unfunded commitment —
model the flag per distribution even though the UI doesn't surface it yet.

## Capital-call allocation

- Default **pro-rata by unfunded commitment** (`commitment − called` per LP in
  the fund). A "by total commitment" basis is selectable at creation; record
  which basis was used.
- Allocations are editable before submission but **must reconcile**:
  `Σ allocations = call amount` — enforced in the wizard UI and re-validated
  server-side on `POST /capital-calls` and on any allocation edit.
- Allocation edits after approval has begun require re-approval (the mock's
  #C-2039 story: edit → returned to CIO).

## Due-diligence approval pipeline

- Ordered stages from the workflow config (default 9: Operations → Front
  Office → CIO → Legal → Ops Final Review → Accounting → Book →
  Custodians Notified → Completed). Stages 7–8 are system/auto stages;
  9 is terminal.
- Each stage has an **approver role** and an **SLA (days)**; breaching the SLA
  fires the "Approval overdue" notification rule.
- **Approve** requires a comment → advances one stage → notifies the next
  approver → audit event. **Reject** requires a comment → returns to the
  *prior* stage with status `Returned` → audit event.
- Only a user whose role matches the stage's approver role (or with `full`
  Approvals capability) may act — enforce server-side.
- **Escalation rules** (admin 5b) inject additional required approvers:
  amount > $20M → CIO + Compliance; cross-fund allocation → Legal; wire to a
  new bank account → dual authorization.

## Overdue calls (added in review)

An allocation not `Wired`/`Confirmed` after the call's due date is **Overdue**:
show aging (days late), surface in needs-attention, and fire notifications.
Production follow-ups: default-interest calculation and LPA remedy tracking
(out of scope here, schema should anticipate them).

## RBAC

The role→capability matrix (admin 5a) is the single source of truth:
per module (Blotter, Approvals, Wires, Recon, Ref Data, Admin) →
`none | view | edit | approve | full`. It must drive **both** UI affordances
and API authorization. Admin capability gates the Admin section. The frontend
currently renders affordances from it (e.g. 2b approve buttons) but does not
guard routes — backend must.

## Wire lifecycle

`Queued → Sent → Acknowledged → Settled`; any failure → `Exception`
(actionable: retry / resolve-after-manual-confirmation, both audited).
**A wire can never be created to an LP without wire instructions on file**
(Investor Registry 5d) — payouts to such LPs are `Blocked` until banking
details are added and verified. New-bank-account wires require dual
authorization (escalation rule).

## Reconciliation

Match book vs custodian per item: non-zero diff → `Break`; missing counterpart
→ `Unmatched`. Both are assignable and clearable (audited). Auto-match runs on
feed sync. KPI: matched %, open exceptions, net amount in break.

## Drawdowns (facility bridges)

The fund draws on subscription/NAV facilities to fund a deal fast, then repays
from the matching capital call — `Drawdown.linkedCallId` carries the link.
Track utilisation vs facility limit; >80% fires the (currently disabled)
utilisation notification rule. Naming warning: **"Drawdowns" here = facility
draws**; LPs use "drawdown" to mean a capital call — keep the terms straight in
backend naming.

## Investor portal access

A `PortalContact`'s role (Primary / Viewer / Tax-only) + funds-visible + the
tenant capability toggles + exposed document types (admin 6c) determine what
the LP sees and downloads. Tax-only contacts see only tax documents. Disabled
contacts cannot sign in. An LP sees only their own capital account across
their funds. Pending tax documents (future years) are never downloadable.

## Auditability

Every create / approve / reject / edit / wire action / config change writes an
append-only, hash-chained `AuditEvent` (actor, action, object, detail, seal).
UI contract: every mutation shows a toast (success/error, Undo where safe) and
destructive/workflow-affecting actions get a confirm step first (the required
-comment modal serves as confirmation for approve/reject).
