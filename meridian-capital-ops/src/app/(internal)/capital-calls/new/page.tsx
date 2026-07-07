import type { Metadata } from "next";
import { getDeals, getFunds, getInvestors } from "@/lib/data";
import { NewCallWizard } from "@/screens/NewCallWizard";

export const metadata: Metadata = { title: "New Capital Call" };

/** Screen 2c — the 5-step call creation wizard. */
export default async function NewCallPage() {
  const [deals, funds, investors] = await Promise.all([getDeals(), getFunds(), getInvestors()]);
  return <NewCallWizard deals={deals} funds={funds.funds} investors={investors.investors} />;
}
