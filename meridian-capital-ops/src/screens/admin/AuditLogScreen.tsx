"use client";

import { useState } from "react";
import type { AuditEvent } from "@/lib/types";
import { ScreenHeader } from "@/components/shell/ScreenHeader";
import { Button } from "@/components/ui/Button";
import { DataTable, type Column } from "@/components/ui/DataTable";
import { Kpi, KpiRow } from "@/components/ui/Kpi";
import { Pill } from "@/components/ui/Pill";
import { FilterChips, SearchInput } from "@/components/ui/controls";
import { ZeroResults } from "@/components/ui/states";

interface Kpis { eventsToday: number; approvals: number; configChanges: number; exceptions: number }

const FILTERS = ["All", "Approvals", "Edits", "Wires", "Admin", "Config"];

function matchesFilter(e: AuditEvent, filter: string): boolean {
  switch (filter) {
    case "Approvals": return e.action.includes("Approved") || e.action.includes("review");
    case "Edits": return e.action.includes("Edited") || e.action.includes("Marked");
    case "Wires": return e.object.startsWith("W-");
    case "Admin":
    case "Config": return e.action.includes("Role") || e.action.includes("Config");
    default: return true;
  }
}

const isConfig = (e: AuditEvent) => e.action.includes("Role") || e.action.includes("Config");

const columns: Column<AuditEvent>[] = [
  { key: "time", header: "Time", cellClass: "font-mono text-[11px] text-ink-muted", render: (e) => e.time, sortValue: (e) => e.time },
  { key: "actor", header: "Actor", cellClass: "font-semibold text-ink", sortValue: (e) => e.actor },
  { key: "action", header: "Action", render: (e) => <Pill tone={e.tone}>{e.action}</Pill>, sortValue: (e) => e.action },
  { key: "object", header: "Object", render: (e) => e.object },
  { key: "detail", header: "Detail", render: (e) => <span className="text-ink-muted">{e.detail}</span> },
  { key: "seal", header: "Seal", cellClass: "font-mono text-[11px] text-ink-muted", render: (e) => e.seal },
];

export function AuditLogScreen({ kpis, events }: { kpis: Kpis; events: AuditEvent[] }) {
  const [filter, setFilter] = useState(FILTERS[0]);
  const [query, setQuery] = useState("");

  const q = query.trim().toLowerCase();
  const filtered = events.filter(
    (e) =>
      matchesFilter(e, filter) &&
      (!q || e.actor.toLowerCase().includes(q) || e.object.toLowerCase().includes(q) || e.detail.toLowerCase().includes(q)),
  );

  return (
    <div>
      <ScreenHeader title={<><span className="font-medium text-ink-faint">Admin /</span> Audit Log</>}>
        <SearchInput value={query} onChange={setQuery} placeholder="Search events…" />
        <Button variant="primary">Export</Button>
      </ScreenHeader>

      <KpiRow>
        <Kpi label="Events Today" value={kpis.eventsToday} sub="all sources" />
        <Kpi label="Approvals" value={kpis.approvals} sub="approve / reject" />
        <Kpi label="Config Changes" value={kpis.configChanges} sub="admin edits" tone="primary" />
        <Kpi label="Exceptions" value={kpis.exceptions} sub="wire reject" tone="danger" />
      </KpiRow>

      <FilterChips options={FILTERS} active={filter} onChange={setFilter} trailing="append-only · hash-chained" />

      <DataTable
        columns={columns}
        rows={filtered}
        rowKey={(e) => e.seal}
        rowClass={(e) => (e.tone === "red" ? "bg-danger-soft/40" : isConfig(e) ? "bg-caution-soft/30" : "")}
        emptyState={<ZeroResults onClear={() => { setFilter(FILTERS[0]); setQuery(""); }} />}
      />
    </div>
  );
}
