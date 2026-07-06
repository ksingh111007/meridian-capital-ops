"use client";

import Link from "next/link";
import { useState } from "react";
import type { Drawdown } from "@/lib/types";
import { fmtDate, money, pct } from "@/lib/format";
import { ScreenHeader } from "@/components/shell/ScreenHeader";
import { Button } from "@/components/ui/Button";
import { Kpi, KpiRow } from "@/components/ui/Kpi";
import { DataTable, Td, type Column } from "@/components/ui/DataTable";
import { FilterChips } from "@/components/ui/controls";
import { Pill } from "@/components/ui/Pill";
import { ZeroResults } from "@/components/ui/states";

const FILTERS = ["All facilities", "Subscription line", "NAV facility", "Repay ≤ 30d"];
/** Mock today (2026-07-05) + 30 days. */
const REPAY_CUTOFF = "2026-08-04";

interface DrawdownKpis {
  facilityLimit: number;
  facilities: number;
  drawn: number;
  utilisationPct: number;
  available: number;
  weightedRate: string;
}

/** "Bridge — Project Atlas" → prefix + deal name (linked to its capital call when bridged). */
function PurposeCell({ d }: { d: Drawdown }) {
  if (!d.linkedCallId) return <span>{d.purpose}</span>;
  const sep = " — ";
  const i = d.purpose.indexOf(sep);
  const prefix = i >= 0 ? d.purpose.slice(0, i + sep.length) : "";
  const deal = i >= 0 ? d.purpose.slice(i + sep.length) : d.purpose;
  return (
    <span>
      {prefix}
      <Link href={`/capital-calls/${d.linkedCallId}`} className="font-medium text-primary hover:underline" onClick={(e) => e.stopPropagation()}>
        {deal}
      </Link>
    </span>
  );
}

/** Screen 4b — draws on the fund's credit facilities (bridge to capital calls). */
export function DrawdownsScreen({ kpis, drawdowns }: { kpis: DrawdownKpis; drawdowns: Drawdown[] }) {
  const [filter, setFilter] = useState(FILTERS[0]);

  const filtered = drawdowns.filter((d) => {
    if (filter === "Subscription line") return d.facility.includes("Sub Line");
    if (filter === "NAV facility") return d.facility.includes("NAV");
    if (filter === "Repay ≤ 30d") return !!d.repayBy && d.repayBy <= REPAY_CUTOFF;
    return true;
  });

  const columns: Column<Drawdown>[] = [
    { key: "facility", header: "Facility", cellClass: "font-semibold text-ink" },
    { key: "lender", header: "Lender" },
    { key: "purpose", header: "Purpose / Deal", render: (d) => <PurposeCell d={d} /> },
    { key: "amount", header: "Drawn", align: "right", cellClass: "font-semibold text-ink", render: (d) => money(d.amount), sortValue: (d) => d.amount },
    { key: "rate", header: "Rate", render: (d) => <span className="num text-ink-muted">{d.rate}</span> },
    { key: "drawDate", header: "Draw Date", render: (d) => fmtDate(d.drawDate, "short"), sortValue: (d) => d.drawDate },
    { key: "repayBy", header: "Repay By", render: (d) => (d.repayBy ? fmtDate(d.repayBy, "short") : <span className="text-ink-faint">—</span>) },
    { key: "status", header: "Status", render: (d) => <Pill>{d.status}</Pill> },
  ];

  const totalDrawn = drawdowns.reduce((s, d) => s + d.amount, 0);

  return (
    <div>
      <ScreenHeader title="Drawdowns" context="credit facilities">
        <Button>Export</Button>
        <Button variant="primary">+ New Draw</Button>
      </ScreenHeader>

      <KpiRow>
        <Kpi label="Facility Limit" value={money(kpis.facilityLimit, { decimals: 1 })} sub={`${kpis.facilities} facilities`} />
        <Kpi label="Drawn" value={money(kpis.drawn, { decimals: 1 })} sub={`${pct(kpis.utilisationPct, 0)} utilised`} tone="primary" />
        <Kpi label="Available" value={money(kpis.available, { decimals: 1 })} sub="undrawn headroom" />
        <Kpi label="Wtd. Rate" value={kpis.weightedRate} sub="blended coupon" />
      </KpiRow>

      <FilterChips options={FILTERS} active={filter} onChange={setFilter} trailing={`${filtered.length} draws`} />

      <DataTable
        columns={columns}
        rows={filtered}
        rowKey={(d) => d.id}
        emptyState={<ZeroResults onClear={() => setFilter(FILTERS[0])} />}
        totalRow={
          filter === FILTERS[0] ? (
            <>
              <Td className="font-semibold text-ink">Total drawn</Td>
              <Td>{null}</Td>
              <Td>{null}</Td>
              <Td align="right">{money(totalDrawn)}</Td>
              <Td>{null}</Td>
              <Td>{null}</Td>
              <Td>{null}</Td>
              <Td className="font-normal text-ink-muted">repaid by calls</Td>
            </>
          ) : undefined
        }
      />
    </div>
  );
}
