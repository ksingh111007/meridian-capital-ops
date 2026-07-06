"use client";

import { Button } from "./Button";

/**
 * The shared screen-state contract (pattern 7a): every list/detail screen
 * renders exactly these for empty / loading / error / no-permission / zero-results.
 */

function StateFrame({ children }: { children: React.ReactNode }) {
  return <div className="flex min-h-44 flex-col items-center justify-center gap-2 px-6 py-10 text-center">{children}</div>;
}

export function EmptyState({ title, message, actionLabel, onAction }: { title: string; message: string; actionLabel?: string; onAction?: () => void }) {
  return (
    <StateFrame>
      <span aria-hidden className="mb-1 h-10 w-10 rounded-lg border-2 border-dashed border-line-strong" />
      <div className="text-sm font-bold text-ink">{title}</div>
      <div className="max-w-60 text-xs text-ink-muted">{message}</div>
      {actionLabel && <Button variant="primary" className="mt-1" onClick={onAction}>{actionLabel}</Button>}
    </StateFrame>
  );
}

export function LoadingSkeleton({ rows = 5 }: { rows?: number }) {
  return (
    <div aria-busy className="flex flex-col gap-3 px-6 py-8">
      <div className="h-2.5 w-1/2 animate-pulse rounded bg-fill-strong" />
      {Array.from({ length: rows }, (_, i) => (
        <div key={i} className="h-2 animate-pulse rounded bg-fill-strong" style={{ width: `${[100, 100, 82, 94, 70][i % 5]}%` }} />
      ))}
    </div>
  );
}

export function ErrorState({ message = "Something went wrong on our end.", onRetry }: { message?: string; onRetry?: () => void }) {
  return (
    <StateFrame>
      <span className="mb-1 flex h-9 w-9 items-center justify-center rounded-full bg-danger-soft text-lg font-bold text-danger">!</span>
      <div className="text-sm font-bold text-ink">Couldn’t load</div>
      <div className="max-w-60 text-xs text-ink-muted">{message}</div>
      {onRetry && <Button variant="dangerOutline" className="mt-1" onClick={onRetry}>Retry</Button>}
    </StateFrame>
  );
}

export function NoPermission({ area }: { area: string }) {
  return (
    <StateFrame>
      <span aria-hidden className="mb-1 text-2xl">🔒</span>
      <div className="text-sm font-bold text-ink">Restricted</div>
      <div className="max-w-60 text-xs text-ink-muted">You don’t have access to {area}.</div>
      <button type="button" className="mt-1 text-xs font-semibold text-primary hover:underline">Request access</button>
    </StateFrame>
  );
}

export function ZeroResults({ message = "No rows match these filters.", onClear }: { message?: string; onClear?: () => void }) {
  return (
    <StateFrame>
      <span aria-hidden className="mb-1 text-2xl text-ink-faint">⌀</span>
      <div className="text-sm font-bold text-ink">No matches</div>
      <div className="max-w-60 text-xs text-ink-muted">{message}</div>
      {onClear && <button type="button" onClick={onClear} className="mt-1 text-xs font-semibold text-primary hover:underline">Clear filters</button>}
    </StateFrame>
  );
}
