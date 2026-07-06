# Frontend conventions

How every screen in this app is built. Follow these when adding screens.

## Screen pattern

1. **`src/app/(internal)/<route>/page.tsx`** — server component. Calls the data layer (`src/lib/data.ts`), passes plain data into a screen component. Add `export const metadata = { title: "…" }`. Dynamic routes: `params` is a **Promise** (Next 16) — `const { id } = await params;` and add `generateStaticParams()`.
2. **`src/screens/<Name>Screen.tsx`** — `"use client"` component owning all interactivity (filters, tabs, modals, sorting). Receives data as props; never imports from `src/mocks/` directly.
3. Portal screens live in `src/app/portal/<route>/page.tsx` (the portal layout provides its own shell).

Screens never fetch GET data from `/api/*` — the server page already has it. Mutations (approve, create, retry) POST to `/api/...` (mock acknowledges; update local state optimistically and show a toast).

## Components (src/components)

- `shell/ScreenHeader` — title + context note + right-side actions. Every internal screen starts with this.
- `ui/Kpi` — `<KpiRow><Kpi label value sub tone/>…</KpiRow>` under the header.
- `ui/DataTable` — typed columns (`render`, `sortValue` for sortable, `align: "right"` for money), `rowKey`, `onRowClick`, `rowClass` for tone-highlighted rows, `totalRow` (use `<Td>`s), `emptyState`. Money columns always `align: "right"` (adds `num` tabular figures).
- `ui/Pill` — status pill; tone auto-derives from the status string via `statusTone()`. Extend that map rather than passing ad-hoc tones.
- `ui/controls` — `FilterChips`, `Tabs`, `Toggle`, `SearchInput`, `Field`, `TextInput`, `TextArea`, `Select`, `SegmentPicker`.
- `ui/Modal` — form modal; `ConfirmDialog` for destructive/workflow actions (pattern 7b).
- `ui/Toast` — `useToast().push({ kind, title, detail, actionLabel, onAction })`. Every mutation shows one.
- `ui/states` — `EmptyState`, `LoadingSkeleton`, `ErrorState`, `NoPermission`, `ZeroResults` (pattern 7a). Filterable tables must render `ZeroResults` when filters match nothing.
- `ui/primitives` — `Card`, `SectionTitle`, `Breadcrumb`, `Avatar`, `ProgressBar`, `DefRow`, `DocIcon`.
- `ui/charts` — `MiniBarChart`, `StackedBar`, `SplitBar` (CSS placeholders; real chart lib comes with the backend).
- `ui/Pipeline` — the 9-stage DD stepper.

## Style rules

- Tailwind only, using the semantic tokens from `globals.css` (`ink`, `ink-muted`, `line`, `fill`, `primary`, `positive`, `caution`, `danger`, …). No raw hex in screens.
- Money/percent formatting via `src/lib/format.ts` (`money`, `pct`, `multiple`, `fmtDate`, `daysUntil`). Amounts are USD millions.
- Dense tables: `text-xs`/`text-[12.5px]`, `py-2` cells. Uppercase 10px tracking-wide section titles.
- Row highlight tones: exceptions/breaks `bg-danger-soft/40`, warnings `bg-caution-soft/40`, selected `bg-primary-soft/40`.
- Accessibility: real `<th>`, `aria-label` on icon buttons, `role="switch"` toggles, status never color-only (pills carry text).

## Mock data

One JSON in `src/mocks/` per API endpoint; typed accessors in `src/lib/data.ts`; the `/api/[...endpoint]` route exposes the same data over HTTP. The mock "today" is **2026-07-05** (`TODAY` in format.ts). Keep cross-screen numbers consistent (e.g. wire W-8847 = the Granite State payout exception on distribution #D-119 = the recon break's batch).
