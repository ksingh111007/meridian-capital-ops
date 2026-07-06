"use client";

import { useState } from "react";
import type { PortalTaxDocument } from "@/lib/types";
import { DataTable, type Column } from "@/components/ui/DataTable";
import { FilterChips } from "@/components/ui/controls";
import { Card } from "@/components/ui/primitives";
import { Pill } from "@/components/ui/Pill";
import { ZeroResults } from "@/components/ui/states";
import { DownloadButton, PortalPageHeader } from "./shared";

const FILTERS = ["All years", "2025", "2024", "Fund III", "Fund II"];

const columns: Column<PortalTaxDocument>[] = [
  { key: "name", header: "Document", cellClass: "font-semibold text-ink" },
  { key: "fund", header: "Fund", render: (d) => <span className="text-ink-muted">{d.fund}</span> },
  { key: "taxYear", header: "Tax year", render: (d) => <span className="text-ink-muted">{d.taxYear}</span> },
  { key: "type", header: "Type" },
  {
    key: "status", header: "Status",
    render: (d) => (d.status === "Available" ? <Pill>Available</Pill> : <Pill tone="neutral">Pending · {d.expectedDate}</Pill>),
  },
  {
    key: "download", header: "", align: "right",
    render: (d) => (d.status === "Available" ? <DownloadButton name={d.name} label="↓ PDF" /> : <span className="text-[10.5px] text-ink-faint">—</span>),
  },
];

/** Screen 6h — K-1s and tax packages by year; pending years show the ETA, download disabled. */
export function PortalTaxScreen({
  banner, documents,
}: {
  banner: { headline: string; detail: string };
  documents: PortalTaxDocument[];
}) {
  const [filter, setFilter] = useState(FILTERS[0]);

  const filtered = documents.filter((d) => {
    if (filter === "All years") return true;
    if (filter === "Fund III" || filter === "Fund II") return d.fund === filter;
    return String(d.taxYear) === filter;
  });

  return (
    <div>
      <PortalPageHeader title="Tax documents" subtitle="Schedule K-1s and tax packages by year and fund" />

      <div className="mt-4 flex items-center gap-2 rounded-lg border border-primary-line bg-primary-soft px-3.5 py-2.5 text-[11px]">
        <b className="text-primary">{banner.headline}</b>
        <span className="text-ink-secondary">{banner.detail}</span>
      </div>

      <div className="mt-1">
        <FilterChips options={FILTERS} active={filter} onChange={setFilter} trailing={`${filtered.length} documents`} />
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
