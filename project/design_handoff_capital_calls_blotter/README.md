# Handoff: Meridian Capital Ops — Private-Credit Trade Blotter

## Overview
A back-office operations platform for a **private-credit fund manager**. It runs the money-movement lifecycle for direct-lending funds: calling capital from LPs, moving it through a due-diligence approval pipeline, drawing on credit facilities, paying distributions via the waterfall, tracking wires, reconciling to the custodian, monitoring deal performance, and giving external LPs a self-service portal. It also includes the admin surfaces that configure all of the above.

The design is delivered as a single interactive HTML canvas (`Capital Calls Blotter Wireframes.dc.html`) containing **28 screen cards** grouped into 5 areas, each card tagged with a stable id (e.g. `6a`, `2b`) shown as a badge and used throughout this document.

## About the Design Files
The files in this bundle are **design references created in HTML** — low-fidelity prototypes that communicate intended structure, content, and behavior. **They are not production code to copy.** The task is to **recreate these designs in the target codebase's environment** (React, Vue, Angular, etc.) using its established component library, routing, data layer, and design system. If no codebase exists yet, pick an appropriate stack (a React + TypeScript SPA with a component library and a typed API layer is a reasonable default for a data-dense internal tool) and implement there.

The HTML uses a small internal template runtime (`support.js`) only so the prototype can render and demonstrate a few interactions. **Do not port `support.js` or its `<x-dc>` / `{{ }}` / `<sc-if>` syntax.** Read the markup for intent and rebuild with idiomatic components.

