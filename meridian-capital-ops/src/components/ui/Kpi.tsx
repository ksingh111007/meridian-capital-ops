/** KPI tile strip shown under a screen header. */
export function KpiRow({ children }: { children: React.ReactNode }) {
  return <div className="grid grid-cols-2 gap-3 border-b border-line bg-fill px-5 py-3.5 lg:grid-cols-4">{children}</div>;
}

export function Kpi({
  label, value, sub, tone = "default",
}: {
  label: string;
  value: React.ReactNode;
  sub?: string;
  tone?: "default" | "primary" | "positive" | "caution" | "danger";
}) {
  const toneCls = { default: "text-ink", primary: "text-primary", positive: "text-positive", caution: "text-caution-strong", danger: "text-danger" }[tone];
  return (
    <div className="min-w-0 rounded-lg border border-line bg-card px-3.5 py-2.5 shadow-card">
      <div className="truncate text-[10px] font-semibold uppercase tracking-wider text-ink-faint">{label}</div>
      <div className={`num mt-1 text-lg font-bold tracking-tight ${toneCls}`}>{value}</div>
      {sub && <div className="mt-0.5 truncate text-[11px] text-ink-muted">{sub}</div>}
    </div>
  );
}
