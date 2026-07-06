import type { Metadata } from "next";
import { getAuditLog } from "@/lib/data";
import { AuditLogScreen } from "@/screens/admin/AuditLogScreen";

export const metadata: Metadata = { title: "Audit Log" };

/** Screen 5h — append-only, hash-chained record of every action. */
export default function AuditLogPage() {
  const { kpis, events } = getAuditLog();
  return <AuditLogScreen kpis={kpis} events={events} />;
}
