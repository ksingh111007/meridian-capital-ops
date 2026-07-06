"use client";

import { useState } from "react";
import type { PortalDocument } from "@/lib/types";
import { fmtDate } from "@/lib/format";
import { Card } from "@/components/ui/primitives";
import { DataTable, type Column } from "@/components/ui/DataTable";
import { FilterChips } from "@/components/ui/controls";
import { ZeroResults } from "@/components/ui/states";
import { DownloadButton, PortalPageHeader } from "./shared";

const FILTERS = ["All", "Capital account", "Notices", "Tax / K-1", "Fund reports", "Fund III", "Fund II"];

/** Chip label → document type value (fund chips filter on fund instead). */
const TYPE_BY_CHIP: Record<string, PortalDocument["type"]> = {
  "Capital account": "Capital account",
  "Notices": "Notice",
  "Tax / K-1": "Tax",
  "Fund reports": "Report",
};

const columns: Column<PortalDocument>[] = [
  { key: "name", header: "Document", cellClass: "font-semibold text-ink" },
  { key: "fund", header: "Fund", render: (d) => <span className="text-ink-muted">{d.fund}</span> },
  { key: "period", header: "Period", render: (d) => <span className="text-ink-muted">{d.period}</span> },
  { key: "type", header: "Type" },
  { key: "date", header: "Date", render: (d) => <span className="text-ink-muted">{fmtDate(d.date)}</span> },
  { key: "download", header: "", align: "right", render: (d) => <DownloadButton name={d.name} label="↓ PDF" /> },
];

/** Screen 6f — one library, filtered by type / fund, each document downloadable. */
export function PortalStatementsScreen({ totalCount, documents }: { totalCount: number; documents: PortalDocument[] }) {
  const [filter, setFilter] = useState(FILTERS[0]);

  const filtered = documents.filter((d) => {
    if (filter === "All") return true;
    if (filter === "Fund III" || filter === "Fund II") return d.fund === filter;
    return d.type === TYPE_BY_CHIP[filter];
  });

  return (
    <div>
      <PortalPageHeader
        title="Statements & documents"
        subtitle={`Everything issued to Redwood Pension · ${totalCount} documents`}
      />

      <div className="mt-2">
        <FilterChips options={FILTERS} active={filter} onChange={setFilter} trailing={`${filtered.length} shown`} />
      </div>

      <Card className="mt-3 overflow-hidden">
        <DataTable
          columns={columns}
          rows={filtered}
          rowKey={(d) => d.id}
          emptyState={<ZeroResults onClear={() => setFilter(FILTERS[0])} />}
        />
      </Card>
    </div>
  );
}
