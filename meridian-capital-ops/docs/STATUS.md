# Project status — session handoff snapshot

_Last updated: 2026-07-07 (end of the database-integration session)._

## 2026-07-07 session — real database + full wiring

The stack is now wired end to end: **Azure SQL** (new `../database` dacpac
project: non-dbo schemas, temporal tables with audit columns + `IsActive`,
post-deployment seed generated from `src/mocks/`) → **backend** (full docs/API.md
contract implemented; `Database:Provider=SqlServer` for Azure SQL, self-seeded
in-memory SQLite for dev/tests; 90 tests green) → **frontend** (`src/lib/data.ts`
is async fetches to the API; `DATA_SOURCE=mock` env flag restores the old
JSON-import behavior — `src/mocks/` is intentionally kept for testing and still
drives both the mock mode and the generated database seed). The `/api/*` catch-all
is now a proxy, so screen mutations (approve/reject, create call, wire retry,
recon assign, portal message) persist server-side. Dev principals: staff
`MERIDIAN_API_USER` (default `u-admin`), portal `MERIDIAN_PORTAL_CONTACT`
(default `pc-1`); see `.env.example`. Backend quick-start + Azure deployment:
`../backend/README.md` and `../database/README.md`.

## Where things stand

**Complete and verified.** All 28 wireframe screens from the design handoff
are implemented as 25 routes (internal app + investor portal), plus the three
improvements agreed with the owner during design review:

1. Needs-attention inbox (Portfolio-home strip + top-bar bell, "YOU" tag on
   items awaiting the current user).
2. Overdue call tracking (`Overdue` wire status, aging badges on the blotter,
   overdue KPI; wizard defaults to pro-rata by **unfunded** commitment with a
   basis selector).
3. Distributions per-investor payouts tab (wire-instructions gate → `Blocked`
   payouts) and actionable exceptions (wire retry/resolve, recon assign/clear).

**Verification at handoff**: `npx tsc --noEmit` clean · `npm run build` passes
· all 25 routes return 200 · key screens visually reviewed via headless
Chromium (portfolio, blotter, DD detail, distributions, portal, admin).

**Git**: history is `c8f5f5c` (design handoff bundle) → `a4e22de` (app)
→ this docs commit, on `main`. Intended GitHub remote:
`https://github.com/ksingh111007/meridian-capital-ops` — the initial-build
session could not push (its credential had no repo scope; the owner pushes via
a git bundle). **A new session started from that GitHub repo can push
normally.** Verify with `git remote -v` + `git push --dry-run` early.

## What is real vs. mocked

Frontend: real and interactive, fetching the .NET API by default
(`DATA_SOURCE=mock` falls back to the JSON mocks). Backend: real — full
docs/API.md contract with server-side RBAC, hash-chained audit, and Azure
SQL/SQLite persistence; screen mutations persist through the `/api/*` proxy
(screens still also update optimistically + toast). Still stand-ins:
authentication (dev `X-User-Id` header scheme for both staff and portal
contacts — swap to SSO per BACKEND_TODO step 1), admin CRUD dialogs (toasts
only; no endpoints yet), document downloads, and the waterfall engine
(distributions are seeded reads).

## Likely next work (in rough priority)

1. **Backend** — follow `docs/BACKEND_TODO.md` (build order + acceptance
   criteria); contract in `docs/API.md` / `docs/DATA_MODEL.md` /
   `docs/BUSINESS_RULES.md`; swap procedure in `docs/ARCHITECTURE.md` §
   "Swapping in the real backend".
2. **Auth + RBAC enforcement** — staff SSO, portal auth boundary, route guards
   rendering the `NoPermission` state.
3. **Smaller frontend follow-ups** (deliberately deferred):
   - Wire the top-bar fund selector to actual data scoping (cosmetic today).
   - Replace CSS chart placeholders (`src/components/ui/charts.tsx`) with a
     charting library.
   - Server-side sort/filter/pagination once data outgrows the mocks.
   - Responsive/mobile layouts (desktop-first today, per the design scope).
   - Deal-detail Covenants/Documents tabs are thin (mock has minimal data).
4. **Design reference** — the original wireframes live in `../project/`
   (repo root); treat as read-only provenance.

## Gotchas for the next agent

- Read `AGENTS.md` (this folder) before coding — Next.js 16 breaking changes
  (async `params`, Turbopack, no `next lint`) plus project hard rules.
- Keep `src/mocks/` internally consistent (DATA_MODEL.md § Cross-screen
  consistency) and the mock clock at 2026-07-05.
- `statusTone()` in `src/components/ui/Pill.tsx` is the single status→color
  map — extend it there.
- The three parallel screen groups (fund-ops / admin / portal) were built
  against the same conventions; if you spot drift between screens, prefer the
  patterns in `PortfolioScreen.tsx` / `CallDetailScreen.tsx`.
