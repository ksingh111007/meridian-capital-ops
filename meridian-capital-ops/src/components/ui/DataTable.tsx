"use client";

import { useMemo, useState } from "react";

export interface Column<T> {
  key: string;
  header: React.ReactNode;
  align?: "left" | "right" | "center";
  /** Custom cell renderer; defaults to String(row[key]). */
  render?: (row: T) => React.ReactNode;
  /** Value used for sorting; enables the sort affordance on this column. */
  sortValue?: (row: T) => string | number;
  /** Extra classes on every cell in this column (e.g. "num"). */
  cellClass?: string;
  headerClass?: string;
}

/**
 * The workhorse table: compact, sticky header, optional client-side sorting,
 * row click-through, tone-highlighted rows, and a totals row.
 * Use inside client components only (column renderers are functions).
 */
export function DataTable<T>({
  columns, rows, rowKey, onRowClick, rowClass, totalRow, emptyState,
}: {
  columns: Column<T>[];
  rows: T[];
  rowKey: (row: T) => string;
  onRowClick?: (row: T) => void;
  /** e.g. highlight exception rows: (row) => row.status === "Exception" ? "bg-danger-soft/40" : "" */
  rowClass?: (row: T) => string;
  totalRow?: React.ReactNode; // full <tr> content (use <Td>s)
  emptyState?: React.ReactNode;
}) {
  const [sort, setSort] = useState<{ key: string; dir: 1 | -1 } | null>(null);

  const sorted = useMemo(() => {
    if (!sort) return rows;
    const col = columns.find((c) => c.key === sort.key);
    if (!col?.sortValue) return rows;
    return [...rows].sort((a, b) => {
      const av = col.sortValue!(a), bv = col.sortValue!(b);
      return (av < bv ? -1 : av > bv ? 1 : 0) * sort.dir;
    });
  }, [rows, sort, columns]);

  if (rows.length === 0 && emptyState) return <>{emptyState}</>;

  return (
    <div className="overflow-x-auto">
      <table className="w-full border-collapse text-left">
        <thead>
          <tr>
            {columns.map((c) => {
              const sortable = !!c.sortValue;
              const active = sort?.key === c.key;
              return (
                <th
                  key={c.key}
                  aria-sort={active ? (sort!.dir === 1 ? "ascending" : "descending") : undefined}
                  className={`sticky top-0 z-10 whitespace-nowrap border-b border-line bg-card px-3 py-2 text-[10px] font-semibold uppercase tracking-wider text-ink-faint first:pl-5 last:pr-5 ${
                    c.align === "right" ? "text-right" : c.align === "center" ? "text-center" : ""
                  } ${sortable ? "cursor-pointer select-none hover:text-ink-secondary" : ""} ${c.headerClass ?? ""}`}
                  onClick={sortable ? () => setSort((s) => (s?.key === c.key ? { key: c.key, dir: s.dir === 1 ? -1 : 1 } : { key: c.key, dir: 1 })) : undefined}
                >
                  {c.header}
                  {sortable && <span className={`ml-1 ${active ? "text-primary" : "text-line-strong"}`}>{active && sort!.dir === -1 ? "↓" : "↑"}</span>}
                </th>
              );
            })}
          </tr>
        </thead>
        <tbody>
          {sorted.map((row) => (
            <tr
              key={rowKey(row)}
              className={`border-b border-line/60 last:border-0 ${onRowClick ? "cursor-pointer" : ""} hover:bg-fill ${rowClass?.(row) ?? ""}`}
              onClick={onRowClick ? () => onRowClick(row) : undefined}
            >
              {columns.map((c) => (
                <td
                  key={c.key}
                  className={`whitespace-nowrap px-3 py-2 text-[12.5px] text-ink-secondary first:pl-5 last:pr-5 ${
                    c.align === "right" ? "num text-right" : c.align === "center" ? "text-center" : ""
                  } ${c.cellClass ?? ""}`}
                >
                  {c.render ? c.render(row) : String((row as Record<string, unknown>)[c.key] ?? "")}
                </td>
              ))}
            </tr>
          ))}
          {totalRow && <tr className="bg-fill-strong font-semibold text-ink">{totalRow}</tr>}
        </tbody>
      </table>
    </div>
  );
}

/** Cell helper for hand-built rows (totals, grouped blotter rows). */
export function Td({ align, className = "", colSpan, children }: { align?: "right" | "center"; className?: string; colSpan?: number; children?: React.ReactNode }) {
  return (
    <td colSpan={colSpan} className={`whitespace-nowrap px-3 py-2 text-[12.5px] first:pl-5 last:pr-5 ${align === "right" ? "num text-right" : align === "center" ? "text-center" : ""} ${className}`}>
      {children}
    </td>
  );
}
