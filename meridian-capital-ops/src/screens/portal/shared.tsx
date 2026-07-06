"use client";

import { money } from "@/lib/format";
import { useToast } from "@/components/ui/Toast";
import { Card } from "@/components/ui/primitives";

/** Portal page header — simpler than the internal ScreenHeader (see 6b spec). */
export function PortalPageHeader({ title, subtitle }: { title: string; subtitle: string }) {
  return (
    <header>
      <h1 className="text-[17px] font-bold tracking-tight text-ink">{title}</h1>
      <p className="mt-0.5 text-[11px] text-ink-muted">{subtitle}</p>
    </header>
  );
}

/** Uppercase section title sitting on the portal's bg-fill main area. */
export function PortalSection({ children, action }: { children: React.ReactNode; action?: React.ReactNode }) {
  return (
    <div className="mb-1.5 mt-5 flex items-baseline justify-between">
      <h2 className="text-[10.5px] font-semibold uppercase tracking-wider text-ink-faint">{children}</h2>
      {action}
    </div>
  );
}

/** Equal cells divided by dashed borders inside one Card (overview / activity strips). */
export function StatStrip({ stats }: { stats: { label: string; value: string; valueClass?: string }[] }) {
  return (
    <Card className="flex flex-wrap overflow-hidden">
      {stats.map((s) => (
        <div key={s.label} className="min-w-28 flex-1 border-r border-dashed border-line px-4 py-3 last:border-0">
          <div className="text-[9.5px] font-semibold uppercase tracking-wider text-ink-faint">{s.label}</div>
          <div className={`num mt-1 text-[15px] font-bold leading-none ${s.valueClass ?? "text-ink"}`}>{s.value}</div>
        </div>
      ))}
    </Card>
  );
}

/** Signed money: positive green with +, negative plain ink with − (LP outflow). */
export function SignedAmount({ value }: { value: number }) {
  return (
    <span className={`num font-semibold ${value > 0 ? "text-positive" : "text-ink"}`}>
      {money(value, { sign: true })}
    </span>
  );
}

/** Small primary-soft download button — mock download acknowledged with a toast. */
export function DownloadButton({ name, label = "↓ Download" }: { name: string; label?: string }) {
  const toast = useToast();
  return (
    <button
      type="button"
      aria-label={`Download ${name}`}
      onClick={() => toast.push({ kind: "success", title: "Download started", detail: name })}
      className="whitespace-nowrap rounded-md border border-primary-line bg-primary-soft px-2 py-0.5 text-[10.5px] font-semibold text-primary transition-colors hover:border-primary"
    >
      {label}
    </button>
  );
}
