"use client";

import { useState } from "react";
import type { FundId, Investor } from "@/lib/types";
import { money, pct } from "@/lib/format";
import { ScreenHeader } from "@/components/shell/ScreenHeader";
import { Button } from "@/components/ui/Button";
import { DataTable, type Column } from "@/components/ui/DataTable";
import { Kpi, KpiRow } from "@/components/ui/Kpi";
import { Pill } from "@/components/ui/Pill";
import { SearchInput } from "@/components/ui/controls";
import { SectionTitle } from "@/components/ui/primitives";
import { ZeroResults } from "@/components/ui/states";

interface Kpis { investors: number; commitments: number; kycVerifiedPct: number; kycInReview: number; wireOnFilePct: number; wireMissing: number }

const FUND_LABEL: Record<FundId, string> = { "fund-i": "I", "fund-ii": "II", "fund-iii": "III" };
const FUND_ORDER: FundId[] = ["fund-i", "fund-ii", "fund-iii"];

function totalCommitment(inv: Investor): number {
  return inv.commitments.reduce((s, c) => s + c.amount, 0);
}

function fundsLabel(inv: Investor): string {
  return FUND_ORDER.filter((f) => inv.commitments.some((c) => c.fundId === f)).map((f) => FUND_LABEL[f]).join(", ");
}

export function InvestorRegistryScreen({ kpis, investors }: { kpis: Kpis; investors: Investor[] }) {
  const [query, setQuery] = useState("");
  const [selectedId, setSelectedId] = useState(investors[0]?.id ?? "inv-redwood");
  const selected = investors.find((i) => i.id === selectedId) ?? investors[0];

  const q = query.trim().toLowerCase();
  const filtered = investors.filter((i) => !q || i.name.toLowerCase().includes(q) || i.type.toLowerCase().includes(q));

  const columns: Column<Investor>[] = [
    { key: "name", header: "Investor", cellClass: "font-semibold text-ink", sortValue: (i) => i.name },
    { key: "type", header: "Type", render: (i) => <span className="text-ink-muted">{i.type}</span>, sortValue: (i) => i.type },
    { key: "commitment", header: "Commitment", align: "right", cellClass: "font-semibold text-ink", render: (i) => money(totalCommitment(i)), sortValue: totalCommitment },
    { key: "funds", header: "Funds", render: (i) => <span className="text-ink-muted">{fundsLabel(i)}</span> },
    { key: "kyc", header: "KYC / AML", render: (i) => <Pill>{i.kycStatus}</Pill>, sortValue: (i) => i.kycStatus },
    { key: "wire", header: "Wire instr.", render: (i) => <Pill>{i.wireInstructionsOnFile ? "On file" : "Missing"}</Pill>, sortValue: (i) => String(i.wireInstructionsOnFile) },
  ];

  return (
    <div>
      <ScreenHeader title={<><span className="font-medium text-ink-faint">Admin /</span> Investor Registry</>}>
        <SearchInput value={query} onChange={setQuery} placeholder="Search investors…" />
        <Button>Export</Button>
        <Button variant="primary">+ Add LP</Button>
      </ScreenHeader>

      <KpiRow>
        <Kpi label="Investors" value={kpis.investors} sub="across 3 funds" />
        <Kpi label="Commitments" value={money(kpis.commitments)} sub="total" />
        <Kpi label="KYC Verified" value={pct(kpis.kycVerifiedPct, 0)} sub={`${kpis.kycInReview} in review`} />
        <Kpi label="Wire Instr. on File" value={pct(kpis.wireOnFilePct, 0)} sub={`${kpis.wireMissing} missing`} tone="primary" />
      </KpiRow>

      <DataTable
        columns={columns}
        rows={filtered}
        rowKey={(i) => i.id}
        onRowClick={(i) => setSelectedId(i.id)}
        rowClass={(i) => (i.id === selected.id ? "bg-primary-soft/40" : "")}
        emptyState={<ZeroResults onClear={() => setQuery("")} />}
      />

      <SectionTitle>{selected.name} · profile</SectionTitle>
      <div className="flex flex-col border-t border-dashed border-line sm:flex-row">
        <div className="flex-1 border-b border-dashed border-line px-5 py-3.5 sm:border-b-0 sm:border-r">
          <h3 className="text-[10px] font-semibold uppercase tracking-wider text-ink-faint">Commitments</h3>
          <div className="mt-2 flex flex-col gap-1.5 text-xs">
            {selected.commitments.map((c) => (
              <div key={c.fundId} className="flex items-center justify-between gap-3">
                <span className="text-ink-muted">Fund {FUND_LABEL[c.fundId]}</span>
                <span className="num font-semibold text-ink">{money(c.amount)}</span>
              </div>
            ))}
            <div className="flex items-center justify-between gap-3 border-t border-dashed border-line pt-1.5">
              <span className="text-ink-muted">Total</span>
              <span className="num font-semibold text-primary">{money(totalCommitment(selected))}</span>
            </div>
          </div>
        </div>
        <div className="flex-1 border-b border-dashed border-line px-5 py-3.5 sm:border-b-0 sm:border-r">
          <h3 className="text-[10px] font-semibold uppercase tracking-wider text-ink-faint">Banking</h3>
          {selected.profile && selected.wireInstructionsOnFile ? (
            <div className="mt-2 flex flex-col gap-1.5 text-xs">
              <div className="flex items-center justify-between gap-3"><span className="text-ink-muted">Bank</span><span className="font-semibold text-ink">{selected.profile.bank}</span></div>
              <div className="flex items-center justify-between gap-3"><span className="text-ink-muted">ABA</span><span className="num font-semibold text-ink">{selected.profile.abaMasked}</span></div>
              <div className="flex items-center justify-between gap-3"><span className="text-ink-muted">Account</span><span className="num font-semibold text-ink">{selected.profile.accountMasked}</span></div>
              <div className="flex items-center justify-between gap-3"><span className="text-ink-muted">Verified</span><span className="font-semibold text-positive">{selected.profile.bankingVerified}</span></div>
            </div>
          ) : (
            <p className="mt-2 text-xs font-medium leading-relaxed text-danger">
              No wire instructions on file — this LP cannot be paid until banking details are added.
            </p>
          )}
        </div>
        <div className="flex-1 px-5 py-3.5">
          <h3 className="text-[10px] font-semibold uppercase tracking-wider text-ink-faint">KYC / AML</h3>
          <div className="mt-2 flex flex-col gap-1.5 text-xs">
            <div className="flex items-center justify-between gap-3">
              <span className="text-ink-muted">Status</span>
              <span className={`font-semibold ${selected.kycStatus === "Verified" ? "text-positive" : "text-caution-strong"}`}>{selected.kycStatus}</span>
            </div>
            <div className="flex items-center justify-between gap-3"><span className="text-ink-muted">Docs</span><span className="font-semibold text-ink">{selected.profile?.kycDocs ?? "—"}</span></div>
            <div className="flex items-center justify-between gap-3"><span className="text-ink-muted">Review due</span><span className="font-semibold text-ink">{selected.profile?.kycReviewDue ?? "—"}</span></div>
          </div>
        </div>
      </div>
    </div>
  );
}