## Fidelity: LOW-FIDELITY (wireframe)
Treat layout, information architecture, content, and flows as the spec. **Do not reproduce the wireframe's visual styling.** Apply the target codebase's design system for color, type, spacing, elevation, and components. In particular, these are deliberate wireframe conventions to **discard**:
- The **"Caveat" handwritten annotations** (orange/blue script notes and arrows) — they are reviewer commentary, not UI.
- The **hard offset drop-shadow** (`6px 8px` solid), **dashed placeholder boxes**, and **sketch borders**.
- Ad-hoc hex colors (see Design Tokens — map them to your system's semantic tokens).

What IS normative: the screens present, their layout/columns, the fields and copy, the navigation model, the approval/waterfall/wire/recon logic, and the empty/loading/error/state contract.

## Navigation & App Shell

Two distinct shells:

1. **Internal app shell** (staff-facing) — a left **collapsible tree nav** + main content area. Tree structure:
   - **Portfolio** (top-level home item — the default landing screen)
   - **Inflows** ▸ Capital Calls · Drawdowns
   - **Outflows** ▸ Distributions · Wire Status
   - **Fund** ▸ Cash Position · Reconciliation
   - **Admin** ▸ Users & Roles · Approval Workflows · Funds & Entities · Investor Registry · Reference Data · Integrations · Notifications · Investor Access · Audit Log
   
   Group headings expand/collapse (chevron ▾/▸). In the prototype this state is global for demo purposes; **in production make it per-user and persisted** (localStorage or user prefs). The active item gets a left accent bar.

2. **Investor Portal shell** (external LP-facing) — a top bar (brand + logged-in investor + log out) over a simpler left nav: Overview · My Investments · Capital Activity · Statements · Tax Documents · Contact IR. This is a **separate authenticated experience** (different audience, permissions, and likely a separate deployment surface). An LP only ever sees their own capital account.

## Screens / Views

Ids match the badge on each card in the HTML. Grouped by the 5 sections.

### 1 — Portfolio & investor experience
- **`6a` Portfolio (home)** — default landing. KPI cards (Invested Capital, Net IRR, Blended MOIC, On Watch), a portfolio-value trend chart + an exposure-by-status stacked bar (Performing/Watch/Non-accrual), then a per-deal grid: Deal · Borrower · Fund · Invested · Outstanding · Yield · Net IRR · MOIC · Status. Rows are clickable → `6d`. Filters: All / Performing / On watch / By sector / Non-accrual.
- **`6d` Deal detail** — breadcrumb back to Portfolio. Metric tiles (Invested, Outstanding, Coupon, Net IRR, MOIC, Fair Value). Tabs: Overview / Cashflows / Covenants / Documents. Overview shows deal terms (borrower, sector, country, fund, tranche, facility, drawn, maturity, spread/floor, upfront fee) + a cashflow schedule table; right rail: Risk & rating, LP exposure (which LPs are in the deal), documents.
- **`6b` Investor Portal — Overview** (external). Greeting + capital-account stat strip (Commitment, Paid-in, Distributions, Current NAV, Net IRR). Per-fund cards with called-progress bars. Recent capital activity table + a downloadable statements list.
- **`6e` Investor Portal — My Investments** (external). Positions-by-fund table (Commitment, Paid-in, Distributions, NAV, Net IRR, TVPI) + a Q2 capital-account rollforward (Beginning NAV → Contributions → Distributions → Change in value → Ending NAV).
- **`6f` Investor Portal — Statements** (external). Filterable document library (type / fund / year chips) → table of documents (statements, notices, K-1s, reports) each with a Download action.
- **`6g` Investor Portal — Capital Activity** (external). Lifetime ledger of every call & distribution across funds: stat strip (Paid in, Distributions received, Net invested, Next call due), filter chips (All / Calls / Distributions / fund / year), table (Date · Fund · Type · Reference · Amount · Status). The full version of the Overview's recent-activity snippet.
- **`6h` Investor Portal — Tax Documents** (external). K-1s and tax packages by year & fund, with an availability banner (2025 available; 2026 estimated Mar 2027). Table (Document · Fund · Tax year · Type · Status · download); pending years show the estimated date and disable download.
- **`6i` Investor Portal — Contact IR** (external). Secure-message form (subject, regarding-select, message, attach, send) + a right rail: "Your IR team" (relationship manager, email, direct line, hours) and "Recent requests" (ticketed, with status).
- **`6c` Admin — Investor Access**. Controls external portal access. KPIs (portal users, active, pending, investors with access). Table: Contact · Investor · Role (Primary/Viewer/Tax-only) · Funds visible · Statements · Status. Below: default portal capability toggles + which document types are exposed to investors. "Preview portal" → `6b`.

### 2 — Capital calls
- **`2a` Blotter (grouped by transaction)**. KPI strip (Total to Call, Due This Week, Funded to Date, Investors Called). Filters incl. "Group by transaction / Flat list". Table groups each **transaction** (deal) as a parent row (fund, tranche, borrower, amount due, due date, stage, status, wire-summary) that expands to the **per-investor** child rows called on it. Row → `2b`.
- **`2b` Row detail / due-diligence workflow**. Breadcrumb + status. Major-details tiles. **9-stage DD pipeline** stepper (done/current/pending). Action bar with **Approve** and **Return/Reject** — both open a **required-comment modal** (live in prototype). Body: Documents (upload + list), Investor Allocations table (per-LP allocation, % of call, wire status, must reconcile to total), and a right **Audit Trail** panel that is **collapsible** (collapses to a thin rail so the grid gets full width — live in prototype).
- **`2c` New capital call** (creation flow). 4-step wizard modal (Deal → Amount & date → Investors → Notice → Review). The Investors step auto-fills **pro-rata allocations by commitment**, editable, and the total must reconcile to the call amount before Continue.

### 3 — Distributions & fund operations
- **`4a` Distributions**. For a selected distribution: header with "To investors" total, an allocation progress bar (LP vs GP split), then the **waterfall calculation ledger** — one row per tier (Return of Capital → Preferred → GP Catch-up → Carried Interest) with Basis/rule, Rate, Distributed, LP share, GP share, Pool left, reconciling to a balanced total. Footer: LP allocation %, effective GP carry %, payment date.
- **`4b` Drawdowns**. Draws on the fund's own credit facilities (subscription / NAV lines) used to fund deals before capital is called. KPIs (Facility Limit, Drawn, Available, Wtd. Rate). Table: Facility · Lender · Purpose/Deal · Drawn · Rate · Draw Date · Repay By · Status.
- **`4c` Wire Status**. Every inbound/outbound movement. KPIs (Wires Today, Settled, In Flight, Exceptions). Table: Wire Ref · Dir (In/Out) · Counterparty · Type (Capital Call / Distribution / Facility Repay) · Amount · Time · Rail (Fedwire/ACH/SWIFT) · Status. Exceptions are visually flagged.
- **`4d` Cash Position**. Liquidity KPIs (Cash on Hand, Uncalled Capital, Facility Headroom, Net 30-Day Projection) + a 13-week forecast bar + coverage bar. Projected-liquidity table (per week: inflows/outflows/net/projected balance) + Accounts table (custodian, account, ccy, type, balance).
- **`4e` Reconciliation**. Book vs custodian. KPIs (Matched %, Exceptions, Unmatched, Amount in Break). Tabs (Cash / Positions / Investor). Table: Date · Description · Source · Book · Custodian · Diff · Status (Matched / Break / Unmatched). Breaks & unmatched rows highlighted.

### 4 — Administration
- **`5a` Users & Roles**. KPIs + tabs (Users / Roles / Permissions). Users table (avatar, role, fund access, last active, status incl. Invited). **Role → capability matrix**: rows = roles, columns = modules (Blotter, Approvals, Wires, Recon, Ref Data, Admin), cells = V (view) / E (edit) / A (approve) / ✓ (full) / — (none).
- **`5b` Approval Workflows**. Configurable version of the `2b` pipeline: 9 stage rows (stage, approver-role select, SLA, auto-advance toggle, required toggle) + escalation rules (e.g. "Call > $20M → require CIO + Compliance", "Wire to a new bank account → dual authorization").
- **`5c` Funds & Entities**. Funds table (vintage, committed, called %, strategy, waterfall type, status). Selected-fund structure: legal entities (GP LLC, Master LP, feeders, blocker) + share classes (mgmt fee / carry / pref) + waterfall type. **These terms drive the `4a` waterfall math.**
- **`5d` Investor Registry (LP master)**. LP table (type, commitment, funds, KYC/AML status, wire instructions on file) + a selected-LP profile (commitments by fund, banking, KYC). Missing wire instructions are flagged and gate whether an LP can be paid.
- **`5e` Reference Data**. Tabs (Borrowers / Deals / Tranches / Currencies / Calendars). Borrowers table (sector, country, deal, internal rating) + currency rates + settlement calendars (drive due-date & wire scheduling).
- **`5f` Integrations & Feeds**. Connection table (custodian, bank/facility, GL/accounting, market/rate data, SWIFT) with type, direction, last-sync, status. Warnings surface (e.g. cert expiring).
- **`5g` Notification Rules**. Rule table: Rule · Trigger · Channel · Recipients · Enabled (toggle) + connected channels (Email/Slack/SMS/Webhook).
- **`5h` Audit Log**. Append-only, hash-chained activity: Time · Actor · Action · Object · Detail · Seal (hash). Filters by category. Every write across the app lands here.
- **`5i` Admin action dialogs**. The create/edit modals behind Admin "+" buttons: **Invite user** (email, role, fund access), **Edit role** (capability segments per module — writes to the `5a` matrix), **New notification rule** (name, trigger, channel, recipients, enabled).

### 5 — Patterns & states
- **`7a` Screen states** — the shared contract for every list/detail screen: **Empty** (icon + message + single primary action), **Loading** (skeleton rows), **Error** (message + Retry), **No permission** (restricted + request access), **Zero results** (no matches + clear filters).
- **`7b` Feedback patterns** — **success/error toasts** (with Undo where safe) and a **confirm dialog** for destructive/workflow actions (e.g. returning a call in the pipeline).

## Interactions & Behavior

Implemented live in the prototype (verify against these):
- **Tree nav** group expand/collapse (persist per-user in production).
- **`2b` Approve / Reject** → open a modal that **requires a comment**; confirming advances (Approve) or returns (Reject) the pipeline stage and writes to the audit trail.
- **`2b` Audit Trail** panel collapse/expand; when collapsed the docs+allocations grid takes the reclaimed width.

Implied (design intent — build these): row-click navigation (blotter→detail, portfolio→deal), sortable columns, working filter chips, search, pagination/virtualization for long tables, inline-editable allocation amounts (`2b`, `2c`), drag-reorder of pipeline stages (`5b`), and the `2c` multi-step wizard with live reconciliation validation.

Every state-changing action should: (a) show a confirm dialog if destructive/workflow-affecting (`7b`), (b) show a success/error toast, and (c) write an audit event (`5h`).

## State Management

Prototype component state (minimal, for demo): `modal` (approve/reject/none for `2b`), `auditOpen` (bool for `2b` panel), and nav group expanded booleans (Inflows/Outflows/Fund/Admin).

Production state you'll actually need: authenticated user + role + permissions (drives every screen's read/edit/approve affordances and route guards); selected fund/period filters; server data for each entity below (with loading/error per the `7a` contract); optimistic updates + audit writes on mutations; and separate auth/session for the external Investor Portal.

