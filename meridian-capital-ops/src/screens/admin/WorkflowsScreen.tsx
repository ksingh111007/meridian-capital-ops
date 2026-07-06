"use client";

import { useState } from "react";
import type { EscalationRule, WorkflowStage } from "@/lib/types";
import { ScreenHeader } from "@/components/shell/ScreenHeader";
import { Button } from "@/components/ui/Button";
import { useToast } from "@/components/ui/Toast";
import { Select, Toggle } from "@/components/ui/controls";
import { SectionTitle } from "@/components/ui/primitives";

const ALL_ROLES = ["Ops Analyst", "Deal Lead", "CIO", "Counsel", "Ops Manager", "Fund Accountant", "Compliance", "System"];

function slaText(st: WorkflowStage): string {
  if (st.terminal) return "—";
  if (st.slaDays !== null) return `${st.slaDays} day${st.slaDays > 1 ? "s" : ""}`;
  return st.autoAdvance ? "auto" : "—";
}

export function WorkflowsScreen({
  workflowName, stages: initialStages, escalationRules,
}: {
  workflowName: string;
  stages: WorkflowStage[];
  escalationRules: EscalationRule[];
}) {
  const toast = useToast();
  const [stages, setStages] = useState(initialStages);
  const [rules, setRules] = useState(escalationRules);

  function patchStage(order: number, patch: Partial<WorkflowStage>) {
    setStages((prev) => prev.map((s) => (s.order === order ? { ...s, ...patch } : s)));
  }

  return (
    <div>
      <ScreenHeader title={<><span className="font-medium text-ink-faint">Admin /</span> Approval Workflows</>} context={workflowName}>
        <span className="w-48">
          <Select aria-label="Workflow" options={[`Workflow: ${workflowName}`]} value={`Workflow: ${workflowName}`} onChange={() => {}} />
        </span>
        <Button variant="primary" onClick={() => toast.push({ kind: "success", title: `Workflow saved · ${stages.length} stages` })}>
          Save workflow
        </Button>
      </ScreenHeader>

      <SectionTitle>Pipeline stages · {stages.length}</SectionTitle>
      <div className="px-5 pb-2">
        {/* column headers */}
        <div className="flex items-center gap-3 py-1.5 text-[9.5px] font-semibold uppercase tracking-wider text-ink-faint">
          <span className="w-5" />
          <span className="flex-1">Stage</span>
          <span className="w-44">Approver role</span>
          <span className="w-16">SLA</span>
          <span className="w-20 text-center">Auto-adv.</span>
          <span className="w-16 text-center">Required</span>
        </div>
        {stages.map((st) => (
          <div key={st.order} className="flex items-center gap-3 border-t border-dashed border-line py-2">
            <span
              aria-hidden
              className={`flex h-5 w-5 flex-none items-center justify-center rounded-full text-[10px] font-bold text-white ${
                st.terminal ? "bg-positive" : "bg-ink"
              }`}
            >
              {st.order}
            </span>
            <span className="flex-1 text-xs font-semibold text-ink">
              {st.name}
              {st.terminal && <span className="ml-1 text-[10px] font-semibold text-ink-faint">· terminal</span>}
            </span>
            <span className="w-44">
              {st.terminal ? (
                <span className="text-xs text-ink-faint">—</span>
              ) : (
                <Select
                  aria-label={`${st.name} approver role`}
                  options={Array.from(new Set([st.approverRole, ...ALL_ROLES]))}
                  value={st.approverRole}
                  onChange={(e) => patchStage(st.order, { approverRole: e.target.value })}
                  className={st.approverRole === "System" ? "text-ink-faint" : ""}
                />
              )}
            </span>
            <span className={`w-16 text-[11px] ${st.terminal ? "text-ink-faint" : "text-ink-muted"}`}>{slaText(st)}</span>
            <span className="flex w-20 justify-center">
              {st.terminal ? <span className="text-xs text-ink-faint">—</span> : (
                <Toggle on={st.autoAdvance} onChange={(on) => patchStage(st.order, { autoAdvance: on })} label={`${st.name} auto-advance`} />
              )}
            </span>
            <span className="flex w-16 justify-center">
              {st.terminal ? <span className="text-xs text-ink-faint">—</span> : (
                <Toggle on={st.required} onChange={(on) => patchStage(st.order, { required: on })} label={`${st.name} required`} />
              )}
            </span>
          </div>
        ))}
        <div className="pb-2 pt-2.5">
          <Button size="sm" onClick={() => toast.push({ kind: "success", title: "Stage editor not in mock" })}>+ Add stage</Button>
        </div>
      </div>

      <SectionTitle>Escalation rules</SectionTitle>
      <div className="flex flex-col gap-2 px-5 pb-5 pt-1">
        {rules.map((r) => (
          <div key={r.condition} className="flex items-center gap-2.5 rounded-lg border border-dashed border-line-strong px-3 py-2.5">
            <Toggle
              on={r.enabled}
              onChange={(on) => setRules((prev) => prev.map((x) => (x.condition === r.condition ? { ...x, enabled: on } : x)))}
              label={r.condition}
            />
            <span className="text-xs text-ink-secondary">
              <b className="text-ink">{r.condition}</b> → {r.effect}
            </span>
          </div>
        ))}
      </div>
    </div>
  );
}
