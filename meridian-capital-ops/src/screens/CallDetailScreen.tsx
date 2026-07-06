"use client";

import { useMemo, useState } from "react";
import type { CapitalCall, CurrentUser, StageEvent, WorkflowStage } from "@/lib/types";
import { daysUntil, fmtDate, money, pct } from "@/lib/format";
import { Breadcrumb, DocIcon } from "@/components/ui/primitives";
import { Button } from "@/components/ui/Button";
import { Pill } from "@/components/ui/Pill";
import { Pipeline } from "@/components/ui/Pipeline";
import { Modal } from "@/components/ui/Modal";
import { Field, TextArea } from "@/components/ui/controls";
import { useToast } from "@/components/ui/Toast";

/**
 * The 2b due-diligence workflow screen. Approve/Reject both require a comment
 * (the modal is the confirmation step); state advances client-side against the
 * mock API. The audit rail collapses to give the grid full width.
 */
export function CallDetailScreen({
  call: initial, stages, user, userCanApprove,
}: {
  call: CapitalCall;
  stages: WorkflowStage[];
  user: CurrentUser;
  userCanApprove: boolean;
}) {
  const toast = useToast();
  const [call, setCall] = useState(initial);
  const [allocations, setAllocations] = useState(initial.allocations);
  const [auditOpen, setAuditOpen] = useState(true);
  const [modal, setModal] = useState<"approve" | "reject" | null>(null);
  const [comment, setComment] = useState("");

  const stage = stages.find((s) => s.order === call.currentStage);
  const nextStage = stages.find((s) => s.order === call.currentStage + 1);
  const prevStage = stages.find((s) => s.order === call.currentStage - 1);
  const isTerminal = !!stage?.terminal;
  /** Only the approver role for the current stage sees live actions. */
  const isMyStage = userCanApprove && stage?.approverRole === user.role;

  const allocTotal = useMemo(() => allocations.reduce((s, a) => s + a.amount, 0), [allocations]);
  const balanced = Math.abs(allocTotal - call.amount) < 0.005;
  const overdueDays = -daysUntil(call.dueDate);

  async function confirmAction() {
    if (!comment.trim() || !modal) return;
    const kind = modal;
    setModal(null);
    await fetch(`/api/capital-calls/${call.id}/${kind}`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ comment }),
    }).catch(() => null);

    const now = "Jul 05 09:45";
    if (kind === "approve") {
      const advanced = call.currentStage + 1;
      setCall((c) => ({
        ...c,
        currentStage: advanced,
        status: stages.find((s) => s.order === advanced)?.terminal ? "Completed" : "In Review",
        stageEvents: [
          ...c.stageEvents.filter((e) => e.stage !== c.currentStage),
          { stage: c.currentStage, state: "done", actor: user.name, date: "Jul 05" } as StageEvent,
          { stage: advanced, state: "current", note: "Awaiting review", date: "Jul 05" } as StageEvent,
        ],
        audit: [{ title: `${stage?.name} approved`, by: user.name, at: now, comment, tone: "green" as const }, ...c.audit],
      }));
      toast.push({ kind: "success", title: `Approved — advanced to ${nextStage?.name}`, detail: `${call.ref} · comment logged to audit trail` });
    } else {
      const returned = Math.max(1, call.currentStage - 1);
      setCall((c) => ({
        ...c,
        currentStage: returned,
        status: "Returned",
        stageEvents: [
          ...c.stageEvents.filter((e) => e.stage !== c.currentStage && e.stage !== returned),
          { stage: returned, state: "current", note: "Returned for re-review", date: "Jul 05" } as StageEvent,
        ],
        audit: [{ title: `Returned to ${prevStage?.name}`, by: user.name, at: now, comment, tone: "amber" as const }, ...c.audit],
      }));
      toast.push({ kind: "success", title: `Returned to ${prevStage?.name}`, detail: `${call.ref} · comment logged to audit trail` });
    }
    setComment("");
  }

  function editAllocation(investorId: string, value: string) {
    const amount = parseFloat(value);
    setAllocations((prev) => prev.map((a) => (a.investorId === investorId ? { ...a, amount: Number.isNaN(amount) ? 0 : amount } : a)));
  }

  return (
    <div className="flex min-h-full flex-col">
      <div className="flex flex-wrap items-center justify-between gap-3 border-b border-line px-5 py-3">
        <Breadcrumb backHref="/capital-calls" backLabel="Blotter" current={call.deal} sub={`Call ${call.ref}`} />
        <Pill className="px-2.5 py-0.5 text-[11px]">{isTerminal ? "Completed" : call.status === "Returned" ? "Returned" : `In ${stage?.name} Review`}</Pill>
      </div>

      {/* major details */}
      <div className="flex flex-wrap border-b border-line bg-card">
        {[
          { k: "Amount to Call", v: money(call.amount), big: true },
          { k: "Due Date", v: fmtDate(call.dueDate), cls: overdueDays > 0 ? "text-danger" : undefined, badge: overdueDays > 0 ? `${overdueDays}d overdue` : undefined },
          { k: "Borrower", v: call.borrower },
          { k: "Tranche", v: call.tranche },
          { k: "Fund", v: call.fundId === "fund-iii" ? "Fund III" : call.fundId === "fund-ii" ? "Fund II" : "Fund I" },
          { k: "Investors", v: `${allocations.length} LPs` },
        ].map((t) => (
          <div key={t.k} className="min-w-30 flex-1 border-r border-dashed border-line px-4 py-3 last:border-0">
            <div className="text-[9.5px] font-semibold uppercase tracking-wider text-ink-faint">{t.k}</div>
            <div className={`num mt-1 font-bold ${t.big ? "text-xl" : "text-sm"} ${t.cls ?? "text-ink"}`}>
              {t.v}
              {t.badge && <span className="ml-1.5 rounded-full bg-danger-soft px-1.5 py-px text-[10px] font-bold text-danger">{t.badge}</span>}
            </div>
          </div>
        ))}
      </div>

      <Pipeline stages={stages} events={call.stageEvents} />

      {/* action bar */}
      <div className="flex flex-wrap items-center justify-between gap-3 border-b border-line bg-card px-5 py-2.5">
        <div className="text-xs text-ink-secondary">
          <b className="text-ink">Stage {call.currentStage} of {stages.length} · {stage?.name}</b>
          {isTerminal ? " — completed" : isMyStage ? " — awaiting your approval" : ` — awaiting ${stage?.approverRole}`}
        </div>
        {!isTerminal && (
          <div className="flex items-center gap-2">
            <Button variant="dangerOutline" disabled={!isMyStage} onClick={() => setModal("reject")}
              title={isMyStage ? undefined : `Only the ${stage?.approverRole} can act on this stage`}>
              ← Return / Reject
            </Button>
            <Button variant="primary" disabled={!isMyStage} onClick={() => setModal("approve")}
              title={isMyStage ? undefined : `Only the ${stage?.approverRole} can act on this stage`}>
              Approve → {nextStage?.name} ▸
            </Button>
          </div>
        )}
      </div>

      {/* body: docs + allocations | collapsible audit rail */}
      <div className="flex min-h-0 flex-1">
        <div className="min-w-0 flex-1">
          <h2 className="px-5 pb-1.5 pt-4 text-[10.5px] font-semibold uppercase tracking-wider text-ink-faint">Documents</h2>
          <div className="flex flex-col gap-2.5 px-5 pb-4 sm:flex-row">
            <button type="button" className="flex h-auto w-full flex-none flex-col items-center justify-center gap-1 rounded-lg border-2 border-dashed border-line-strong px-4 py-4 text-[11px] leading-snug text-ink-muted hover:border-primary hover:text-primary sm:w-32">
              ⬆<span>Drop files<br />or browse</span>
            </button>
            <ul className="flex min-w-0 flex-1 flex-col gap-1.5">
              {call.documents.map((doc) => (
                <li key={doc.name} className="flex items-center gap-2.5 rounded-md border border-line px-3 py-2">
                  <DocIcon />
                  <span className="min-w-0 flex-1">
                    <span className="block truncate text-xs font-semibold text-ink">{doc.name}</span>
                    <span className="block text-[10px] text-ink-faint">{doc.by} · {doc.date}</span>
                  </span>
                  <button type="button" aria-label={`Remove ${doc.name}`} className="text-ink-faint hover:text-danger">×</button>
                </li>
              ))}
            </ul>
          </div>

          <h2 className="px-5 pb-1.5 pt-1 text-[10.5px] font-semibold uppercase tracking-wider text-ink-faint">
            Investor Allocations · {allocations.length} LPs
          </h2>
          <div className="overflow-x-auto px-5 pb-5">
            <table className="w-full border-collapse overflow-hidden rounded-lg border border-line text-left">
              <thead>
                <tr className="bg-fill">
                  {["Investor", "Commitment", "Allocation", "% of Call", "Wire"].map((h, i) => (
                    <th key={h} className={`whitespace-nowrap border-b border-line px-3 py-2 text-[10px] font-semibold uppercase tracking-wider text-ink-faint ${i === 1 || i === 2 || i === 3 ? "text-right" : ""}`}>{h}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {allocations.map((a) => (
                  <tr key={a.investorId} className="border-b border-line/60">
                    <td className="px-3 py-2 text-xs font-semibold text-ink">{a.investor}</td>
                    <td className="num px-3 py-2 text-right text-xs text-ink-muted">{money(a.commitment)}</td>
                    <td className="px-3 py-2 text-right">
                      <span className="inline-flex items-center gap-1 text-xs font-semibold">
                        $<input
                          type="number" step="0.1" min="0"
                          aria-label={`${a.investor} allocation in millions`}
                          value={a.amount}
                          onChange={(e) => editAllocation(a.investorId, e.target.value)}
                          className="num w-20 rounded-md border border-line-strong bg-card px-2 py-1 text-right text-xs font-semibold focus:border-primary focus:outline-none"
                        />M
                      </span>
                    </td>
                    <td className="num px-3 py-2 text-right text-xs">{allocTotal > 0 ? pct((a.amount / allocTotal) * 100) : "—"}</td>
                    <td className="px-3 py-2"><Pill>{a.wireStatus}</Pill></td>
                  </tr>
                ))}
                <tr className="bg-fill-strong">
                  <td className="px-3 py-2 text-xs font-bold text-ink">Total allocated</td>
                  <td className="num px-3 py-2 text-right text-xs text-ink-muted">{money(allocations.reduce((s, a) => s + a.commitment, 0))}</td>
                  <td className="num px-3 py-2 text-right text-xs font-bold text-ink">{money(allocTotal)}</td>
                  <td className="num px-3 py-2 text-right text-xs font-bold">100%</td>
                  <td className="px-3 py-2 text-[11px] font-semibold">
                    {balanced ? <span className="text-positive">✓ balanced</span> : <span className="text-danger">≠ {money(call.amount)} call</span>}
                  </td>
                </tr>
              </tbody>
            </table>
            {!balanced && (
              <p className="mt-2 text-[11px] font-medium text-danger">
                Allocations must reconcile to the {money(call.amount)} call before this call can advance.
              </p>
            )}
          </div>
        </div>

        {/* audit rail — collapses to a thin strip so the grid gets full width */}
        {auditOpen ? (
          <aside className="w-64 flex-none border-l border-line bg-fill px-4 py-4">
            <div className="flex items-center justify-between pb-2">
              <h2 className="text-[10.5px] font-semibold uppercase tracking-wider text-ink-faint">Audit Trail</h2>
              <button type="button" title="Collapse panel" aria-label="Collapse audit trail" onClick={() => setAuditOpen(false)}
                className="flex h-5 w-5 items-center justify-center rounded-md border border-line-strong text-xs text-ink-muted hover:text-ink">»</button>
            </div>
            <ol>
              {call.audit.map((e, i) => (
                <li key={`${e.at}-${i}`} className="flex gap-2.5 border-b border-dashed border-line py-2 last:border-0">
                  <span aria-hidden className={`mt-1 h-2 w-2 flex-none rounded-full ${
                    e.tone === "green" ? "bg-positive" : e.tone === "blue" ? "bg-primary ring-3 ring-primary-soft" : e.tone === "amber" ? "bg-caution-strong" : "bg-line-strong"}`} />
                  <span className="min-w-0">
                    <span className="block text-[11.5px] font-semibold text-ink">{e.title}</span>
                    <span className="block text-[10px] text-ink-faint">{e.by} · {e.at}</span>
                    {e.comment && <span className="mt-0.5 block text-[10.5px] italic text-ink-secondary">“{e.comment}”</span>}
                  </span>
                </li>
              ))}
            </ol>
          </aside>
        ) : (
          <button
            type="button"
            title="Expand audit trail"
            onClick={() => setAuditOpen(true)}
            className="flex w-9 flex-none cursor-pointer flex-col items-center gap-3 border-l border-line bg-fill py-3 hover:bg-fill-strong"
          >
            <span className="flex h-5 w-5 items-center justify-center rounded-md border border-line-strong text-xs text-ink-muted">«</span>
            <span className="text-[9px] font-bold uppercase tracking-[0.14em] text-ink-faint" style={{ writingMode: "vertical-rl", transform: "rotate(180deg)" }}>
              Audit Trail
            </span>
          </button>
        )}
      </div>

      {/* required-comment modal (the confirmation step for both actions) */}
      <Modal
        open={modal !== null}
        onClose={() => setModal(null)}
        title={modal === "reject" ? `Return → ${prevStage?.name} review` : `Approve → ${nextStage?.name}`}
        footer={
          <>
            <Button onClick={() => setModal(null)}>Cancel</Button>
            <Button variant={modal === "reject" ? "danger" : "primary"} disabled={!comment.trim()} onClick={confirmAction}>
              {modal === "reject" ? "Confirm return" : "Confirm & advance"}
            </Button>
          </>
        }
      >
        <div className="flex flex-col gap-3">
          <div className="text-[11px] text-ink-muted">{call.deal} · Call {call.ref} · {money(call.amount)}</div>
          {modal === "reject" && (
            <div className="rounded-md border border-caution-line bg-caution-soft px-3 py-2 text-[11px] text-caution">
              This call goes back to <b>{prevStage?.name}</b> for re-review.
            </div>
          )}
          <Field label="Comment" required>
            <TextArea value={comment} onChange={(e) => setComment(e.target.value)} placeholder="Required — logged to the audit trail" />
          </Field>
          <button type="button" className="flex items-center gap-2 self-start text-[11px] text-ink-muted hover:text-primary">
            <span className="h-4 w-4 rounded border border-dashed border-line-strong" aria-hidden /> ⬆ Attach supporting document (optional)
          </button>
        </div>
      </Modal>
    </div>
  );
}
