# Workspace guide (read me first)

Five things live in this repository:

1. **`meridian-capital-ops/`** — the product front-end: a Next.js 16 app for a
   private-credit fund-operations platform (capital calls, distributions,
   wires, reconciliation, admin, investor portal). **All frontend work happens
   here.** Its own `CLAUDE.md`/`AGENTS.md` and `docs/` folder carry the working
   rules — start with `meridian-capital-ops/docs/STATUS.md`, then `README.md`.
2. **`backend/`** — the .NET 8 API (clean architecture, controllers, EF Core +
   Dapper, Quartz jobs, OData, xUnit with an integration-test emphasis). **All
   backend work happens here** — start with `backend/README.md`. It implements
   the full `meridian-capital-ops/docs/API.md` contract; persistence is Azure
   SQL (`Database:Provider=SqlServer`) or a self-seeded in-memory SQLite store
   for dev/tests.
3. **`database/`** — the SQL database project (SDK-style, builds to a dacpac,
   deploys to Azure SQL). Owns the schema — non-dbo schemas, temporal tables
   with audit columns and `IsActive` — plus the post-deployment seed generated
   from the frontend mocks. **All database work happens here** — start with
   `database/README.md`; table files and seeds are generated (see its tools/),
   never hand-edited.
4. **`infra/`** — Azure infrastructure-as-code for the whole stack: Bicep
   templates, `dev`/`prod` environment parameter files, per-component deploy
   scripts, and GitHub Actions workflows. **All deployment/infra work happens
   here** — start with `infra/README.md`. (Each app keeps its own
   `Dockerfile` beside its code; everything Azure lives in `infra/`.)
5. **`project/` + `chats/` + root `README.md`** — the original Claude Design
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
cd backend
dotnet test            # domain unit + API integration tests — must stay green
dotnet run --project src/Meridian.Api   # http://localhost:8080 (Swagger at /swagger)

cd meridian-capital-ops
npm install
npm run dev            # http://localhost:3000 → /portfolio (internal) · /portal (LP view)
                       # fetches the backend; DATA_SOURCE=mock to run standalone on the JSON mocks
npx tsc --noEmit       # must stay clean (strict)
npm run build          # must pass before committing significant work

cd database
dotnet build Meridian.Database.sqlproj  # → bin/Debug/Meridian.Database.dacpac — must build clean
```

## Ground rules

- **Strive for code simplicity and reusability.** Prefer the smallest change
  that works; reuse the shared UI kit (`src/components/ui/`), the data-layer
  accessors, and existing patterns before writing anything new. If a new
  abstraction is needed, make it once, make it shared, and follow
  `meridian-capital-ops/docs/CONVENTIONS.md`.
- The frontend fetches the backend API through `src/lib/data.ts` (async
  accessors; `DATA_SOURCE=mock` swaps back to the JSON files in `src/mocks/`,
  which are deliberately kept — they drive mock mode AND generate the database
  seed). The backend honors `docs/API.md`, `docs/DATA_MODEL.md`,
  `docs/BUSINESS_RULES.md` (remaining build order in
  `meridian-capital-ops/docs/BACKEND_TODO.md`).
- The EF model (`backend/.../AppDbContext`) is the schema source of truth: after
  changing it, regenerate the database project's table files and seeds
  (`database/README.md` § "The schema is generated from the EF model").
- Frontend changes follow `meridian-capital-ops/docs/CONVENTIONS.md`
  (server page → client screen pattern, shared UI kit, semantic tokens only).
- Mock data is one internally consistent story (see DATA_MODEL.md
  "Cross-screen consistency") — keep it consistent when editing, keep the
  story "today" = **2026-07-05** (`TODAY` in `src/lib/format.ts`, `BusinessDate`
  in the backend), and regenerate `database/scripts/seed/` after editing mocks.
