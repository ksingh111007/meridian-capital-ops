import type { Metadata } from "next";
import { getReconciliation } from "@/lib/data";
import { ReconciliationScreen } from "@/screens/ReconciliationScreen";

export const metadata: Metadata = { title: "Reconciliation" };

/** Screen 4e — book vs custodian, breaks surfaced for clearing. */
export default function ReconciliationPage() {
  const { source, kpis, items } = getReconciliation();
  return <ReconciliationScreen source={source} kpis={kpis} items={items} />;
}
