# Architecture

## Two shells, two audiences

| Shell | Routes | Audience | Auth (production) |
| --- | --- | --- | --- |
| **Internal** — `src/app/(internal)/` | `/portfolio`, `/capital-calls`, `/distributions`, `/drawdowns`, `/wires`, `/cash`, `/reconciliation`, `/admin/*` | Fund staff | Staff SSO; every route guarded by the role→capability matrix |
| **Portal** — `src/app/portal/` | `/portal/*` | External LPs | Separate auth surface (own session/subdomain); an LP sees only their own capital account |

The internal shell provides a **collapsible tree nav** (`TreeNav` — group state
persisted in `localStorage`, key `mco.nav.collapsed`; move to user preferences
in production) and a **top bar** with a persistent fund selector, the
needs-attention bell, and the current user. The portal shell is a lighter top
bar + flat side nav.

In the mock there is **no authentication**: the internal "current user" is
J. Okafor (Counsel) from `src/mocks/me.json` — chosen so the live approval demo
on `/capital-calls/call-2041` (stage 4 = Legal) is enabled — and the portal is
hard-scoped to Redwood Pension. The RBAC matrix exists as data (`users.json`)
and gates the 2b approve/reject affordance, but **route-level enforcement is
backend work** (see BACKEND_TODO.md).

## Screen pattern (how every page is wired)

```
src/app/(internal)/<route>/page.tsx     server component
  └─ calls src/lib/data.ts accessors    (sync JSON reads today; async fetches later)
  └─ renders <XyzScreen data={…}/>      passes plain serializable props
src/screens/XyzScreen.tsx               "use client" — filters, tabs, sorting, modals, toasts
```

- GET data never round-trips through `/api/*` — the server component already
  has it. The `/api/[...endpoint]` route exposes the same data over HTTP so the
  contract is exercisable (curl, tests, later client-side fetching).
- Mutations (`approve`, `reject`, create call, wire retry, recon assign, portal
  message) POST to `/api/...`; the mock acknowledges without persisting and the
  screen updates local state optimistically + shows a toast.
- Dynamic routes use Next 16 **async params**: `const { id } = await params`.

## Swapping in the real backend

Status 2026-07-07: steps 1–2 are **done** (`src/lib/data.ts` is async fetches to
the ../backend API; `DATA_SOURCE=mock` keeps the JSON fallback), and
`src/app/api/[...endpoint]` is now a proxy so screen mutations persist. Still
open, deliberately:

3. Replace the optimistic mutation handlers in screens with real POSTs +
   revalidation (`router.refresh()` or tagged cache invalidation) — today the
   POSTs persist server-side but screens still update local state.
4. Delete `src/mocks/` and the proxy's mock mode once nothing references them —
   **not yet**: the mocks still drive mock mode and generate the database seed
   (`../database/tools/generate-seed.mjs`).
5. Add route guards: internal layout resolves the session user + capability
   matrix; portal layout resolves the LP session. Render the `NoPermission`
   state (`src/components/ui/states.tsx`) on failure. (RBAC is already enforced
   server-side; the default `u-admin` dev principal sees every screen.)

## State management

- **Server data**: fetched in server components per request (today: sync JSON).
- **UI state**: local `useState` in screens (filters, tabs, expanded groups,
  wizard step). Nothing global except toasts (`ToastProvider`) and nav collapse
  (localStorage).
- **Session**: `getCurrentUser()` (internal) / `getPortalAccount()` (portal) —
  replace with real session lookups.

## Known intentional simplifications

- Mutations don't persist (no backend) — a page refresh restores mock data.
- The fund selector in the top bar is contextual chrome only; screens render
  their mock fund scope. Wire it to a query param / context when data is real.
- Charts are CSS placeholder bars (`src/components/ui/charts.tsx`) — swap for a
  charting library when real time-series arrive.
- Fixed desktop-first layout (min ~1100px comfortable); tables scroll
  horizontally on narrow viewports. Full responsive/mobile is out of scope, as
  in the wireframes.
