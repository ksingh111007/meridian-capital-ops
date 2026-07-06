"use client";

import { fmtDate, money, multiple, pct } from "@/lib/format";
import { Card } from "@/components/ui/primitives";
import { DataTable, Td, type Column } from "@/components/ui/DataTable";
import { PortalPageHeader, PortalSection } from "./shared";

export interface PortalPosition {
  fund: string;
  vintage: number;
  commitment: number;
  paidIn: number;
  distributions: number;
  nav: number;
  netIrrPct: number;
  tvpi: number;
}

export interface PortalRollforwardLine {
  line: string;
  fundIII: number | null;
  fundII: number | null;
  total: number;
  kind: string; // "start" | "positive" | "negative" | "end"
}

export interface PortalInvestments {
  asOf: string;
  positions: PortalPosition[];
  totals: { commitment: number; paidIn: number; distributions: number; nav: number; netIrrPct: number; tvpi: number };
  rollforward: { period: string; lines: PortalRollforwardLine[] };
}

const m = (v: number) => money(v, { decimals: 1 });

const positionColumns: Column<PortalPosition>[] = [
  { key: "fund", header: "Fund", cellClass: "font-semibold text-ink" },
  { key: "vintage", header: "Vintage", render: (p) => <span className="text-ink-muted">{p.vintage}</span> },
  { key: "commitment", header: "Commitment", align: "right", render: (p) => m(p.commitment) },
  { key: "paidIn", header: "Paid-in", align: "right", render: (p) => m(p.paidIn) },
  { key: "distributions", header: "Distributions", align: "right", render: (p) => <span className="text-positive">{m(p.distributions)}</span> },
  { key: "nav", header: "NAV", align: "right", render: (p) => <span className="font-semibold text-primary">{m(p.nav)}</span> },
  { key: "netIrrPct", header: "Net IRR", align: "right", render: (p) => pct(p.netIrrPct) },
  { key: "tvpi", header: "TVPI", align: "right", render: (p) => multiple(p.tvpi) },
];

/** One rollforward money cell; sign/color derive from the line's kind. */
function rollCell(value: number | null, kind: string, strong = false) {
  if (value === null) return <span className="text-ink-muted">—</span>;
  if (kind === "end") return <span className="font-bold text-primary">{m(value)}</span>;
  const cls = kind === "positive" ? "text-positive" : "text-ink";
  return <span className={`${strong ? "font-semibold" : ""} ${cls}`}>{money(value, { sign: kind === "positive", decimals: 1 })}</span>;
}

const rollforwardColumns: Column<PortalRollforwardLine>[] = [
  { key: "line", header: "Line", render: (l) => <span className={l.kind === "end" ? "font-semibold text-ink" : "text-ink-secondary"}>{l.line}</span> },
  { key: "fundIII", header: "Fund III", align: "right", render: (l) => rollCell(l.fundIII, l.kind) },
  { key: "fundII", header: "Fund II", align: "right", render: (l) => rollCell(l.fundII, l.kind) },
  { key: "total", header: "Total", align: "right", render: (l) => rollCell(l.total, l.kind, true) },
];

/** Screen 6e — positions across funds + the quarter's capital-account rollforward. */
export function PortalInvestmentsScreen({ investments }: { investments: PortalInvestments }) {
  const { positions, totals, rollforward } = investments;

  return (
    <div>
      <PortalPageHeader
        title="My investments"
        subtitle={`Positions across Meridian Credit funds · as of ${fmtDate(investments.asOf)}`}
      />

      <PortalSection>Positions by fund</PortalSection>
      <Card className="overflow-hidden">
        <DataTable
          columns={positionColumns}
          rows={positions}
          rowKey={(p) => p.fund}
          totalRow={
            <>
              <Td className="font-semibold text-ink">Total</Td>
              <Td>{null}</Td>
              <Td align="right">{m(totals.commitment)}</Td>
              <Td align="right">{m(totals.paidIn)}</Td>
              <Td align="right" className="text-positive">{m(totals.distributions)}</Td>
              <Td align="right" className="text-primary">{m(totals.nav)}</Td>
              <Td align="right">{pct(totals.netIrrPct)}</Td>
              <Td align="right">{multiple(totals.tvpi)}</Td>
            </>
          }
        />
      </Card>

      <PortalSection>Capital account rollforward · {rollforward.period}</PortalSection>
      <Card className="overflow-hidden">
        <DataTable
          columns={rollforwardColumns}
          rows={rollforward.lines}
          rowKey={(l) => l.line}
          rowClass={(l) => (l.kind === "end" ? "bg-fill-strong" : "")}
        />
      </Card>
    </div>
  );
}
