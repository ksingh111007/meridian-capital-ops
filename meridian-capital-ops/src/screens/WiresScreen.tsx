"use client";

import { useRouter } from "next/navigation";
import { useState } from "react";
import type { Wire } from "@/lib/types";
import { money } from "@/lib/format";
import { postJson } from "@/lib/mutate";
import { ScreenHeader } from "@/components/shell/ScreenHeader";
import { Button } from "@/components/ui/Button";
import { Kpi, KpiRow } from "@/components/ui/Kpi";
import { DataTable, type Column } from "@/components/ui/DataTable";
import { FilterChips } from "@/components/ui/controls";
import { Pill } from "@/components/ui/Pill";
import { ConfirmDialog } from "@/components/ui/Modal";
import { useToast } from "@/components/ui/Toast";
import { ZeroResults } from "@/components/ui/states";

const FILTERS = ["All", "Outbound", "Inbound", "Exceptions"];
const IN_FLIGHT = new Set<Wire["status"]>(["Sent", "Acknowledged", "Queued"]);

/** Screen 4c — every inbound / outbound movement and where it is in settlement. */
export function WiresScreen({ asOf, wires: initial }: { asOf: string; wires: Wire[] }) {
  const router = useRouter();
  const toast = useToast();
  const [wires, setWires] = useState(initial);
  const [filter, setFilter] = useState(FILTERS[0]);
  const [resolving, setResolving] = useState<Wire | null>(null);

  const filtered = wires.filter((w) => {
    if (filter === "Outbound") return w.direction === "Out";
    if (filter === "Inbound") return w.direction === "In";
    if (filter === "Exceptions") return w.status === "Exception";
    return true;
  });

  // KPIs computed from the live rows so Resolve keeps the strip honest
  const settled = wires.filter((w) => w.status === "Settled");
  const inFlight = wires.filter((w) => IN_FLIGHT.has(w.status));
  const exceptions = wires.filter((w) => w.status === "Exception");

  async function retry(w: Wire) {
    const { ok, error } = await postJson(`wires/${w.id}/retry`);
    if (!ok) {
      toast.push({ kind: "error", title: `Retry failed · ${w.ref}`, detail: error });
      return;
    }
    // The backend re-queues the wire and clears the exception — mirror that.
    setWires((prev) => prev.map((x) => (x.id === w.id ? { ...x, status: "Queued" as const, exceptionReason: undefined } : x)));
    toast.push({ kind: "success", title: `Wire retry queued · ${w.ref}` });
  }

  function confirmResolve() {
    if (!resolving) return;
    const ref = resolving.ref;
    setWires((prev) => prev.map((w) => (w.id === resolving.id ? { ...w, status: "Settled" as const, exceptionReason: undefined } : w)));
    setResolving(null);
    toast.push({ kind: "success", title: `${ref} marked resolved`, detail: "Exception cleared · settlement confirmed manually" });
  }

  const columns: Column<Wire>[] = [
    { key: "ref", header: "Wire Ref", cellClass: "font-mono font-semibold text-ink" },
    {
      key: "direction", header: "Dir",
      render: (w) =>
        w.direction === "In"
          ? <span className="font-bold text-positive">← In</span>
          : <span className="font-bold text-primary">→ Out</span>,
    },
    { key: "counterparty", header: "Counterparty" },
    { key: "type", header: "Type", render: (w) => <span className="text-ink-muted">{w.type} <span className="text-ink-faint">{w.linkedRef}</span></span> },
    { key: "amount", header: "Amount", align: "right", cellClass: "font-semibold text-ink", render: (w) => money(w.amount), sortValue: (w) => w.amount },
    { key: "time", header: "Time", cellClass: "text-ink-muted" },
    { key: "rail", header: "Rail", cellClass: "text-ink-muted" },
    { key: "status", header: "Status", render: (w) => <Pill>{w.status}</Pill> },
    {
      key: "actions", header: "", align: "right",
      render: (w) =>
        w.status === "Exception" ? (
          <span className="inline-flex gap-1.5">
            <Button size="sm" onClick={() => retry(w)}>Retry</Button>
            <Button size="sm" variant="ghost" onClick={() => setResolving(w)}>Resolve</Button>
          </span>
        ) : null,
    },
  ];

  return (
    <div>
      <ScreenHeader title="Wire Status" context="Jul 05 · live">
        <Button>Export</Button>
        <Button variant="primary" onClick={() => router.push("/reconciliation")}>Reconcile</Button>
      </ScreenHeader>

      <KpiRow>
        <Kpi label="Wires Today" value={String(wires.length)} sub={`${wires.length} shown`} />
        <Kpi label="Settled" value={money(settled.reduce((s, w) => s + w.amount, 0))} sub={`${settled.length} wires`} />
        <Kpi label="In Flight" value={money(inFlight.reduce((s, w) => s + w.amount, 0))} sub={`${inFlight.length} wires`} tone="primary" />
        <Kpi label="Exceptions" value={String(exceptions.length)} sub="needs attention" tone="danger" />
      </KpiRow>

      <FilterChips options={FILTERS} active={filter} onChange={setFilter} trailing={`as of ${asOf}`} />

      <DataTable
        columns={columns}
        rows={filtered}
        rowKey={(w) => w.id}
        rowClass={(w) => (w.status === "Exception" ? "bg-danger-soft/40" : "")}
        emptyState={<ZeroResults onClear={() => setFilter(FILTERS[0])} />}
      />

      {exceptions.some((w) => w.exceptionReason) && (
        <div className="mx-5 my-3 rounded-md border border-danger-line bg-danger-soft px-3 py-2 text-[11px] text-danger">
          {exceptions.filter((w) => w.exceptionReason).map((w) => (
            <span key={w.id} className="block"><b>{w.ref}</b> — {w.exceptionReason}</span>
          ))}
        </div>
      )}

      <ConfirmDialog
        open={resolving !== null}
        onCancel={() => setResolving(null)}
        onConfirm={confirmResolve}
        title={`Mark ${resolving?.ref} resolved?`}
        confirmLabel="Mark resolved"
        body={
          <>
            This clears the exception on <b>{resolving?.ref}</b> ({resolving?.counterparty} · {resolving ? money(resolving.amount) : ""}) after
            settlement has been confirmed manually with the counterparty bank. The wire will show as <b>Settled</b>.
          </>
        }
      />
    </div>
  );
}
