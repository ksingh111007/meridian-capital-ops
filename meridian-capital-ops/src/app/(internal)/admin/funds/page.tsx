import type { Metadata } from "next";
import { getFunds } from "@/lib/data";
import { FundsScreen } from "@/screens/admin/FundsScreen";

export const metadata: Metadata = { title: "Funds & Entities" };

/** Screen 5c — fund setup, legal entities, share classes & waterfall terms. */
export default function FundsPage() {
  const { kpis, funds, entities, shareClasses } = getFunds();
  return <FundsScreen kpis={kpis} funds={funds} entities={entities} shareClasses={shareClasses} />;
}
