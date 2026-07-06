# Data model

Authoritative source: `src/lib/types.ts`. This doc adds relationships, id
conventions, and per-entity notes the types can't express. All money fields are
USD **millions**. Sample values live in `src/mocks/` and are internally
consistent across screens (see "Cross-screen consistency" below).

## Id & reference conventions

| Entity | id | Human ref |
| --- | --- | --- |
| Fund | `fund-i` / `fund-ii` / `fund-iii` | "Fund III" |
| Deal | `deal-<codename>` (`deal-atlas`) | "Project Atlas" |
| Investor (LP) | `inv-<name>` (`inv-redwood`) | — |
| Capital call | `call-<n>` (`call-2041`) | `#C-2041` |
| Distribution | `dist-<n>` (`dist-118`) | `#D-118` |
| Wire | `wire-<n>` (`wire-8847`) | `W-8847` |

## Entity graph

```
Fund ─┬─ LegalEntity*            (GP / Master / Feeder / Cayman / Blocker)
      ├─ ShareClass*             (mgmtFeePct, carryPct, prefPct → waterfall inputs)
      ├─ Deal*                   (borrower, tranche, terms, performance)
      │    └─ DealDetail         (cashflows, risk, LP exposure, documents)
      ├─ CapitalCall*            (one deal per call in this design; see note)
      │    ├─ CallAllocation*    (investor, amount, wireStatus)
      │    ├─ StageEvent*        (pipeline instance state)
      │    └─ audit[]            (call-scoped trail shown in 2b)
      └─ Distribution*
           ├─ WaterfallTier*     (ordered; must reconcile to distributable)
           └─ InvestorPayout*    (per-LP; gated by wire instructions)

Investor ─ commitments[{ fundId, amount, called }]   → unfunded = amount − called
StaffUser ─ role → Role.capabilities (module → none/view/edit/approve/full)
PortalContact ─ investorId → Investor (external portal identity)
Drawdown ─ linkedCallId → CapitalCall (facility bridge repaid by the call)
Wire ─ linkedRef → "#C-…" | "#D-…" | facility name
ReconItem ─ description references the batch/receipt it reconciles
AuditEvent ─ global append-only log (admin screen 5h)
```

## Per-entity notes

- **CapitalCall.currentStage** is 1-based against the workflow's ordered
  stages (9 = `Completed`, terminal). `stageEvents` holds only stages that have
  history (`done`/`current`); the UI derives `pending` for the rest.
- **CallAllocation.wireStatus** lifecycle: `Pending → Scheduled → Wired →
  Confirmed`, plus **`Overdue`** (added in review): unpaid after
  `call.dueDate`. Overdue drives blotter aging badges and needs-attention.
- **Note — call purpose**: this design binds a call to exactly one deal. Real
  call notices often bundle deal fundings + management fees + expenses. The
  backend schema should allow a `purposes[]` breakdown per call even though the
  UI currently shows one deal (flagged in BACKEND_TODO.md).
- **Distribution** must satisfy: `Σ tier.distributed = distributable`,
  `lpTotal = Σ tier.lpShare`, `gpTotal = Σ tier.gpShare`,
  `lpTotal + gpTotal = distributable`, and `Σ payouts.amount = lpTotal`.
  `InvestorPayout.status = "Blocked"` ⇔ the LP has no wire instructions on file.
- **Investor.commitments[].called** feeds the wizard's pro-rata-by-unfunded
  default. Keep it current as calls complete.
- **Role.capabilities** is the RBAC source of truth; module names are the
  `MODULES` const in types.ts.
- **AuditEvent.seal** is a truncated hash in the mock; production should
  hash-chain (`seal_n = H(seal_{n-1} ‖ event_n)`) for tamper evidence.
- **AttentionItem** is a computed projection, not stored state: pending
  approvals for the caller (`mine: true`), overdue allocations, wire
  exceptions, recon breaks, integration warnings, and calls due ≤ 7 days.
- **CapitalAccount** (investor × fund) appears portal-side as
  `PortalAccount.funds[]` + the rollforward: beginning NAV + contributions −
  distributions + change in value = ending NAV.

## Cross-screen consistency (keep when editing mocks)

The sample dataset tells one coherent story — screens cross-reference it:

- Wire **W-8847** (SWIFT exception, Granite State, $4.08M) = the `Exception`
  payout on distribution **#D-119** = the SWIFT Gateway cert warning (5f) = the
  audit-log exception event = a needs-attention item. The **$0.42M recon
  break** is the #D-119 batch's partial settlement.
- **Oakmont Trust** has `wireInstructionsOnFile: false` → its #D-119 payout is
  `Blocked`, flagged in the Investor Registry (5d).
- Call **#C-2041** (Project Atlas, $16M): Blue Harbor's $5.10M allocation is
  `Wired` = inbound wire W-8842 = a `Matched` recon item; the call sits at
  stage 4 (Legal) awaiting the mock current user (J. Okafor, Counsel).
- Call **#C-2039** (Project Delta, due 2026-07-01) is `Returned` at CIO with
  two `Overdue` allocations — the overdue-tracking demo.
- Drawdowns bridge deals and are repaid by the matching call
  (`linkedCallId`); Cedar's repaid draw = outbound wire W-8846.
- Portal (Redwood Pension) activity mirrors the internal data: −$8.20M due on
  #C-2041, +$2.10M processing on #D-119, +$3.40M paid on #D-117.

Mock "today" is **2026-07-05** (`TODAY` in `src/lib/format.ts`); overdue/due-soon
math keys off it. Numbers/names/dates are illustrative sample data, not
authoritative business values.
