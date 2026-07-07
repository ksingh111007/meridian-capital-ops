"use client";

import Link from "next/link";
import { useState } from "react";
import type { Distribution, InvestorPayout, WaterfallTier } from "@/lib/types";
import { money, pct } from "@/lib/format";
import { postJson } from "@/lib/mutate";
import { ScreenHeader } from "@/components/shell/ScreenHeader";
import { Button } from "@/components/ui/Button";
import { DataTable, Td, type Column } from "@/components/ui/DataTable";
import { FilterChips, Tabs } from "@/components/ui/controls";
import { SplitBar } from "@/components/ui/charts";
import { Pill } from "@/components/ui/Pill";
import { useToast } from "@/components/ui/Toast";

const TABS = ["Waterfall calculation", "Investor payouts"];

function fundName(fundId: string) {
  return fundId === "fund-iii" ? "Fund III" : fundId === "fund-ii" ? "Fund II" : "Fund I";
}

/** "#D-118" + "2026-09-30" → "D-118 · Sep 2026" */
function chipLabel(d: Distribution) {
  const dt = new Date(`${d.paymentDate}T00:00:00Z`);
  const my = dt.toLocaleDateString("en-US", { month: "short", year: "numeric", timeZone: "UTC" });
  return `${d.ref.replace("#", "")} · ${my}`;
}

/** "2026-09-30" → "Sep 30, 2026" (footer strip style). */
function longDate(iso: string) {
  return new Date(`${iso}T00:00:00Z`).toLocaleDateString("en-US", { month: "short", day: "2-digit", year: "numeric", timeZone: "UTC" });
}

