<!-- BEGIN:nextjs-agent-rules -->
# This is NOT the Next.js you know

This version has breaking changes — APIs, conventions, and file structure may all differ from your training data. Read the relevant guide in `node_modules/next/dist/docs/` before writing any code. Heed deprecation notices.
<!-- END:nextjs-agent-rules -->

# Working on Meridian Capital Ops

A private-credit fund-ops front-end (mock-data-only; backend not built yet).
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
- Mock "today" is **2026-07-05** (`TODAY` in format.ts) — overdue/due-soon math
  depends on it. Keep `src/mocks/*.json` cross-screen consistent
  (see DATA_MODEL.md § Cross-screen consistency).
- Mutations don't persist: screens update local state optimistically, POST to
  the mock `/api/*`, and show a toast. Keep that pattern until the backend lands.
- The mock signed-in staff user is J. Okafor (Counsel) — chosen so the live
  approve/reject demo on `/capital-calls/call-2041` (stage 4 = Legal) works.
  If you change `src/mocks/me.json`, keep some stage actionable for demos.
