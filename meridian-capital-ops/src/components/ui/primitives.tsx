import Link from "next/link";

export function Card({ className = "", children }: { className?: string; children: React.ReactNode }) {
  return <div className={`rounded-lg border border-line bg-card shadow-card ${className}`}>{children}</div>;
}

export function SectionTitle({ children, action }: { children: React.ReactNode; action?: React.ReactNode }) {
  return (
    <div className="flex items-center justify-between px-5 pb-1.5 pt-4">
      <h2 className="text-[10.5px] font-semibold uppercase tracking-wider text-ink-faint">{children}</h2>
      {action}
    </div>
  );
}

export function Breadcrumb({ backHref, backLabel, current, sub }: { backHref: string; backLabel: string; current: string; sub?: string }) {
  return (
    <div className="flex items-baseline gap-2 text-xs text-ink-muted">
      <Link href={backHref} className="font-medium text-primary hover:underline">← {backLabel}</Link>
      <span>/</span>
      <span className="text-[15px] font-bold text-ink">{current}</span>
      {sub && <span>· {sub}</span>}
    </div>
  );
}

export function Avatar({ initials, muted = false, size = "md" }: { initials: string; muted?: boolean; size?: "md" | "lg" }) {
  return (
    <span
      className={`inline-flex flex-none items-center justify-center rounded-full font-bold ${
        size === "lg" ? "h-9 w-9 text-[11px]" : "h-6 w-6 text-[9px]"
      } ${muted ? "bg-fill-strong text-ink-faint" : "bg-gp text-white"}`}
    >
      {initials}
    </span>
  );
}

/** Thin horizontal progress bar. */
export function ProgressBar({ pct, className = "", barClass = "bg-primary" }: { pct: number; className?: string; barClass?: string }) {
  return (
    <div className={`h-1.5 overflow-hidden rounded-full bg-fill-strong ${className}`} role="progressbar" aria-valuenow={Math.round(pct)} aria-valuemin={0} aria-valuemax={100}>
      <div className={`h-full rounded-full ${barClass}`} style={{ width: `${Math.min(100, Math.max(0, pct))}%` }} />
    </div>
  );
}

/** Label/value row used in side rails and profile panels. */
export function DefRow({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <div className="flex items-center justify-between gap-3 text-xs">
      <span className="text-ink-muted">{label}</span>
      <span className="num text-right font-semibold text-ink">{children}</span>
    </div>
  );
}

/** Signed money coloring: positive green, negative plain ink. */
export function signedClass(value: number): string {
  return value > 0 ? "text-positive" : "text-ink";
}

/** Simple document row glyph. */
export function DocIcon() {
  return <span aria-hidden className="inline-block h-5 w-4 flex-none rounded-sm border-[1.5px] border-line-strong bg-fill" />;
}
