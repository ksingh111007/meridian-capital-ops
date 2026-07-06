# Design system

The wireframes were deliberately low-fi (sketch borders, handwritten
annotations). Per the handoff, that aesthetic was **discarded**; this app ships
a clean, dense, institutional look. Layout, information architecture, copy,
and flows follow the wireframes; visual styling is this system.

## Tokens (`src/app/globals.css` `@theme`)

| Role | Token | Value | Wireframe origin |
| --- | --- | --- | --- |
| Text primary | `ink` | `#1c1917` | `#1a1a1a` |
| Text secondary | `ink-secondary` / `ink-muted` | `#57534e` / `#6f6a63` | `#6b655c` / `#8a847a` (darkened for AA) |
| Text faint (labels) | `ink-faint` | `#a8a29e` | `#a49d92` |
| Canvas / fills | `canvas`, `fill`, `fill-strong` | `#f3f2ef`, `#faf9f7`, `#f2f0ec` | `#efece5`, `#faf9f6` |
| Borders | `line`, `line-strong` | `#e5e2dc`, `#d6d2ca` | `#e2ddd3`, `#c7c2b8` |
| Primary / action | `primary` (+ `-soft`, `-line`, `-strong`) | `#2a6fd6` | same |
| Positive | `positive` (+ soft/line) | `#1f7a45` | `#2f7d4f` |
| Caution | `caution`, `caution-strong` (+ soft/line) | `#92610e`, `#b45309` | `#9a6a12`, `#c2410c` |
| Danger | `danger` (+ soft/line) | `#b91c1c` | same |
| GP (waterfall) | `gp` | `#1f2937` | same |

Type: **Inter** (UI) + **JetBrains Mono** (refs/hashes); every money/percent
cell uses tabular figures (`.num`). Base size 13px; tables 12–12.5px. Radius
6–12px; soft layered shadows (`shadow-card`, `shadow-pop`) replace the
wireframe's hard offset shadow.

## Status → tone mapping

Single source of truth: `statusTone()` in `src/components/ui/Pill.tsx`.
Green = money landed / healthy / done · Blue = actively in review / in flight ·
Amber = needs a human · Red = broken/blocked · Neutral = scheduled/inactive.
Status is never color-only — pills always carry text.

## Component inventory

`src/components/ui/`: `Pill` · `Kpi`/`KpiRow` · `DataTable` (sortable, row
click, totals row, tone rows) · `Button` (primary/default/accent/danger/
dangerOutline/ghost) · `Modal` + `ConfirmDialog` · `Toast` (provider + hook) ·
`FilterChips` · `Tabs` · `Toggle` · `SearchInput` · form fields (`Field`,
`TextInput`, `TextArea`, `Select`, `SegmentPicker`) · `Pipeline` (DD stepper) ·
state views (`EmptyState`, `LoadingSkeleton`, `ErrorState`, `NoPermission`,
`ZeroResults`) · primitives (`Card`, `SectionTitle`, `Breadcrumb`, `Avatar`,
`ProgressBar`, `DefRow`, `DocIcon`) · charts (`MiniBarChart`, `StackedBar`,
`SplitBar` — CSS placeholders pending a charting lib).

`src/components/shell/`: `TreeNav` (collapsible, persisted), `TopBar` (fund
selector + needs-attention bell + user), `PortalNav`, `ScreenHeader`.

## Cross-cutting UI contracts

- **Screen states (7a)**: every list/detail screen must be able to render
  empty / loading-skeleton / error-with-retry / no-permission / zero-results
  from `ui/states.tsx`. Filterable tables render `ZeroResults` when filters
  match nothing (live today).
- **Feedback (7b)**: every mutation → toast (Undo/Retry where safe);
  destructive or workflow-affecting actions → confirm step first (for
  approve/reject the required-comment modal *is* the confirmation).
- **Accessibility**: semantic tables (`<th>`, `aria-sort`), labeled icon
  buttons, `role="switch"` toggles, modal focus trap + ESC, muted grays
  darkened from the wireframe values to hold AA on white.
