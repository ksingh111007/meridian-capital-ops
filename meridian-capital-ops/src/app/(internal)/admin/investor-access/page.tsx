import type { Metadata } from "next";
import { getInvestorAccess, getInvestors } from "@/lib/data";
import { InvestorAccessScreen } from "@/screens/admin/InvestorAccessScreen";

export const metadata: Metadata = { title: "Investor Access" };

/** Screen 6c — who gets the portal, and what they can see. */
export default async function InvestorAccessPage() {
  const [config, investors] = await Promise.all([getInvestorAccess(), getInvestors()]);
  return <InvestorAccessScreen config={config} investors={investors.investors} />;
}
