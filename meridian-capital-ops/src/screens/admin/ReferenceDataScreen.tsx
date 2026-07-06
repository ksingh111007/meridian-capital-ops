"use client";

import { useState } from "react";
import type { PillTone } from "@/lib/types";
import { ScreenHeader } from "@/components/shell/ScreenHeader";
import { Button } from "@/components/ui/Button";
import { DataTable, type Column } from "@/components/ui/DataTable";
import { Pill } from "@/components/ui/Pill";
import { SearchInput, Tabs } from "@/components/ui/controls";
import { Card } from "@/components/ui/primitives";
import { EmptyState, ZeroResults } from "@/components/ui/states";

interface Borrower { name: string; sector: string; country: string; deal: string; internalRating: string }
interface Currency { code: string; rate: number; note: string }
interface Calendar { name: string; nextHoliday: string }

const TABS = ["Borrowers", "Deals", "Tranches", "Currencies", "Calendars"];

/** CCC… → red, single-B / B− → amber, everything better → blue. */
function ratingTone(rating: string): PillTone {
  if (rating.startsWith("CCC")) return "red";
  if (rating === "B" || rating === "B−") return "amber";
  return "blue";
}

export function ReferenceDataScreen({
  borrowers, currencies, currenciesUpdated, calendars,
}: {
  borrowers: Borrower[];
  currencies: Currency[];
  currenciesUpdated: string;
  calendars: Calendar[];
}) {
  const [tab, setTab] = useState(TABS[0]);
  const [query, setQuery] = useState("");

  const q = query.trim().toLowerCase();
  const filtered = borrowers.filter(
    (b) => !q || b.name.toLowerCase().includes(q) || b.sector.toLowerCase().includes(q) || b.deal.toLowerCase().includes(q),
  );

  const columns: Column<Borrower>[] = [
    { key: "name", header: "Borrower", cellClass: "font-semibold text-ink", sortValue: (b) => b.name },
    { key: "sector", header: "Sector", render: (b) => <span className="text-ink-muted">{b.sector}</span>, sortValue: (b) => b.sector },
    { key: "country", header: "Country", render: (b) => <span className="text-ink-muted">{b.country}</span> },
    { key: "deal", header: "Deal", sortValue: (b) => b.deal },
    { key: "rating", header: "Internal rating", render: (b) => <Pill tone={ratingTone(b.internalRating)}>{b.internalRating}</Pill>, sortValue: (b) => b.internalRating },
  ];

  const currenciesCard = (
    <Card className="flex-1 px-3.5 py-3">
      <h3 className="text-[10px] font-semibold uppercase tracking-wider text-ink-faint">Currencies</h3>
      <div className="mt-2 flex flex-col gap-1.5 text-xs">
        {currencies.map((c) => (
          <div key={c.code} className="flex items-center justify-between gap-3">
            <span className="font-semibold text-ink">{c.code}</span>
            <span className="num text-ink-muted">{c.rate.toFixed(4)}{c.note ? ` · ${c.note}` : ""}</span>
          </div>
        ))}
        <div className="mt-0.5 text-[10px] text-ink-faint">{currenciesUpdated}</div>
      </div>
    </Card>
  );

  const calendarsCard = (
    <Card className="flex-1 px-3.5 py-3">
      <h3 className="text-[10px] font-semibold uppercase tracking-wider text-ink-faint">Settlement calendars</h3>
      <div className="mt-2 flex flex-col gap-1.5 text-xs">
        {calendars.map((c) => (
          <div key={c.name} className="flex items-center justify-between gap-3">
            <span className="font-semibold text-ink">{c.name}</span>
            <span className="text-ink-muted">next: {c.nextHoliday}</span>
          </div>
        ))}
        <div className="mt-0.5 text-[10px] text-ink-faint">drives due-date &amp; wire scheduling</div>
      </div>
    </Card>
  );

  return (
    <div>
      <ScreenHeader title={<><span className="font-medium text-ink-faint">Admin /</span> Reference Data</>}>
        <SearchInput value={query} onChange={setQuery} placeholder="Search borrowers…" />
        <Button variant="primary">+ New borrower</Button>
      </ScreenHeader>

      <Tabs tabs={TABS} active={tab} onChange={setTab} />

      {tab === "Borrowers" && (
        <>
          <DataTable columns={columns} rows={filtered} rowKey={(b) => b.name} emptyState={<ZeroResults onClear={() => setQuery("")} />} />
          <div className="flex flex-col gap-3 border-t border-line bg-fill px-5 py-4 sm:flex-row">
            {currenciesCard}
            {calendarsCard}
          </div>
        </>
      )}

      {(tab === "Deals" || tab === "Tranches") && (
        <EmptyState
          title="Maintained in this mock via Deals"
          message="Deal & tranche reference lives on the deal records; a dedicated editor follows the same table contract."
        />
      )}

      {tab === "Currencies" && (
        <div className="flex flex-col gap-3 px-5 py-4 sm:flex-row">
          {currenciesCard}
          {calendarsCard}
        </div>
      )}

      {tab === "Calendars" && <div className="flex max-w-md px-5 py-4">{calendarsCard}</div>}
    </div>
  );
}
