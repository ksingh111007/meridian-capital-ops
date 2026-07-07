# Workspace guide (read me first)

Three things live in this repository:

1. **`meridian-capital-ops/`** — the product front-end: a Next.js 16 app for a
   private-credit fund-operations platform (capital calls, distributions,
   wires, reconciliation, admin, investor portal). **All frontend work happens
   here.** Its own `CLAUDE.md`/`AGENTS.md` and `docs/` folder carry the working
   rules — start with `meridian-capital-ops/docs/STATUS.md`, then `README.md`.
2. **`backend/`** — the .NET 8 API (clean architecture, controllers, EF Core +
   Dapper over in-memory SQLite, Quartz jobs, OData, xUnit with an
   integration-test emphasis). **All backend work happens here** — start with
   `backend/README.md`. It implements `meridian-capital-ops/docs/API.md`
   beginning with the capital-call approval pipeline; the frontend still runs
   on its own mocks until the swap described in ARCHITECTURE.md §Swap.
3. **`project/` + `chats/` + root `README.md`** — the original Claude Design
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

cd backend
dotnet test            # domain unit + API integration tests — must stay green
dotnet run --project src/Meridian.Api   # http://localhost:8080 (Swagger at /swagger)
```

## Ground rules

- **Strive for code simplicity and reusability.** Prefer the smallest change
  that works; reuse the shared UI kit (`src/components/ui/`), the data-layer
  accessors, and existing patterns before writing anything new. If a new
  abstraction is needed, make it once, make it shared, and follow
  `meridian-capital-ops/docs/CONVENTIONS.md`.
- The frontend still runs entirely on mocks: every API call is mocked by one
  JSON file in `meridian-capital-ops/src/mocks/` behind `src/lib/data.ts`.
  The backend in `backend/` implements a growing subset of the contract
  (build order in `meridian-capital-ops/docs/BACKEND_TODO.md`) and must honor
  `docs/API.md`, `docs/DATA_MODEL.md`, `docs/BUSINESS_RULES.md`; keep its seed
  data consistent with the mock story.
- Frontend changes follow `meridian-capital-ops/docs/CONVENTIONS.md`
  (server page → client screen pattern, shared UI kit, semantic tokens only).
- Mock data is one internally consistent story (see DATA_MODEL.md
  "Cross-screen consistency") — keep it consistent when editing, and keep the
  mock "today" = **2026-07-05** (`TODAY` in `src/lib/format.ts`).
