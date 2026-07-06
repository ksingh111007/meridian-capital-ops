# Workspace guide (read me first)

Two things live in this repository:

1. **`meridian-capital-ops/`** — the product: a Next.js 16 front-end for a
   private-credit fund-operations platform (capital calls, distributions,
   wires, reconciliation, admin, investor portal). **All work happens here.**
   Its own `CLAUDE.md`/`AGENTS.md` and `docs/` folder carry the working rules —
   start with `meridian-capital-ops/docs/STATUS.md`, then `README.md`.
2. **`project/` + `chats/` + root `README.md`** — the original Claude Design
   wireframe handoff the app was built from. **Reference only — never edit.**
   The HTML prototypes are not production code; `project/support.js` must not
   be ported. The design's developer spec is
   `project/design_handoff_capital_calls_blotter/README.md`.

The app implements all 28 wireframe screens plus three reviewed improvements
(needs-attention inbox, overdue call tracking with pro-rata-by-unfunded
allocations, distribution payouts tab with actionable exceptions). The visual
style is a clean institutional design system — the wireframes' sketch
aesthetic was deliberately discarded per the handoff instructions.

## Quick commands

```bash
cd meridian-capital-ops
npm install
npm run dev            # http://localhost:3000 → /portfolio (internal) · /portal (LP view)
npx tsc --noEmit       # must stay clean (strict)
npm run build          # must pass before committing significant work
```

## Ground rules

- The backend does not exist yet. Every API call is mocked by one JSON file in
  `meridian-capital-ops/src/mocks/` behind `src/lib/data.ts`. Backend work
  starts at `meridian-capital-ops/docs/BACKEND_TODO.md` and must honor
  `docs/API.md`, `docs/DATA_MODEL.md`, `docs/BUSINESS_RULES.md`.
- Frontend changes follow `meridian-capital-ops/docs/CONVENTIONS.md`
  (server page → client screen pattern, shared UI kit, semantic tokens only).
- Mock data is one internally consistent story (see DATA_MODEL.md
  "Cross-screen consistency") — keep it consistent when editing, and keep the
  mock "today" = **2026-07-05** (`TODAY` in `src/lib/format.ts`).
