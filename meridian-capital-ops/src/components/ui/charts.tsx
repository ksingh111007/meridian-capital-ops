/**
 * Lightweight CSS chart placeholders. Swap for a real charting library when
 * the backend lands (heights/percentages come pre-computed from the API).
 */

export function MiniBarChart({ values, label }: { values: number[]; label: string }) {
  return (
    <div aria-label={label} role="img" className="mt-2 flex h-14 items-end gap-1 rounded-md border border-line bg-fill p-1.5">
      {values.map((v, i) => (
        <span key={i} className={`flex-1 rounded-t-sm ${i === values.length - 1 ? "bg-primary" : "bg-primary/25"}`} style={{ height: `${v}%` }} />
      ))}
    </div>
  );
}

export function StackedBar({
  segments, height = 14,
}: {
  segments: { pct: number; color: string; label: string }[];
  height?: number;
}) {
  return (
    <div>
      <div className="flex overflow-hidden rounded-md" style={{ height }} role="img" aria-label={segments.map((s) => `${s.label} ${s.pct}%`).join(", ")}>
        {segments.map((s) => (
          <span key={s.label} style={{ width: `${s.pct}%`, background: s.color }} />
        ))}
      </div>
      <div className="mt-2 flex flex-wrap gap-3.5 text-[10.5px] text-ink-secondary">
        {segments.map((s) => (
          <span key={s.label} className="inline-flex items-center gap-1.5">
            <span className="h-2 w-2 rounded-sm" style={{ background: s.color }} />
            {s.label} {s.pct}%
          </span>
        ))}
      </div>
    </div>
  );
}

/** LP vs GP allocation split bar (screen 4a). */
export function SplitBar({ lpPct }: { lpPct: number }) {
  const gpPct = +(100 - lpPct).toFixed(1);
  return (
    <div className="flex h-4 overflow-hidden rounded-md" role="img" aria-label={`LP ${lpPct}%, GP ${gpPct}%`}>
      <span className="bg-primary" style={{ width: `${lpPct}%` }} />
      <span className="bg-gp" style={{ width: `${gpPct}%` }} />
    </div>
  );
}
