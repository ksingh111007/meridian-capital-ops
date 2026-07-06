"use client";

import { useState } from "react";
import type { DealDetail } from "@/lib/types";
import { fmtDate, money, multiple, pct } from "@/lib/format";
import { Breadcrumb, Card, DefRow, DocIcon, SectionTitle } from "@/components/ui/primitives";
import { Button } from "@/components/ui/Button";
import { Tabs } from "@/components/ui/controls";
import { Pill } from "@/components/ui/Pill";
import { DataTable, type Column } from "@/components/ui/DataTable";

type Cashflow = DealDetail["cashflows"][number];

const cashflowColumns: Column<Cashflow>[] = [
  { key: "date", header: "Date", render: (c) => fmtDate(c.date), sortValue: (c) => c.date },
  { key: "type", header: "Type", render: (c) => <span className="text-ink-muted">{c.type}</span> },
  { key: "amount", header: "Amount", align: "right", render: (c) => (
      <span className={`num font-medium ${c.amount > 0 ? "text-positive" : "text-ink"}`}>{money(c.amount, { sign: true })}</span>
    ), sortValue: (c) => c.amount },
  { key: "principalBalance", header: "Principal bal.", align: "right", render: (c) => money(c.principalBalance) },
];

const TABS = ["Overview", "Cashflows", "Covenants", "Documents"];

export function DealDetailScreen({ deal }: { deal: DealDetail }) {
  const [tab, setTab] = useState(TABS[0]);
  const fundLabel = deal.fundId === "fund-iii" ? "Fund III" : deal.fundId === "fund-ii" ? "Fund II" : "Fund I";

  const riskPanel = (
    <div className="flex flex-col gap-2">
      <DefRow label="Internal rating"><Pill tone={deal.risk.internalRating.startsWith("C") ? "red" : "amber"}>{deal.risk.internalRating}</Pill></DefRow>
      <DefRow label="Trend">{deal.risk.trend}</DefRow>
      <DefRow label="Covenants">
        <span className={deal.risk.covenants.includes("compliance") ? "text-positive" : "text-caution-strong"}>{deal.risk.covenants}</span>
      </DefRow>
      <DefRow label="Net leverage">{deal.risk.netLeverage}</DefRow>
      <DefRow label="Last review">{deal.risk.lastReview}</DefRow>
    </div>
  );

  const documents = (
    <ul className="flex flex-col gap-2">
      {deal.documents.map((doc) => (
        <li key={doc} className="flex items-center gap-2.5 text-xs text-ink-secondary">
          <DocIcon /> {doc}
          <button type="button" className="ml-auto text-[11px] font-semibold text-primary hover:underline">↓ PDF</button>
        </li>
      ))}
    </ul>
  );

  return (
    <div>
      <div className="flex flex-wrap items-center justify-between gap-3 border-b border-line px-5 py-3">
        <Breadcrumb backHref="/portfolio" backLabel="Portfolio" current={deal.name} sub={deal.borrower} />
        <div className="flex items-center gap-2">
          <Pill className="px-2.5 py-0.5 text-[11px]">{deal.status}</Pill>
          <Button>Export</Button>
          <Button variant="primary">+ Add note</Button>
        </div>
      </div>

      {/* metric tiles */}
      <div className="flex flex-wrap border-b border-line bg-card">
        {[
          { k: "Invested", v: money(deal.invested), big: true },
          { k: "Outstanding", v: money(deal.outstanding) },
          { k: "Coupon", v: deal.spread },
          { k: "Net IRR", v: pct(deal.netIrrPct), cls: deal.netIrrPct >= 10 ? "text-positive" : "text-caution-strong" },
          { k: "MOIC", v: multiple(deal.moic) },
          { k: "Fair Value", v: money(deal.fairValue) },
        ].map((t) => (
          <div key={t.k} className="min-w-28 flex-1 border-r border-dashed border-line px-4 py-3 last:border-0">
            <div className="text-[9.5px] font-semibold uppercase tracking-wider text-ink-faint">{t.k}</div>
            <div className={`num mt-1 font-bold ${t.big ? "text-xl" : "text-sm"} ${t.cls ?? "text-ink"}`}>{t.v}</div>
          </div>
        ))}
      </div>

      <Tabs tabs={TABS} active={tab} onChange={setTab} />

      {tab === "Overview" && (
        <div className="flex flex-col lg:flex-row">
          <div className="min-w-0 flex-1">
            <SectionTitle>Deal terms</SectionTitle>
            <div className="grid grid-cols-1 gap-x-8 gap-y-2 px-5 pb-4 sm:grid-cols-2">
              <DefRow label="Borrower">{deal.borrower}</DefRow>
              <DefRow label="Facility">{money(deal.facility)}</DefRow>
              <DefRow label="Sector">{deal.sector}</DefRow>
              <DefRow label="Drawn">{money(deal.drawn)}</DefRow>
              <DefRow label="Country">{deal.country}</DefRow>
              <DefRow label="Maturity">{deal.maturity}</DefRow>
              <DefRow label="Fund">{fundLabel}</DefRow>
              <DefRow label="Spread / Floor">{deal.spreadFloor}</DefRow>
              <DefRow label="Tranche">{deal.tranche}</DefRow>
              <DefRow label="Upfront fee">{pct(deal.upfrontFeePct, 2)}</DefRow>
            </div>
            <SectionTitle>Cashflow schedule</SectionTitle>
            <DataTable columns={cashflowColumns} rows={deal.cashflows} rowKey={(c) => `${c.date}-${c.type}`} />
          </div>
          <aside className="w-full flex-none border-t border-line bg-fill px-4 py-4 lg:w-66 lg:border-l lg:border-t-0">
            <h3 className="pb-2 text-[10.5px] font-semibold uppercase tracking-wider text-ink-faint">Risk &amp; rating</h3>
            {riskPanel}
            <h3 className="pb-2 pt-5 text-[10.5px] font-semibold uppercase tracking-wider text-ink-faint">LP exposure · {deal.lpExposure.length}</h3>
            <div className="flex flex-col gap-2">
              {deal.lpExposure.map((lp) => (
                <div key={lp.investor} className="flex justify-between text-xs">
                  <span className="font-semibold text-ink">{lp.investor}</span>
                  <span className="num text-ink-muted">{money(lp.amount)}</span>
                </div>
              ))}
            </div>
            <h3 className="pb-2 pt-5 text-[10.5px] font-semibold uppercase tracking-wider text-ink-faint">Documents</h3>
            {documents}
          </aside>
        </div>
      )}

      {tab === "Cashflows" && <DataTable columns={cashflowColumns} rows={deal.cashflows} rowKey={(c) => `${c.date}-${c.type}`} />}

      {tab === "Covenants" && (
        <div className="max-w-md px-5 py-4">
          <Card className="p-4">{riskPanel}</Card>
        </div>
      )}

      {tab === "Documents" && <div className="max-w-md px-5 py-4">{documents}</div>}
    </div>
  );
}
