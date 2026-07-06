"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";
import type { CallAllocation, CapitalCall, WorkflowStage } from "@/lib/types";
import { daysUntil, fmtDate, money } from "@/lib/format";
import { ScreenHeader } from "@/components/shell/ScreenHeader";
import { Button } from "@/components/ui/Button";
import { Kpi, KpiRow } from "@/components/ui/Kpi";
import { FilterChips, SearchInput } from "@/components/ui/controls";
import { Pill } from "@/components/ui/Pill";
import { Td } from "@/components/ui/DataTable";
import { ZeroResults } from "@/components/ui/states";

const FILTERS = ["Group by transaction", "Flat list", "Due ≤ 7d", "Unwired", "Overdue"];
const FUNDED_STATUSES = new Set(["Wired", "Confirmed"]);

function fundLabel(fundId: string) {
  return fundId === "fund-iii" ? "III" : fundId === "fund-ii" ? "II" : "I";
}

/** Due-date cell with overdue aging ("4d late") on past-due, not-fully-funded calls. */
function DueDate({ call }: { call: CapitalCall }) {
  const days = daysUntil(call.dueDate);
  const unfunded = call.allocations.some((a) => !FUNDED_STATUSES.has(a.wireStatus));
  if (days < 0 && unfunded) {
    return (
      <span>
        {fmtDate(call.dueDate, "short")}{" "}
        <span className="rounded-full bg-danger-soft px-1.5 py-px text-[10px] font-bold text-danger">{-days}d late</span>
      </span>
    );
  }
  return <span>{fmtDate(call.dueDate, "short")}</span>;
}

function WireSummary({ allocations }: { allocations: CallAllocation[] }) {
  const wired = allocations.filter((a) => FUNDED_STATUSES.has(a.wireStatus)).length;
  return (
    <span className="inline-flex items-center gap-1.5 text-[11px] font-semibold text-ink-muted">
      {wired === allocations.length ? "confirmed" : `${wired}/${allocations.length} wired`}
      <span className="inline-flex gap-0.5">
        {allocations.map((a, i) => (
          <span key={i} className={`h-1.5 w-1.5 rounded-full ${
            FUNDED_STATUSES.has(a.wireStatus) ? "bg-positive" : a.wireStatus === "Overdue" ? "bg-danger" : "bg-line-strong"
          }`} />
        ))}
      </span>
    </span>
  );
}

