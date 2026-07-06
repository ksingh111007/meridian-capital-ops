"use client";

import Link from "next/link";
import { useState } from "react";
import type { Fund, LegalEntity, ShareClass } from "@/lib/types";
import { money, pct } from "@/lib/format";
import { ScreenHeader } from "@/components/shell/ScreenHeader";
import { Button } from "@/components/ui/Button";
import { DataTable, type Column } from "@/components/ui/DataTable";
import { Kpi, KpiRow } from "@/components/ui/Kpi";
import { Pill } from "@/components/ui/Pill";
import { SectionTitle } from "@/components/ui/primitives";

interface Kpis { funds: number; investing: number; committed: number; calledPct: number; calledAmount: number; legalEntities: number }

export function FundsScreen({
  kpis, funds, entities, shareClasses,
}: {
  kpis: Kpis;
  funds: Fund[];
  entities: LegalEntity[];
  shareClasses: ShareClass[];
}) {
  const [selectedId, setSelectedId] = useState(funds[0]?.id ?? "fund-iii");
  const selected = funds.find((f) => f.id === selectedId) ?? funds[0];
  const fundEntities = entities.filter((e) => e.fundId === selected.id);
  const fundClasses = shareClasses.filter((c) => c.fundId === selected.id);

  const columns: Column<Fund>[] = [
    { key: "name", header: "Fund", cellClass: "font-semibold text-ink", sortValue: (f) => f.name },
    { key: "vintage", header: "Vintage", sortValue: (f) => f.vintage },
    { key: "committed", header: "Committed", align: "right", cellClass: "font-semibold text-ink", render: (f) => money(f.committed), sortValue: (f) => f.committed },
    { key: "called", header: "Called", align: "right", render: (f) => pct(f.calledPct, 0), sortValue: (f) => f.calledPct },
    { key: "strategy", header: "Strategy", render: (f) => <span className="text-ink-muted">{f.strategy}</span> },
    { key: "waterfall", header: "Waterfall", render: (f) => <span className="text-ink-muted">{f.waterfallType}</span> },
    { key: "status", header: "Status", render: (f) => <Pill>{f.status}</Pill>, sortValue: (f) => f.status },
  ];

  return (
    <div>
      <ScreenHeader title={<><span className="font-medium text-ink-faint">Admin /</span> Funds &amp; Entities</>}>
        <Button>Export</Button>
        <Button variant="primary">+ New Fund</Button>
      </ScreenHeader>

      <KpiRow>
        <Kpi label="Funds" value={kpis.funds} sub={`${kpis.investing} investing`} />
        <Kpi label="Committed" value={money(kpis.committed)} sub="across vintages" />
        <Kpi label="Called" value={pct(kpis.calledPct, 0)} sub={money(kpis.calledAmount)} tone="primary" />
        <Kpi label="Legal Entities" value={kpis.legalEntities} sub="GP, feeders, blockers" />
      </KpiRow>

      <DataTable
        columns={columns}
        rows={funds}
        rowKey={(f) => f.id}
        onRowClick={(f) => setSelectedId(f.id)}
        rowClass={(f) => (f.id === selected.id ? "bg-primary-soft/40" : "")}
      />

      <SectionTitle>{selected.name} · structure</SectionTitle>
      <div className="flex flex-col border-t border-dashed border-line sm:flex-row">
        <div className="flex-1 border-b border-dashed border-line px-5 py-3.5 sm:border-b-0 sm:border-r">
          <h3 className="text-[10px] font-semibold uppercase tracking-wider text-ink-faint">Legal entities · {fundEntities.length}</h3>
          <div className="mt-2 flex flex-col gap-1.5">
            {fundEntities.length === 0 && <span className="text-xs text-ink-muted">No entities in this mock.</span>}
            {fundEntities.map((e) => (
              <div key={e.name} className="flex items-center justify-between gap-3 text-xs">
                <span className="font-semibold text-ink">{e.name}</span>
                <span className="text-ink-muted">{e.kind}</span>
              </div>
            ))}
          </div>
        </div>
        <div className="flex-1 px-5 py-3.5">
          <h3 className="text-[10px] font-semibold uppercase tracking-wider text-ink-faint">Share classes &amp; waterfall</h3>
          <div className="mt-2 flex flex-col gap-1.5">
            {fundClasses.length === 0 && <span className="text-xs text-ink-muted">No share classes in this mock.</span>}
            {fundClasses.map((c) => (
              <div key={c.name} className="flex items-center justify-between gap-3 text-xs">
                <span className="font-semibold text-ink">{c.name}</span>
                <span className="num text-ink-muted">
                  mgmt {c.mgmtFeePct.toFixed(2)}% · carry {c.carryPct}% · pref {c.prefPct}%
                </span>
              </div>
            ))}
            <div className="mt-0.5 flex items-center justify-between gap-3 border-t border-dashed border-line pt-2 text-xs">
              <span className="font-semibold text-ink">Waterfall</span>
              <Link href="/distributions" className="font-semibold text-primary hover:underline">
                {selected.waterfallType} whole-fund →
              </Link>
            </div>
            <div className="flex items-center justify-between gap-3 text-xs">
              <span className="font-semibold text-ink">Base currency</span>
              <span className="text-ink-muted">{selected.baseCurrency}</span>
            </div>
          </div>
        </div>
      </div>
      <p className="border-t border-line px-5 py-3 text-[11px] text-ink-muted">
        Fund terms live here — the fee, carry &amp; pref set on a share class drive the Distributions waterfall math.
      </p>
    </div>
  );
}
