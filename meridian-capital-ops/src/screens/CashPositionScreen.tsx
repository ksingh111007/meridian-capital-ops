"use client";

import type { CashPosition } from "@/lib/types";
import { money } from "@/lib/format";
import { ScreenHeader } from "@/components/shell/ScreenHeader";
import { Button } from "@/components/ui/Button";
import { DataTable, Td, type Column } from "@/components/ui/DataTable";
import { MiniBarChart } from "@/components/ui/charts";
import { ProgressBar, SectionTitle } from "@/components/ui/primitives";

type Week = CashPosition["weeks"][number];
type Account = CashPosition["accounts"][number];

/** Screen 4d — liquidity now, forecast, and per-account balances. */
export function CashPositionScreen({ cash }: { cash: CashPosition }) {
  const weekColumns: Column<Week>[] = [
    { key: "label", header: "Week", cellClass: "font-semibold text-ink" },
    { key: "inflows", header: "Inflows (calls)", align: "right", render: (w) => <span className="text-primary">{money(w.inflows, { sign: true, decimals: 1 })}</span> },
    { key: "outflows", header: "Outflows (distrib + repay)", align: "right", cellClass: "text-ink-muted", render: (w) => money(w.outflows, { decimals: 1 }) },
    {
      key: "net", header: "Net", align: "right",
      render: (w) => (
        <span className={`font-semibold ${w.net >= 0 ? "text-positive" : "text-caution-strong"}`}>{money(w.net, { sign: true, decimals: 1 })}</span>
      ),
    },
    { key: "projectedBalance", header: "Proj. balance", align: "right", render: (w) => money(w.projectedBalance, { decimals: 1 }) },
  ];

  const accountColumns: Column<Account>[] = [
    { key: "custodian", header: "Custodian", cellClass: "font-semibold text-ink" },
    { key: "account", header: "Account" },
    { key: "currency", header: "Ccy", cellClass: "text-ink-muted" },
    { key: "type", header: "Type", cellClass: "text-ink-muted" },
    { key: "balance", header: "Balance", align: "right", cellClass: "font-semibold text-ink", render: (a) => money(a.balance, { decimals: 1 }) },
  ];

  const totalBalance = cash.accounts.reduce((s, a) => s + a.balance, 0);

  return (
    <div>
      <ScreenHeader title="Cash Position" context="Fund III">
        <Button variant="primary">Forecast</Button>
      </ScreenHeader>

      {/* liquidity KPIs + forecast band */}
      <div className="grid grid-cols-2 gap-3 border-b border-line bg-fill px-5 py-4 lg:grid-cols-4">
        <KpiCard label="Cash on Hand" value={money(cash.cashOnHand, { decimals: 1 })} sub={`${cash.accountsCount} accounts`} />
        <KpiCard label="Uncalled Capital" value={money(cash.uncalledCapital, { decimals: 0 })} sub={`${cash.uncalledLps} LPs`} />
        <KpiCard label="Facility Headroom" value={money(cash.facilityHeadroom, { decimals: 0 })} sub={`of ${money(cash.facilityLimit, { decimals: 0 })}`} />
        <KpiCard label="Net 30-Day Proj." value={money(cash.net30DayProjection, { decimals: 1 })} sub="calls less payouts" valueClass="text-caution-strong" />
        <div className="col-span-2 rounded-lg border border-line bg-card px-3.5 py-3 shadow-card">
          <div className="text-[10px] font-semibold uppercase tracking-wider text-ink-faint">13-week cash forecast</div>
          <MiniBarChart values={cash.forecastBars} label="13-week cash forecast" />
        </div>
        <div className="col-span-2 rounded-lg border border-line bg-card px-3.5 py-3 shadow-card">
          <div className="mb-3 text-[10px] font-semibold uppercase tracking-wider text-ink-faint">Cover of next 30-day outflows</div>
          <div className="flex items-center gap-3">
            <ProgressBar pct={70} className="h-2.5 flex-1" />
            <span className="num text-lg font-bold text-ink">{cash.coverageRatio}×</span>
          </div>
          <div className="mt-2 text-[11px] text-ink-muted">cash + headroom vs projected outflows</div>
        </div>
      </div>

      <SectionTitle>Projected liquidity · next 4 weeks</SectionTitle>
      <DataTable columns={weekColumns} rows={cash.weeks} rowKey={(w) => w.label} />

      <SectionTitle>Accounts</SectionTitle>
      <DataTable
        columns={accountColumns}
        rows={cash.accounts}
        rowKey={(a) => a.account}
        totalRow={
          <>
            <Td className="font-semibold text-ink">Total</Td>
            <Td>{null}</Td>
            <Td>{null}</Td>
            <Td>{null}</Td>
            <Td align="right">{money(totalBalance, { decimals: 1 })}</Td>
          </>
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
