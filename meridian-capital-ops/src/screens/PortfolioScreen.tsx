"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";
import type { AttentionItem, Deal, PortfolioSummary } from "@/lib/types";
import { fmtDate, money, multiple, pct } from "@/lib/format";
import { ScreenHeader } from "@/components/shell/ScreenHeader";
import { Button } from "@/components/ui/Button";
import { DataTable, Td, type Column } from "@/components/ui/DataTable";
import { FilterChips, SearchInput } from "@/components/ui/controls";
import { MiniBarChart, StackedBar } from "@/components/ui/charts";
import { Pill } from "@/components/ui/Pill";
import { ZeroResults } from "@/components/ui/states";

const FILTERS = ["All deals", "Performing", "On watch", "Non-accrual"];

export function PortfolioScreen({ summary, deals, attention }: { summary: PortfolioSummary; deals: Deal[]; attention: AttentionItem[] }) {
  const router = useRouter();
  const [filter, setFilter] = useState(FILTERS[0]);
  const [query, setQuery] = useState("");

  const filtered = deals.filter((d) => {
    const byStatus =
      filter === "All deals" || (filter === "Performing" && d.status === "Performing") ||
      (filter === "On watch" && d.status === "Watch") || (filter === "Non-accrual" && d.status === "Non-accrual");
    const q = query.trim().toLowerCase();
    return byStatus && (!q || d.name.toLowerCase().includes(q) || d.borrower.toLowerCase().includes(q));
  });

  const irrCell = (d: Deal) => (
    <span className={`num font-semibold ${d.status === "Non-accrual" ? "text-danger" : d.irrTrend === "down" ? "text-caution-strong" : d.irrTrend === "up" ? "text-positive" : "text-ink"}`}>
      {pct(d.netIrrPct)} {d.irrTrend === "up" ? "▲" : d.irrTrend === "down" ? "▼" : ""}
    </span>
  );

  const columns: Column<Deal>[] = [
    { key: "name", header: "Deal", cellClass: "font-semibold text-ink", sortValue: (d) => d.name },
    { key: "borrower", header: "Borrower", sortValue: (d) => d.borrower },
    { key: "fund", header: "Fund", render: (d) => <span className="text-ink-muted">{d.fundId === "fund-iii" ? "III" : d.fundId === "fund-ii" ? "II" : "I"}</span> },
    { key: "invested", header: "Invested", align: "right", render: (d) => money(d.invested), sortValue: (d) => d.invested },
    { key: "outstanding", header: "Outstanding", align: "right", cellClass: "font-semibold text-ink", render: (d) => money(d.outstanding), sortValue: (d) => d.outstanding },
    { key: "spread", header: "Yield", render: (d) => <span className="num text-ink-muted">{d.spread}</span> },
    { key: "irr", header: "Net IRR", align: "right", render: irrCell, sortValue: (d) => d.netIrrPct },
    { key: "moic", header: "MOIC", align: "right", render: (d) => multiple(d.moic), sortValue: (d) => d.moic },
    { key: "status", header: "Status", render: (d) => <Pill>{d.status}</Pill>, sortValue: (d) => d.status },
  ];

  const totals = {
    invested: deals.reduce((s, d) => s + d.invested, 0),
    outstanding: deals.reduce((s, d) => s + d.outstanding, 0),
  };

  return (
    <div>
      <ScreenHeader title="Portfolio" context={`all funds · as of ${fmtDate(summary.asOf)}`}>
        <SearchInput value={query} onChange={setQuery} placeholder="Search deals…" />
        <Button variant="primary">Export</Button>
      </ScreenHeader>

      {/* Needs-attention inbox — the ops start-of-day strip */}
      <section aria-label="Needs attention" className="border-b border-line bg-fill px-5 py-3">
        <div className="mb-2 flex items-baseline justify-between">
          <h2 className="text-[10.5px] font-semibold uppercase tracking-wider text-ink-faint">Needs attention</h2>
          <span className="text-[11px] text-ink-faint">{attention.length} items</span>
        </div>
        <div className="flex gap-2.5 overflow-x-auto pb-1">
          {attention.map((a) => (
            <Link
              key={a.id}
              href={a.href}
              className={`w-64 flex-none rounded-lg border bg-card p-2.5 shadow-card transition-colors hover:border-ink ${
                a.tone === "red" ? "border-danger-line" : a.tone === "amber" ? "border-caution-line" : a.mine ? "border-primary-line" : "border-line"
              }`}
            >
              <div className="flex items-start gap-2">
                <span aria-hidden className={`mt-1 h-2 w-2 flex-none rounded-full ${
                  a.tone === "red" ? "bg-danger" : a.tone === "amber" ? "bg-caution-strong" : a.tone === "blue" ? "bg-primary" : "bg-line-strong"
                }`} />
                <span className="min-w-0">
                  <span className="block text-xs font-semibold leading-snug text-ink">
                    {a.title} {a.mine && <span className="ml-1 rounded-full bg-primary-soft px-1.5 text-[9px] font-bold text-primary">YOU</span>}
                  </span>
                  <span className="mt-0.5 block truncate text-[11px] text-ink-muted">{a.detail}</span>
                </span>
              </div>
            </Link>
          ))}
        </div>
      </section>

      {/* whole-book KPIs + charts */}
      <div className="grid grid-cols-2 gap-3 border-b border-line bg-fill px-5 py-4 lg:grid-cols-4">
        <KpiCard label="Invested Capital" value={money(summary.investedCapital)} sub={`${summary.activeDeals} active deals`} />
        <KpiCard label="Net IRR" value={pct(summary.netIrrPct)} sub="since inception" valueClass="text-positive" />
        <KpiCard label="Blended MOIC" value={multiple(summary.blendedMoic)} sub="TVPI to date" />
        <KpiCard label="On Watch" value={String(summary.onWatchCount)} sub={`${money(summary.onWatchExposure)} exposure`} valueClass="text-caution-strong" />
        <div className="col-span-2 rounded-lg border border-line bg-card px-3.5 py-3 shadow-card">
          <div className="text-[10px] font-semibold uppercase tracking-wider text-ink-faint">Portfolio value · 8 quarters</div>
          <MiniBarChart values={summary.valueTrend} label="Portfolio value trend over 8 quarters" />
        </div>
        <div className="col-span-2 rounded-lg border border-line bg-card px-3.5 py-3 shadow-card">
          <div className="mb-3 text-[10px] font-semibold uppercase tracking-wider text-ink-faint">Exposure by status</div>
          <StackedBar segments={[
            { pct: summary.exposureMix.performingPct, color: "var(--color-positive)", label: "Performing" },
            { pct: summary.exposureMix.watchPct, color: "var(--color-caution-strong)", label: "Watch" },
            { pct: summary.exposureMix.nonAccrualPct, color: "var(--color-danger)", label: "Non-accrual" },
          ]} />
        </div>
      </div>

      <FilterChips options={FILTERS} active={filter} onChange={setFilter} trailing={`${filtered.length} deals`} />

      <DataTable
        columns={columns}
        rows={filtered}
        rowKey={(d) => d.id}
        onRowClick={(d) => router.push(`/portfolio/deals/${d.id}`)}
        rowClass={(d) => (d.status === "Non-accrual" ? "bg-danger-soft/40" : "")}
        emptyState={<ZeroResults onClear={() => { setFilter(FILTERS[0]); setQuery(""); }} />}
        totalRow={
          filter === FILTERS[0] && !query ? (
            <>
              <Td className="font-semibold text-ink">Portfolio</Td>
              <Td className="text-ink-muted">{deals.length} borrowers</Td>
              <Td>{null}</Td>
              <Td align="right">{money(totals.invested)}</Td>
              <Td align="right">{money(totals.outstanding)}</Td>
              <Td className="num text-ink-muted">S+2.9%</Td>
              <Td align="right" className="text-positive">{pct(summary.netIrrPct)}</Td>
              <Td align="right">{multiple(summary.blendedMoic)}</Td>
              <Td>{null}</Td>
            </>
          ) : undefined
        }
      />
    </div>
  );
}

function KpiCard({ label, value, sub, valueClass = "text-ink" }: { label: string; value: string; sub: string; valueClass?: string }) {
  return (
    <div className="rounded-lg border border-line bg-card px-3.5 py-3 shadow-card">
      <div className="text-[10px] font-semibold uppercase tracking-wider text-ink-faint">{label}</div>
      <div className={`num mt-1.5 text-[22px] font-bold leading-none tracking-tight ${valueClass}`}>{value}</div>
      <div className="mt-1.5 text-[11px] text-ink-muted">{sub}</div>
    </div>
  );
}