## Data Model (entities & key fields)

- **Fund**: name, vintage, strategy, committed, calledPct, waterfallType (European whole-fund | American deal-by-deal), baseCurrency, status (Active/Investing/Harvesting).
- **LegalEntity**: fund, name, kind (GP / Master LP / Feeder / Blocker / jurisdiction).
- **ShareClass**: fund, name, mgmtFeePct, carryPct, prefPct.
- **Investor (LP)**: name, type (Pension/Endowment/Family Office/Insurance/FoF/Trust/Asset Mgr), commitments[{fund, amount}], kycStatus (Verified/In review), wireInstructionsOnFile (bool), funds[].
- **Deal**: project name, borrower, sector, country, fund, tranche, facilityType, commitment, drawn/outstanding, spread, floor, upfrontFee, maturity, internalRating, status (Performing/Watch/Non-accrual), netIRR, MOIC, fairValue.
- **Borrower**: name, sector, country, internalRating.
- **CapitalCall**: ref (#C-####), deal, fund, amount, dueDate, stage (1–9), status, allocations[].
- **CallAllocation**: investor, amount, pctOfCall, wireStatus (Pending/Scheduled/Wired/Confirmed).
- **DDStage** (config): order, name, approverRole, slaDays, autoAdvance, required. **DDStageInstance**: call, stage, state (done/current/pending), approver, timestamp, comment.
- **Distribution**: ref (#D-###), fund, distributable, tiers[], lpTotal, gpCarry, paymentDate, investorPayouts[].
- **WaterfallTier**: name, basis, rate, distributed, lpShare, gpShare, poolLeft.
- **Drawdown**: facility, lender, purposeDeal, amount, rate, drawDate, repayBy, status (Outstanding/Requested/Repaid).
- **Wire**: ref (W-####), direction (In/Out), counterparty, type, amount, time, rail (Fedwire/ACH/SWIFT), status (Queued/Sent/Acknowledged/Settled/Exception).
- **CashAccount**: custodian, account, currency, type (Operating/Escrow/Reserve), balance.
- **ReconItem**: date, description, source, bookAmount, custodianAmount, diff, status (Matched/Break/Unmatched).
- **User** (staff): name, role, fundAccess, lastActive, status (Active/Invited).
- **Role** + **CapabilityMatrix**: per module (Blotter/Approvals/Wires/Recon/RefData/Admin) → none|view|edit|approve|full.
- **PortalContact**: name, email, investor, role (Primary/Viewer/Tax-only), fundsVisible, canDownload, status (Active/Invited/Disabled).
- **Integration**: name, type, direction, lastSync, status (Connected/Warning/Error).
- **NotificationRule**: name, trigger, condition, channels[], recipients[], enabled.
- **AuditEvent**: time, actor, action, object, detail, sealHash (hash-chained).
- **CapitalAccount** (investor×fund): commitment, paidIn, distributions, nav, netIRR, DPI, TVPI, quarterly rollforward.

## Business Rules

- **Distribution waterfall (European whole-fund)**: Return of Capital → Preferred Return (8% hurdle) → GP Catch-up (100% GP until GP = 20% of profit) → Carried Interest (80/20). Sum of LP shares + GP shares must equal distributable (reconcile). Terms (fee/carry/pref/type) come from the fund's share class in `5c`. Support an American/deal-by-deal variant as a fund attribute.
- **Capital-call allocation**: default pro-rata by each LP's commitment in the fund; editable; total must equal the call amount before the call can advance.
- **DD approval pipeline**: 9 ordered stages, each with an approver role + SLA. Approve advances to the next stage; Reject returns it (with a mandatory comment) for re-review. Escalation thresholds add approvers (e.g. amount > $20M → CIO + Compliance; cross-fund allocation → Legal; wire to a new bank account → dual authorization). Terminal stage = Completed.
- **RBAC**: the role→capability matrix (`5a`) gates read/edit/approve on every module and must drive both UI affordances and API authorization. Admin capability governs the Admin section.
- **Wire lifecycle**: Queued → Sent → Acknowledged → Settled; any failure → Exception (flagged, actionable). Wires cannot be sent to an LP without wire instructions on file (`5d`).
- **Reconciliation**: match book vs custodian per item; non-zero diff → Break; missing counterpart → Unmatched. Surface breaks for assignment/clearing.
- **Drawdowns**: the fund borrows on a facility to fund a deal quickly, then repays from the matching capital call — link drawdowns to their call/deal.
- **Investor Portal access**: a PortalContact's role (Primary/Viewer/Tax-only) + funds-visible + the tenant's capability toggles + exposed document types (`6c`) determine what the external LP can see/download. An LP sees only their own capital account across the funds they're in.
- **Auditability**: every create/approve/reject/edit/wire/config change writes an append-only, tamper-evident (`sealHash`) audit event.

## Design Tokens (placeholder — map to your system)

These are the raw values the wireframe uses. **Replace with your design system's semantic tokens**; they're listed so intent (which semantic role each color plays) is clear.
- **Ink / text**: `#1a1a1a` (primary), `#6b655c` / `#8a847a` (secondary), `#9a938a` / `#a49d92` (muted).
- **Surfaces**: `#ffffff` (card), `#faf9f6` / `#f6f3ee` (subtle fill), `#efece5` (canvas). Borders `#e2ddd3` / `#ece8e0`; dashed placeholder `#c7c2b8`.
- **Semantic accents**: primary/action **blue** `#2a6fd6`; positive **green** `#2f7d4f`; caution **amber** `#9a6a12` / `#c2410c`; danger **red** `#b91c1c`.
- **Status pills** (map to badge component): Scheduled=neutral grey, Pending=amber, Wired/Settled/Active/Matched=green, Confirmed/In-review=blue, Exception/Break/Failed=red.
- **Type**: UI = Inter (system-ui fallback). Numbers/refs/hashes = a monospace face with **tabular figures** for all money/percent columns. (The **Caveat** script font is annotation-only — do not ship.)
- **Radius**: ~6–9px on cards/inputs, pill = fully rounded. **Shadow/border**: replace the wireframe's hard `6px 8px` offset shadow and sketch borders with your elevation system.
- **Density**: this is a data-dense internal tool — compact tables, ~11–12px body in the mockups; use your system's comfortable-but-dense table scale. Minimum 44px touch targets if any surface is touch.

## Assets
No real assets are used. Image/logo/screenshot areas are represented as **dashed placeholder boxes**; document rows use a simple rectangle glyph. Charts (portfolio value, cash forecast, exposure bars) are **CSS placeholders** — implement with your charting library. Icons are simple CSS shapes/Unicode in the wireframe — use your icon set. There are no brand assets to preserve.

## Suggested component inventory
Build once, reuse everywhere (map to your library):
- **AppShell** + **TreeNav** (collapsible, persisted, active state, route-driven) — internal; **PortalShell** + top bar + **SideNav** — external.
- **DataTable** — the workhorse: sortable, filterable, paginated/virtualized, sticky header, right-aligned **tabular-number** money cells, grouped/expandable rows (`2a`), row-click navigation, inline-editable cells (`2b`/`2c` allocations), totals row.
- **KpiTile** / **StatStrip**, **Breadcrumb**, **Tabs**, **SectionHeader**, **Avatar**.
- **StatusPill** (neutral / amber / green / blue / red) and **Money** / **Percent** cell (tabular, signed coloring).
- **Pipeline / Stepper** (read-only instance for `2b`; editable config for `5b`).
- **Modal** (form modal; **Wizard** modal `2c` with per-step validation + live reconcile), **ConfirmDialog** (`7b`), **Toast** (success / error + undo).
- **FilterChips**, **SegmentedControl**, **Toggle**, **PermissionSegment** (none / V / E / A), **CapabilityMatrix** (`5a`).
- **FileList** / **DropZone** / **DownloadRow**; **Chart**s (value-trend bars, stacked-exposure bar, progress + forecast bars — swap CSS placeholders for your charting lib).
- **State views**: **EmptyState**, **LoadingSkeleton**, **ErrorState**, **NoPermission**, **ZeroResults** (`7a`); **WaterfallLedger** (`4a`), **Rollforward** table (`6e`).

## Suggested routing map
Internal app (every route guarded by the capability matrix):
- `/portfolio` (`6a`) · `/portfolio/deals/:dealId` (`6d`)
- `/capital-calls` (`2a`) · `/capital-calls/:callId` (`2b`) · `/capital-calls/new` (`2c`)
- `/distributions` (+ `/distributions/:distId`) (`4a`) · `/drawdowns` (`4b`) · `/wires` (`4c`) · `/cash` (`4d`) · `/reconciliation` (`4e`)
- `/admin/users` (`5a`) · `/admin/workflows` (`5b`) · `/admin/funds` (`5c`) · `/admin/investors` (`5d`) · `/admin/reference` (`5e`) · `/admin/integrations` (`5f`) · `/admin/notifications` (`5g`) · `/admin/audit` (`5h`) · `/admin/investor-access` (`6c`)

External portal (separate auth surface / subdomain, scoped to the logged-in LP):
- `/portal` (`6b`) · `/portal/investments` (`6e`) · `/portal/activity` (`6g`) · `/portal/statements` (`6f`) · `/portal/tax` (`6h`) · `/portal/contact` (`6i`)

## API surface (shape sketch — confirm real contracts)
Illustrative, not final. Every mutation must enforce RBAC and emit an audit event server-side.
- Portfolio/deals: `GET /portfolio/summary`, `GET /deals`, `GET /deals/:id`, `GET /deals/:id/cashflows`
- Capital calls: `GET /capital-calls?groupBy=transaction`, `GET /capital-calls/:id`, `POST /capital-calls`, `PATCH /capital-calls/:id/allocations`, `POST /capital-calls/:id/approve {comment}`, `POST /capital-calls/:id/reject {comment}`
- Distributions: `GET /distributions/:id` (tiers + payouts), `POST /distributions`
- Ops: `GET /drawdowns`, `GET /wires`, `GET /cash/position`, `GET /cash/forecast`, `GET /reconciliation?type=cash`
- Admin: CRUD under `/admin/users`, `/admin/roles` (+ matrix), `/admin/funds`, `/admin/investors`, `/admin/reference/*`, `/admin/integrations`, `/admin/notification-rules`; `GET /admin/audit`; `GET|PATCH /admin/investor-access`
- Portal (scoped to authed LP): `GET /portal/accounts`, `GET /portal/activity`, `GET /portal/statements`, `GET /portal/tax`, `POST /portal/messages`

## Acceptance criteria (key flows)
- **Approve / Reject a call (`2b`)**: only an approver for the current stage sees the actions; both require a comment; Approve advances the stage and notifies the next approver; Reject returns per workflow rules; both append an audit event; escalation thresholds inject required approvers.
- **New-call wizard (`2c`)**: allocations default pro-rata by commitment and stay editable; the total must equal the call amount before Continue; submit creates the call at stage 1 and generates notices.
- **Distribution (`4a`)**: from distributable + the fund's share-class terms, compute tiers; LP + GP shares reconcile to distributable; per-investor payouts are pro-rata by ownership.
- **Wires (`4c` / `5d`)**: an LP with no wire instructions on file cannot be wired to; failed wires become actionable Exceptions.
- **Reconciliation (`4e`)**: diff ≠ 0 → Break; missing counterpart → Unmatched; both are assignable / clearable.
- **Portal access (`6c`)**: an LP sees only their own funds/accounts; a Tax-only contact sees only Tax; a Disabled contact can't sign in; downloads are gated by exposed-document-types + capability.
- **Cross-cutting**: every list/detail screen implements all five `7a` states; every mutation shows a toast and, if destructive / workflow-affecting, a confirm dialog first.

## Accessibility
- Full keyboard support: tree nav, table row focus + sortable-header state, modal focus-trap + ESC, wizard step nav.
- Semantic tables with real `<th>`; associate form labels + validation via `aria-describedby`; status is never color-only (pills carry text — keep it).
- Re-check contrast when mapping tokens — the wireframe's muted greys (e.g. `#9a938a`) may fail AA on white and should darken.
- Money / percent cells use tabular figures with screen-reader-friendly labels for sign & currency.

## Screen index (28)
- **Portfolio & investor**: `6a` Portfolio · `6d` Deal detail · `6b` Portal Overview · `6e` My Investments · `6f` Statements · `6g` Capital Activity · `6h` Tax Documents · `6i` Contact IR · `6c` Investor Access (admin)
- **Capital calls**: `2a` Blotter · `2b` Detail / DD · `2c` New call
- **Distributions & fund ops**: `4a` Distributions · `4b` Drawdowns · `4c` Wire Status · `4d` Cash Position · `4e` Reconciliation
- **Administration**: `5a` Users & Roles · `5b` Approval Workflows · `5c` Funds & Entities · `5d` Investor Registry · `5e` Reference Data · `5f` Integrations · `5g` Notifications · `5h` Audit Log · `5i` Action dialogs
- **Patterns & states**: `7a` Screen states · `7b` Feedback patterns

## Files
- `Meridian Capital Ops - Wireframes (standalone).html` — **start here.** A single self-contained file: open it in any browser (double-click, no server) to see and interact with all 28 screens — collapse the tree nav, open the `2b` approve/reject modal, collapse the `2b` audit panel. This is the visual reference in lieu of screenshots.
- `Capital Calls Blotter Wireframes.dc.html` — the same design as editable source (28 screen cards). Each screen is a `.dv-opt` block with its id badge; section headers are `.dv-turn` (ids `t0`, `t6`, `t2`, `t4`, `t5`, `t7`). Requires `support.js` alongside it.
- `support.js` — prototype runtime only (**do not port**).

> **No static screenshots are bundled** — the standalone HTML is a better reference (inspect any screen live, read computed layout). To export images/PDF, open the standalone file and use the browser's Print → Save as PDF.

## Known gaps / explicitly out of scope
The following were intentionally not drawn — confirm requirements before building:
- Investor Portal secure-messaging back-end/inbox threading for `6i` (the form + ticket list are drawn; the messaging service is not specified).
- Detailed **sort / filter / search / pagination** behavior specs (chips are illustrative).
- **Responsive / mobile** layouts (mockups are fixed-width desktop).
- **Authentication / SSO / session** and the external portal's auth boundary.
- Real **charting**, multi-currency display/formatting, and i18n.
- Numbers, names, and dates are **illustrative sample data**, not authoritative.
