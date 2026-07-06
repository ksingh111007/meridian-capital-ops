"use client";

import { useState } from "react";
import type { PortalActivityRow } from "@/lib/types";
import { fmtDate, money } from "@/lib/format";
import { Card } from "@/components/ui/primitives";
import { DataTable, type Column } from "@/components/ui/DataTable";
import { FilterChips } from "@/components/ui/controls";
import { Pill } from "@/components/ui/Pill";
import { ZeroResults } from "@/components/ui/states";
import { PortalPageHeader, SignedAmount, StatStrip } from "./shared";

export interface PortalActivityStats {
  paidIn: number;
  distributionsReceived: number;
  netInvested: number;
  nextCallDue: string;
}

const FILTERS = ["All", "Calls", "Distributions", "Fund III", "Fund II"];

const columns: Column<PortalActivityRow>[] = [
  { key: "date", header: "Date", render: (r) => fmtDate(r.date), sortValue: (r) => r.date },
  { key: "fund", header: "Fund", render: (r) => <span className="text-ink-muted">{r.fund}</span> },
  { key: "type", header: "Type" },
  { key: "reference", header: "Reference", render: (r) => <span className="text-ink-muted">{r.reference}</span> },
  { key: "amount", header: "Amount", align: "right", render: (r) => <SignedAmount value={r.amount} />, sortValue: (r) => r.amount },
  { key: "status", header: "Status", render: (r) => <Pill>{r.status}</Pill> },
];

/** Screen 6g — the full call/distribution ledger behind the Overview snapshot. */
export function PortalActivityScreen({ stats, rows }: { stats: PortalActivityStats; rows: PortalActivityRow[] }) {
  const [filter, setFilter] = useState(FILTERS[0]);

  const filtered = rows.filter((r) =>
    filter === "All" ? true
    : filter === "Calls" ? r.type === "Capital Call"
    : filter === "Distributions" ? r.type === "Distribution"
    : r.fund === filter,
  );

  return (
    <div>
      <PortalPageHeader title="Capital activity" subtitle="Every call & distribution across your funds · lifetime" />

      <div className="mt-4">
        <StatStrip
          stats={[
            { label: "Paid in", value: money(stats.paidIn, { decimals: 1 }) },
            { label: "Distributions received", value: money(stats.distributionsReceived, { decimals: 1 }), valueClass: "text-positive" },
            { label: "Net invested", value: money(stats.netInvested, { decimals: 1 }) },
            { label: "Next call due", value: fmtDate(stats.nextCallDue, "short"), valueClass: "text-caution-strong" },
          ]}
        />
      </div>

      <div className="mt-2">
        <FilterChips options={FILTERS} active={filter} onChange={setFilter} trailing={`${filtered.length} records`} />
      </div>

      <Card className="mt-3 overflow-hidden">
        <DataTable
          columns={columns}
          rows={filtered}
          rowKey={(r) => r.reference}
          emptyState={<ZeroResults onClear={() => setFilter(FILTERS[0])} />}
        />
      </Card>
    </div>
  );
}
