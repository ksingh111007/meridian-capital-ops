/** Formatting helpers. Amounts are USD millions throughout the mock data. */

/** $16.00M / $1.20B / −$8.20M */
export function money(millions: number, opts?: { sign?: boolean; decimals?: number }): string {
  const sign = millions < 0 ? "−" : opts?.sign && millions > 0 ? "+" : "";
  const abs = Math.abs(millions);
  if (abs >= 1000) return `${sign}$${(abs / 1000).toFixed(2)}B`;
  const decimals = opts?.decimals ?? (abs >= 100 ? 1 : 2);
  return `${sign}$${abs.toFixed(decimals)}M`;
}

export function pct(value: number, decimals = 1): string {
  return `${value.toFixed(decimals)}%`;
}

export function multiple(value: number): string {
  return `${value.toFixed(2)}×`;
}

/** "2026-07-08" → "Jul 08 2026"; short → "Jul 08" */
export function fmtDate(iso: string, style: "short" | "long" = "long"): string {
  const d = new Date(`${iso}T00:00:00Z`);
  const s = d.toLocaleDateString("en-US", { month: "short", day: "2-digit", timeZone: "UTC" });
  return style === "short" ? s : `${s} ${d.getUTCFullYear()}`;
}

/** Mock "today" — matches the sample data's as-of date. */
export const TODAY = "2026-07-05";

export function daysBetween(fromIso: string, toIso: string): number {
  return Math.round((Date.parse(`${toIso}T00:00:00Z`) - Date.parse(`${fromIso}T00:00:00Z`)) / 86_400_000);
}

/** Days until due (negative = overdue) relative to mock today. */
export function daysUntil(dueIso: string): number {
  return daysBetween(TODAY, dueIso);
}