/** Screen 4a — allocation summary over the waterfall calculation ledger + per-LP payouts. */
export function DistributionsScreen({ distributions }: { distributions: Distribution[] }) {
  const toast = useToast();
  const [selectedId, setSelectedId] = useState(distributions[0]?.id ?? "");
  const [tab, setTab] = useState(TABS[0]);

  const dist = distributions.find((d) => d.id === selectedId) ?? distributions[0];
  const lpPct = +((dist.lpTotal / dist.distributable) * 100).toFixed(1);
  const gpPct = +(100 - lpPct).toFixed(1);

  const tier = (prefix: string) => dist.tiers.find((t) => t.tier.startsWith(prefix));
  const returnOfCapital = tier("1")?.distributed ?? 0;
  const preferred = tier("2")?.distributed ?? 0;
  const profitSplit = tier("4")?.lpShare ?? 0;

  async function retryWire() {
    const { ok, error } = await postJson("wires/wire-8847/retry");
    if (!ok) {
      toast.push({ kind: "error", title: "Wire retry failed", detail: error });
      return;
    }
    toast.push({ kind: "success", title: "Wire retry queued" });
  }

  // ---------- waterfall tab ----------

  const waterfallRows: WaterfallTier[] = [
    { tier: "Distributable", basis: dist.sourceNote, rate: "—", distributed: dist.distributable, lpShare: null, gpShare: null, poolLeft: dist.distributable },
    ...dist.tiers,
  ];

  const tierColumns: Column<WaterfallTier>[] = [
    { key: "tier", header: "Tier", cellClass: "font-semibold text-ink" },
    { key: "basis", header: "Basis / rule", cellClass: "text-ink-muted" },
    { key: "rate", header: "Rate", align: "right" },
    { key: "distributed", header: "Distributed", align: "right", cellClass: "font-semibold text-ink", render: (t) => money(t.distributed) },
    { key: "lpShare", header: "LP share", align: "right", render: (t) => (t.lpShare !== null ? <span className="font-semibold text-primary">{money(t.lpShare)}</span> : <span className="text-ink-faint">—</span>) },
    { key: "gpShare", header: "GP share", align: "right", render: (t) => (t.gpShare !== null ? <span className="font-semibold text-ink">{money(t.gpShare)}</span> : <span className="text-ink-faint">—</span>) },
    { key: "poolLeft", header: "Pool left", align: "right", render: (t) => money(t.poolLeft) },
  ];

  const tierDistributedTotal = dist.tiers.reduce((s, t) => s + t.distributed, 0);

  // ---------- payouts tab ----------

  const payoutColumns: Column<InvestorPayout>[] = [
    {
      key: "investor", header: "Investor",
      render: (p) => (
        <span className="block">
          <span className="font-semibold text-ink">
            {p.investor}
            {p.status === "Blocked" && (
              <span className="ml-1.5 rounded-full bg-danger-soft px-1.5 py-px text-[10px] font-bold text-danger">no wire instructions</span>
            )}
          </span>
          {p.status === "Blocked" && p.blockedReason && (
            <span className="mt-0.5 block text-[10.5px] text-danger">{p.blockedReason}</span>
          )}
        </span>
      ),
    },
    { key: "commitment", header: "Commitment", align: "right", cellClass: "text-ink-muted", render: (p) => money(p.commitment) },
    { key: "amount", header: "Amount", align: "right", cellClass: "font-semibold text-ink", render: (p) => money(p.amount) },
    { key: "pct", header: "% of LP total", align: "right", render: (p) => pct(p.pctOfLpTotal) },
    { key: "wireRef", header: "Wire ref", cellClass: "text-ink-muted", render: (p) => p.wireRef ?? "—" },
    {
      key: "status", header: "Status",
      render: (p) => (
        <span className="inline-flex items-center gap-1.5">
          <Pill>{p.status}</Pill>
          {p.status === "Exception" && (
            <Button variant="ghost" size="sm" onClick={(e) => { e.stopPropagation(); retryWire(); }}>Retry</Button>
          )}
        </span>
      ),
    },
  ];

  const blocked = dist.payouts.filter((p) => p.status === "Blocked");

  return (
    <div>
      <ScreenHeader title="Distributions" context="Fund III · Q3 2026">
        <Button>Export</Button>
        <Button variant="primary">+ New Distribution</Button>
      </ScreenHeader>

      <FilterChips
        options={distributions.map(chipLabel)}
        active={chipLabel(dist)}
        onChange={(label) => {
          const next = distributions.find((d) => chipLabel(d) === label);
          if (next) setSelectedId(next.id);
        }}
        trailing={`${dist.payouts.length} LPs · ${dist.waterfallType} waterfall`}
      />

      {/* header band: which distribution + headline LP amount */}
      <div className="flex flex-wrap items-center justify-between gap-3 border-b border-line bg-card px-5 py-3.5">
        <div className="text-sm font-bold text-ink">
          Distribution {dist.ref} · {fundName(dist.fundId)} · <span className="font-medium text-ink-secondary">{dist.waterfallType} whole-fund waterfall</span>
        </div>
        <div className="text-right">
          <div className="text-[10px] font-semibold uppercase tracking-wider text-ink-faint">To investors</div>
          <div className="num text-[22px] font-bold leading-tight tracking-tight text-primary">{money(dist.lpTotal)}</div>
        </div>
      </div>

      {/* allocation summary bar */}
      <div className="border-b border-line bg-fill px-5 py-3.5">
        <div className="mb-2 flex items-baseline justify-between">
          <div className="text-[10px] font-semibold uppercase tracking-wider text-ink-faint">
            Allocation of {money(dist.distributable, { decimals: 1 })} distributable pool
          </div>
          <div className="text-[11px] font-semibold text-ink-secondary">
            <span className="text-primary">LP {pct(lpPct)}</span> · GP {pct(gpPct)}
          </div>
        </div>
        <SplitBar lpPct={lpPct} />
        <div className="mt-2 flex flex-wrap items-baseline justify-between gap-2 text-[11px] text-ink-secondary">
          <span>
            Return of Capital <b className="num text-ink">{money(returnOfCapital)}</b> · Preferred <b className="num text-ink">{money(preferred)}</b> · Profit split <b className="num text-ink">{money(profitSplit)}</b>
          </span>
          <span>GP catch-up + carry <b className="num text-ink">{money(dist.gpTotal)}</b></span>
        </div>
      </div>

      <Tabs tabs={TABS} active={tab} onChange={setTab} />

      {tab === TABS[0] ? (
        <DataTable
          columns={tierColumns}
          rows={waterfallRows}
          rowKey={(t) => t.tier}
          totalRow={
            <>
              <Td className="font-semibold text-ink">Total paid</Td>
              <Td>{null}</Td>
              <Td>{null}</Td>
              <Td align="right">{money(tierDistributedTotal)}</Td>
              <Td align="right" className="text-primary">{money(dist.lpTotal)}</Td>
              <Td align="right">{money(dist.gpTotal)}</Td>
              <Td align="right" className="text-positive">balanced ✓</Td>
            </>
          }
        />
      ) : (
        <>
          <DataTable
            columns={payoutColumns}
            rows={dist.payouts}
            rowKey={(p) => p.investorId}
            rowClass={(p) => (p.status === "Exception" || p.status === "Blocked" ? "bg-danger-soft/40" : "")}
            totalRow={
              <>
                <Td className="font-semibold text-ink">Total</Td>
                <Td align="right" className="text-ink-muted">{money(dist.payouts.reduce((s, p) => s + p.commitment, 0))}</Td>
                <Td align="right">{money(dist.lpTotal)}</Td>
                <Td align="right">100%</Td>
                <Td>{null}</Td>
                <Td>{null}</Td>
              </>
            }
          />
          {blocked.length > 0 && (
            <div className="mx-5 my-3 rounded-md border border-caution-line bg-caution-soft px-3 py-2 text-[11px] text-caution">
              <b>{blocked.length} payout{blocked.length > 1 ? "s" : ""} blocked</b> — {blocked.map((p) => p.investor).join(", ")} has no wire
              instructions on file. Add banking details in the{" "}
              <Link href="/admin/investors" className="font-semibold text-primary hover:underline">Investor Registry</Link> to release it.
            </div>
          )}
        </>
      )}

      {/* footer strip */}
      <div className="flex flex-wrap border-t border-line bg-fill">
        {[
          { k: "LP allocation", v: pct(lpPct) },
          { k: "GP carry rate (effective)", v: pct(gpPct) },
          { k: "Payment date", v: longDate(dist.paymentDate) },
        ].map((c) => (
          <div key={c.k} className="min-w-40 flex-1 border-r border-dashed border-line px-5 py-3 text-xs text-ink-muted last:border-0">
            {c.k} <b className="num ml-1 text-ink">{c.v}</b>
          </div>
        ))}
      </div>
    </div>
  );
}
