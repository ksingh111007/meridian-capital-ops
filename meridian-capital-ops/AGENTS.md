<!-- BEGIN:nextjs-agent-rules -->
# This is NOT the Next.js you know

This version has breaking changes — APIs, conventions, and file structure may all differ from your training data. Read the relevant guide in `node_modules/next/dist/docs/` before writing any code. Heed deprecation notices.
<!-- END:nextjs-agent-rules -->

# Working on Meridian Capital Ops

A private-credit fund-ops front-end. Data comes from the .NET API in
`../backend` by default (`src/lib/data.ts` = async fetches; start the backend
per `../backend/README.md`); `DATA_SOURCE=mock` serves the JSON mocks instead
(no backend needed — see `.env.example`).
**Read `docs/STATUS.md` first** for where things stand, then:

- `docs/CONVENTIONS.md` — the screen pattern (server `page.tsx` → `"use client"`
  screen in `src/screens/`), the shared UI kit, styling rules. Follow it for
  every change; reuse `src/components/ui/*` instead of adding new primitives.
- `docs/API.md` + `docs/DATA_MODEL.md` + `docs/BUSINESS_RULES.md` — the backend
  contract. `src/lib/types.ts` is authoritative for shapes.
- `docs/BACKEND_TODO.md` — the backend build order and acceptance criteria.
- `docs/DESIGN.md` — tokens & status→tone mapping (extend `statusTone()` in
  `src/components/ui/Pill.tsx` for new statuses; never ad-hoc colors).

Hard rules learned in the build (Next.js 16):

- `params` / `searchParams` are **Promises** — always `await` them.
- No `next lint`; verify with `npx tsc --noEmit` (strict, must stay clean) and
  `npm run build` (Turbopack).
- Amounts everywhere are **USD millions**; format via `src/lib/format.ts`;
  money cells right-aligned tabular (`.num`).
- Story "today" is **2026-07-05** (`TODAY` in format.ts; the backend pins
  `BusinessDate` to match) — overdue/due-soon math depends on it. Keep
  `src/mocks/*.json` cross-screen consistent (DATA_MODEL.md § Cross-screen
  consistency): the mocks still drive mock mode AND generate the database seed
  (`../database/tools/generate-seed.mjs`) — regenerate the seed after editing.
- Mutations POST to `/api/*` (proxied to the backend, persisted + audited
  server-side; acknowledged-only in mock mode) and screens update local state
  optimistically + toast. Keep that pattern.
- The default signed-in staff user is `u-admin` (Administrator — full
  capability matrix, can act on any approval stage; set `MERIDIAN_API_USER`
  to demo a specific role, e.g. `u-jokafor` for Legal on
  `/capital-calls/call-2041`). Mock mode still signs in J. Okafor via me.json.
