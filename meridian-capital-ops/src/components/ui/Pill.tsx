import type { PillTone } from "@/lib/types";

const TONES: Record<PillTone, string> = {
  neutral: "text-ink-secondary border-line-strong bg-fill-strong",
  amber: "text-caution border-caution-line bg-caution-soft",
  green: "text-positive border-positive-line bg-positive-soft",
  blue: "text-primary border-primary-line bg-primary-soft",
  red: "text-danger border-danger-line bg-danger-soft",
};

/** Map every domain status string to its pill tone (single source of truth). */
export function statusTone(status: string): PillTone {
  switch (status) {
    // green — money landed / healthy / done
    case "Wired": case "Confirmed": case "Settled": case "Paid": case "Funded":
    case "Completed": case "Performing": case "Active": case "Matched":
    case "Verified": case "On file": case "Connected": case "Resolved": case "Available":
      return "green";
    // blue — in an active review / in flight
    case "In Review": case "Sent": case "Acknowledged": case "Outstanding":
    case "Investing": case "Primary": case "Processing": case "Paying":
      return "blue";
    // amber — needs a human
    case "Pending": case "Returned": case "In review": case "Warning":
    case "Watch": case "Invited": case "Tax-only": case "Unmatched": case "Due":
      return "amber";
    // red — broken
    case "Overdue": case "Exception": case "Break": case "Failed":
    case "Non-accrual": case "Missing": case "Error": case "Blocked":
      return "red";
    default:
      return "neutral"; // Scheduled, Queued, Harvesting, Viewer, Disabled, …
  }
}

export function Pill({ tone, children, className = "" }: { tone?: PillTone; children: React.ReactNode; className?: string }) {
  const resolved = tone ?? (typeof children === "string" ? statusTone(children) : "neutral");
  return (
    <span className={`inline-block whitespace-nowrap rounded-full border px-2 py-px text-[10.5px] font-semibold leading-4 ${TONES[resolved]} ${className}`}>
      {children}
    </span>
  );
}
