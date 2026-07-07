import type { Metadata } from "next";
import { getPortalActivity } from "@/lib/data";
import { PortalActivityScreen } from "@/screens/portal/PortalActivityScreen";

export const metadata: Metadata = { title: "Capital Activity · Investor Portal" };

/** Screen 6g — full ledger of calls & distributions. */
export default async function PortalActivityPage() {
  const { stats, rows } = await getPortalActivity();
  return <PortalActivityScreen stats={stats} rows={rows} />;
}
