import type { Metadata } from "next";
import { getDeals, getFunds, getInvestors } from "@/lib/data";
import { NewCallWizard } from "@/screens/NewCallWizard";

export const metadata: Metadata = { title: "New Capital Call" };

/** Screen 2c — the 5-step call creation wizard. */
export default function NewCallPage() {
  return <NewCallWizard deals={getDeals()} funds={getFunds().funds} investors={getInvestors().investors} />;
}