export function CapitalCallsScreen({ calls, stages }: { calls: CapitalCall[]; stages: WorkflowStage[] }) {
  const router = useRouter();
  const [filter, setFilter] = useState(FILTERS[0]);
  const [query, setQuery] = useState("");
  const [collapsed, setCollapsed] = useState<Record<string, boolean>>(
    Object.fromEntries(calls.map((c) => [c.id, c.allocations.length > 3])),
  );

  const stageName = (call: CapitalCall) => stages.find((s) => s.order === call.currentStage)?.name ?? "";

  const q = query.trim().toLowerCase();
  const visible = calls.filter((c) => {
    if (q && !c.deal.toLowerCase().includes(q) && !c.borrower.toLowerCase().includes(q) &&
        !c.allocations.some((a) => a.investor.toLowerCase().includes(q))) return false;
    if (filter === "Due ≤ 7d") return daysUntil(c.dueDate) >= 0 && daysUntil(c.dueDate) <= 7;
    if (filter === "Unwired") return c.allocations.some((a) => !FUNDED_STATUSES.has(a.wireStatus));
    if (filter === "Overdue") return daysUntil(c.dueDate) < 0 && c.allocations.some((a) => !FUNDED_STATUSES.has(a.wireStatus));
    return true;
  });

  // KPIs computed from the data (not hard-coded) so filters stay honest
  const totalToCall = calls.reduce((s, c) => s + c.amount, 0);
  const dueThisWeek = calls.filter((c) => daysUntil(c.dueDate) >= 0 && daysUntil(c.dueDate) <= 7);
  const funded = calls.flatMap((c) => c.allocations).filter((a) => FUNDED_STATUSES.has(a.wireStatus)).reduce((s, a) => s + a.amount, 0);
  const overdueAllocs = calls.flatMap((c) => c.allocations).filter((a) => a.wireStatus === "Overdue");
  const investorsCalled = new Set(calls.flatMap((c) => c.allocations.map((a) => a.investorId))).size;
  const flat = filter === "Flat list";

  return (
    <div>
      <ScreenHeader title="Capital Calls" context="Fund III · Q3 2026">
        <SearchInput value={query} onChange={setQuery} placeholder="Search calls, LPs…" />
        <Button>Export</Button>
        <Button variant="primary" onClick={() => router.push("/capital-calls/new")}>+ New Call</Button>
      </ScreenHeader>

      <KpiRow>
        <Kpi label="Total to Call" value={money(totalToCall)} sub={`across ${calls.length} transactions`} />
        <Kpi label="Due This Week" value={money(dueThisWeek.reduce((s, c) => s + c.amount, 0))} sub={`${dueThisWeek.length} transactions`} tone="primary" />
        <Kpi label="Funded to Date" value={`${Math.round((funded / totalToCall) * 100)}%`} sub={`${money(funded)} received`} />
        <Kpi
          label="Overdue" value={overdueAllocs.length ? money(overdueAllocs.reduce((s, a) => s + a.amount, 0)) : "—"}
          sub={overdueAllocs.length ? `${overdueAllocs.length} LP wires past due` : `${investorsCalled} investors called`}
          tone={overdueAllocs.length ? "danger" : "default"}
        />
      </KpiRow>

      <FilterChips options={FILTERS} active={filter} onChange={setFilter}
        trailing={`${visible.length} transactions · ${visible.reduce((s, c) => s + c.allocations.length, 0)} investors`} />

      {visible.length === 0 ? (
        <ZeroResults message="No calls match these filters." onClear={() => { setFilter(FILTERS[0]); setQuery(""); }} />
      ) : (
        <div className="overflow-x-auto">
          <table className="w-full border-collapse text-left">
            <thead>
              <tr>
                {["Transaction / Investor", "Fund", "Tranche", "Borrower", "Commitment", "Amount Due", "Due Date", "Stage", "Wire"].map((h, i) => (
                  <th key={h} className={`whitespace-nowrap border-b border-line px-3 py-2 text-[10px] font-semibold uppercase tracking-wider text-ink-faint first:pl-5 last:pr-5 ${
                    i === 4 || i === 5 ? "text-right" : ""}`}>
                    {h}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {visible.map((call) => {
                const isCollapsed = flat ? false : collapsed[call.id];
                return [
                  /* transaction (parent) row */
                  <tr
                    key={call.id}
                    className="cursor-pointer border-b border-line bg-fill-strong/70 hover:bg-fill-strong"
                    onClick={() => router.push(`/capital-calls/${call.id}`)}
                  >
                    <Td className="font-bold text-ink">
                      {!flat && (
                        <button
                          type="button"
                          aria-label={isCollapsed ? `Expand ${call.deal}` : `Collapse ${call.deal}`}
                          onClick={(e) => { e.stopPropagation(); setCollapsed((p) => ({ ...p, [call.id]: !p[call.id] })); }}
                          className="mr-1.5 inline-block w-3.5 text-[10px] text-ink-muted"
                        >
                          {isCollapsed ? "▸" : "▾"}
                        </button>
                      )}
                      {call.deal}
                      <span className="ml-2 rounded-full bg-line px-1.5 py-px text-[9.5px] font-semibold text-ink-muted">
                        {call.allocations.length} investor{call.allocations.length > 1 ? "s" : ""}
                      </span>
                    </Td>
                    <Td>{fundLabel(call.fundId)}</Td>
                    <Td>{call.tranche}</Td>
                    <Td>{call.borrower}</Td>
                    <Td align="right" className="text-ink-faint">—</Td>
                    <Td align="right" className="font-bold text-ink">{money(call.amount)}</Td>
                    <Td><DueDate call={call} /></Td>
                    <Td>
                      <span className="font-semibold text-ink">{stageName(call)}</span>{" "}
                      <Pill className="ml-1">{call.status}</Pill>
                    </Td>
                    <Td><WireSummary allocations={call.allocations} /></Td>
                  </tr>,
                  /* per-investor child rows */
                  ...(isCollapsed ? [] : call.allocations.map((a) => (
                    <tr key={`${call.id}-${a.investorId}`} className="cursor-pointer border-b border-line/60 hover:bg-fill"
                        onClick={() => router.push(`/capital-calls/${call.id}`)}>
                      <Td className="text-ink-secondary">
                        <span className="ml-4 mr-2 inline-block h-px w-3 bg-line-strong align-middle" aria-hidden />
                        {a.investor}
                      </Td>
                      <Td className="text-ink-faint">{fundLabel(call.fundId)}</Td>
                      <Td className="text-ink-faint">·</Td>
                      <Td className="text-ink-faint">·</Td>
                      <Td align="right" className="text-ink-muted">{money(a.commitment)}</Td>
                      <Td align="right" className="font-semibold text-ink">{money(a.amount)}</Td>
                      <Td className="text-ink-muted">{fmtDate(call.dueDate, "short")}</Td>
                      <Td className="text-ink-faint">·</Td>
                      <Td><Pill>{a.wireStatus}</Pill></Td>
                    </tr>
                  ))),
                ];
              })}
            </tbody>
          </table>
        </div>
      )}

      <p className="px-5 py-3 text-[11px] text-ink-faint">
        Click a row for the due-diligence detail. Overdue wires also surface in{" "}
        <Link href="/portfolio" className="text-primary hover:underline">Needs attention</Link>.
      </p>
    </div>
  );
}
