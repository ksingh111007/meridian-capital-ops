/**
 * Read-only due-diligence pipeline stepper (screen 2b).
 * Status dots, not numbers: ✓ done · solid dot in ring = current · faint dot = pending.
 */
import type { StageEvent, WorkflowStage } from "@/lib/types";

export function Pipeline({ stages, events }: { stages: WorkflowStage[]; events: StageEvent[] }) {
  const byStage = new Map(events.map((e) => [e.stage, e]));
  return (
    <ol className="flex overflow-x-auto border-b border-line bg-fill px-3 pb-4 pt-5" aria-label="Due-diligence pipeline">
      {stages.map((s, i) => {
        const ev = byStage.get(s.order);
        const state: StageEvent["state"] = ev?.state ?? "pending";
        const connectorDone = ev?.state === "done";
        return (
          <li key={s.order} className="relative min-w-21 flex-1 pt-7 text-center">
            {i < stages.length - 1 && (
              <span aria-hidden className={`absolute left-1/2 top-[9px] h-0.5 w-full ${connectorDone ? "bg-positive" : "bg-line-strong"}`} />
            )}
            <span
              aria-hidden
              className={`absolute left-1/2 top-0 z-10 flex h-5 w-5 -translate-x-1/2 items-center justify-center rounded-full border-2 bg-card text-[10px] font-bold ${
                state === "done"
                  ? "border-positive bg-positive text-white"
                  : state === "current"
                    ? "border-primary text-primary ring-4 ring-primary-soft"
                    : "border-line-strong text-ink-faint"
              }`}
            >
              {state === "done" ? "✓" : <span className="block h-1.5 w-1.5 rounded-full bg-current" />}
            </span>
            <div className={`px-1 text-[10px] font-semibold leading-tight ${state === "current" ? "text-primary" : state === "done" ? "text-ink" : "text-ink-muted"}`}>
              {s.name}
            </div>
            <div className="mt-0.5 text-[9px] leading-tight text-ink-faint">
              {ev ? [ev.note ?? ev.actor, ev.date].filter(Boolean).join(" · ") : s.approverRole || "—"}
            </div>
          </li>
        );
      })}
    </ol>
  );
}
