"use client";

import Link from "next/link";
import type { PortalAccount, PortalActivityRow, PortalDocument } from "@/lib/types";
import { fmtDate, money, multiple, pct } from "@/lib/format";
import { Card, DocIcon, ProgressBar } from "@/components/ui/primitives";
import { DataTable, type Column } from "@/components/ui/DataTable";
import { Pill } from "@/components/ui/Pill";
import { DownloadButton, PortalPageHeader, PortalSection, SignedAmount, StatStrip } from "./shared";

/** Screen 6b — Investor Portal Overview: the LP's capital account at a glance. */
export function PortalOverviewScreen({
  account, activity, documents,
}: {
  account: PortalAccount;
  activity: PortalActivityRow[];
  documents: PortalDocument[];
}) {
  const recentActivity = activity.slice(0, 4);
  const recentDocs = documents.slice(0, 4);

  const activityColumns: Column<PortalActivityRow>[] = [
    { key: "date", header: "Date", render: (r) => fmtDate(r.date) },
    { key: "fund", header: "Fund", render: (r) => <span className="text-ink-muted">{r.fund}</span> },
    { key: "type", header: "Type", render: (r) => `${r.type} ${r.reference}` },
    { key: "amount", header: "Amount", align: "right", render: (r) => <SignedAmount value={r.amount} /> },
    { key: "status", header: "Status", render: (r) => <Pill>{r.status}</Pill> },
  ];

  return (
    <div>
      <PortalPageHeader
        title={`Good morning, ${account.investor}`}
        subtitle={`Your capital account across Meridian Credit funds · as of ${fmtDate(account.asOf)}`}
      />

      <div className="mt-4">
        <StatStrip
          stats={[
            { label: "Commitment", value: money(account.stats.commitment, { decimals: 1 }) },
            { label: "Paid-in", value: money(account.stats.paidIn, { decimals: 1 }) },
            { label: "Distributions", value: money(account.stats.distributions, { decimals: 1 }), valueClass: "text-positive" },
            { label: "Current NAV", value: money(account.stats.nav, { decimals: 1 }), valueClass: "text-primary" },
            { label: "Net IRR", value: pct(account.stats.netIrrPct) },
          ]}
        />
      </div>

      <PortalSection>Your funds</PortalSection>
      <div className="flex flex-col gap-3 md:flex-row">
        {account.funds.map((f) => (
          <Card key={f.fundId} className="min-w-0 flex-1 p-4">
            <div className="flex items-baseline justify-between gap-2">
              <span className="text-[13px] font-bold text-ink">{f.name}</span>
              <span className="text-[10px] text-ink-muted">{f.vintage} vintage</span>
            </div>
            <div className="mt-2.5 flex gap-5 text-[11px] text-ink-muted">
              {[
                { k: "Commitment", v: money(f.commitment, { decimals: 1 }) },
                { k: "NAV", v: money(f.nav, { decimals: 1 }), cls: "text-primary" },
                { k: "Net IRR", v: pct(f.netIrrPct) },
                { k: "DPI", v: multiple(f.dpi) },
              ].map((s) => (
                <span key={s.k}>
                  {s.k}
                  <b className={`num mt-0.5 block text-[13px] font-bold ${s.cls ?? "text-ink"}`}>{s.v}</b>
                </span>
              ))}
            </div>
            <div className="mt-3">
              <div className="mb-1 flex justify-between text-[9.5px] text-ink-muted">
                <span>Called {pct(f.calledPct, 0)}</span>
                <span className="num">{money(f.calledAmount, { decimals: 1 })} of {money(f.commitment, { decimals: 1 })}</span>
              </div>
              <ProgressBar pct={f.calledPct} />
            </div>
          </Card>
        ))}
      </div>

      <div className="mt-1 flex flex-col gap-4 lg:flex-row">
        <div className="min-w-0 flex-[1.15]">
          <PortalSection
            action={<Link href="/portal/activity" className="text-[10.5px] font-medium text-primary hover:underline">View all</Link>}
          >
            Recent capital activity
          </PortalSection>
          <Card className="overflow-hidden">
            <DataTable columns={activityColumns} rows={recentActivity} rowKey={(r) => r.reference} />
          </Card>
        </div>
        <div className="min-w-0 flex-1">
          <PortalSection>Statements &amp; documents</PortalSection>
          <div className="flex flex-col gap-2">
            {recentDocs.map((d) => (
              <Card key={d.id} className="flex items-center gap-2.5 px-3 py-2">
                <DocIcon />
                <div className="min-w-0 flex-1">
                  <div className="truncate text-[11px] font-semibold text-ink">{d.name}</div>
                  <div className="text-[9.5px] text-ink-muted">PDF · {fmtDate(d.date)}</div>
                </div>
                <DownloadButton name={d.name} />
              </Card>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}
