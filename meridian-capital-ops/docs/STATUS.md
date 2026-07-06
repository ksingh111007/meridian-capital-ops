# Project status — session handoff snapshot

_Last updated: 2026-07-06 (end of the initial build session)._

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

Frontend: real and interactive. Backend: entirely mocked — one JSON per
endpoint in `src/mocks/`, typed accessors in `src/lib/data.ts`, HTTP mirror at
`/api/[...endpoint]`. Mutations acknowledge but don't persist (screens update
optimistically + toast). No authentication; no RBAC route enforcement
(matrix exists as data and gates the 2b approve buttons only).

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
