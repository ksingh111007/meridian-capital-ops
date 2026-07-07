"use client";

import { useState } from "react";
import type { ReconItem } from "@/lib/types";
import { fmtDate, money, pct } from "@/lib/format";
import { postJson } from "@/lib/mutate";
import { ScreenHeader } from "@/components/shell/ScreenHeader";
import { Button } from "@/components/ui/Button";
import { Kpi, KpiRow } from "@/components/ui/Kpi";
import { DataTable, type Column } from "@/components/ui/DataTable";
import { FilterChips, Field, Select, Tabs } from "@/components/ui/controls";
import { Pill } from "@/components/ui/Pill";
import { ConfirmDialog, Modal } from "@/components/ui/Modal";
import { useToast } from "@/components/ui/Toast";
import { EmptyState, ZeroResults } from "@/components/ui/states";

const TABS = ["Cash", "Positions", "Investor"];
const FILTERS = ["All items", "Breaks only", "Unmatched", "Under review"];
const ASSIGNEES = ["D. Whitfield", "J. Chen", "T. Alvarez"];

interface ReconKpis {
  matchedPct: number;
  matchedCount: number;
  totalItems: number;
  exceptions: number;
  unmatched: number;
  amountInBreak: number;
}

/** Screen 4e — book vs custodian, breaks surfaced for clearing. */
export function ReconciliationScreen({ source, kpis, items: initial }: { source: string; kpis: ReconKpis; items: ReconItem[] }) {
  const toast = useToast();
  const [items, setItems] = useState(initial);
  const [tab, setTab] = useState(TABS[0]);
  const [filter, setFilter] = useState(FILTERS[0]);
  const [assigning, setAssigning] = useState<ReconItem | null>(null);
  const [assignee, setAssignee] = useState(ASSIGNEES[0]);
  const [clearing, setClearing] = useState<ReconItem | null>(null);

  const filtered = items.filter((it) => {
    if (filter === "Breaks only") return it.status === "Break";
    if (filter === "Unmatched") return it.status === "Unmatched";
    if (filter === "Under review") return it.status !== "Matched" && !!it.assignee;
    return true;
  });

  async function confirmAssign() {
    if (!assigning) return;
    const target = assigning;
    setAssigning(null);
    const { ok, error } = await postJson(`reconciliation/${target.id}/assign`, { assignee });
    if (!ok) {
      toast.push({ kind: "error", title: "Assignment failed", detail: error });
      return;
    }
    setItems((prev) => prev.map((it) => (it.id === target.id ? { ...it, assignee } : it)));
    toast.push({ kind: "success", title: `Break assigned to ${assignee}`, detail: target.description });
  }

  function confirmClear() {
    if (!clearing) return;
    const target = clearing;
    setClearing(null);
    setItems((prev) => prev.map((it) => (it.id === target.id ? { ...it, status: "Matched" as const, diff: 0 } : it)));
    toast.push({ kind: "success", title: "Break cleared", detail: `${target.description} · marked Matched` });
  }

  const diffCell = (it: ReconItem) => {
    if (it.diff === 0) return <span className="text-ink-muted">$0.00</span>;
    return <span className={`font-bold ${it.status === "Break" ? "text-danger" : "text-caution-strong"}`}>{money(it.diff)}</span>;
  };

  const columns: Column<ReconItem>[] = [
    { key: "date", header: "Date", render: (it) => fmtDate(it.date, "short") },
    {
      key: "description", header: "Description",
      render: (it) => (
        <span className="block">
          <span className="font-semibold text-ink">{it.description}</span>
          {it.assignee && <span className="mt-0.5 block text-[10.5px] text-ink-muted">assigned to {it.assignee}</span>}
        </span>
      ),
    },
    { key: "source", header: "Source", cellClass: "text-ink-muted" },
    { key: "book", header: "Book", align: "right", render: (it) => (it.book !== null ? money(it.book) : <span className="text-ink-faint">—</span>) },
    { key: "custodian", header: "Custodian", align: "right", render: (it) => (it.custodian !== null ? money(it.custodian) : <span className="text-ink-faint">—</span>) },
    { key: "diff", header: "Diff", align: "right", render: diffCell },
    { key: "status", header: "Status", render: (it) => <Pill>{it.status}</Pill> },
    {
      key: "actions", header: "", align: "right",
      render: (it) =>
        it.status !== "Matched" ? (
          <span className="inline-flex gap-1.5">
            <Button size="sm" onClick={() => { setAssignee(it.assignee ?? ASSIGNEES[0]); setAssigning(it); }}>Assign</Button>
            <Button size="sm" variant="ghost" onClick={() => setClearing(it)}>Clear</Button>
          </span>
        ) : null,
    },
  ];

  return (
    <div>
      <ScreenHeader title="Reconciliation" context="Jul 04 close">
        <Button>Prior day</Button>
        <Button variant="primary" onClick={() => toast.push({ kind: "success", title: "Auto-match complete · 0 new matches" })}>
          Auto-match
        </Button>
      </ScreenHeader>

      <KpiRow>
        <Kpi label="Matched" value={pct(kpis.matchedPct)} sub={`${kpis.matchedCount} of ${kpis.totalItems} items`} tone="positive" />
        <Kpi label="Exceptions" value={String(kpis.exceptions)} sub="value breaks" tone="danger" />
        <Kpi label="Unmatched" value={String(kpis.unmatched)} sub="no counterpart" tone="primary" />
        <Kpi label="Amount in Break" value={money(kpis.amountInBreak)} sub="net difference" />
      </KpiRow>

      <Tabs tabs={TABS} active={tab} onChange={setTab} />

      {tab !== "Cash" ? (
        <EmptyState
          title={`No ${tab.toLowerCase()} recon in this mock`}
          message="The cash recon is the built-out example; positions & investor recon follow the same contract."
        />
      ) : (
        <>
          <FilterChips options={FILTERS} active={filter} onChange={setFilter} trailing={`source: ${source}`} />

          <DataTable
            columns={columns}
            rows={filtered}
            rowKey={(it) => it.id}
            rowClass={(it) => (it.status === "Break" ? "bg-danger-soft/40" : it.status === "Unmatched" ? "bg-caution-soft/40" : "")}
            emptyState={<ZeroResults onClear={() => setFilter(FILTERS[0])} />}
          />
        </>
      )}

      {/* assign-a-break modal */}
      <Modal
        open={assigning !== null}
        onClose={() => setAssigning(null)}
        title="Assign break"
        footer={
          <>
            <Button onClick={() => setAssigning(null)}>Cancel</Button>
            <Button variant="primary" onClick={confirmAssign}>Assign</Button>
          </>
        }
      >
        <div className="flex flex-col gap-3">
          <div className="text-[11px] text-ink-muted">
            {assigning?.description} · diff {assigning ? money(assigning.diff) : ""}
          </div>
          <Field label="Assignee" required>
            <Select options={ASSIGNEES} value={assignee} onChange={(e) => setAssignee(e.target.value)} />
          </Field>
        </div>
      </Modal>

      <ConfirmDialog
        open={clearing !== null}
        onCancel={() => setClearing(null)}
        onConfirm={confirmClear}
        title="Clear this break?"
        confirmLabel="Clear break"
        body={
          <>
            <b>{clearing?.description}</b> will be marked <b>Matched</b> with a zero difference. Only clear a break once the
            underlying item pair has been verified against the custodian record.
          </>
        }
      />
    </div>
  );
}
