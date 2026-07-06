import type { Metadata } from "next";
import { getInvestorAccess, getInvestors } from "@/lib/data";
import { InvestorAccessScreen } from "@/screens/admin/InvestorAccessScreen";

export const metadata: Metadata = { title: "Investor Access" };

/** Screen 6c — who gets the portal, and what they can see. */
export default function InvestorAccessPage() {
  return <InvestorAccessScreen config={getInvestorAccess()} investors={getInvestors().investors} />;
}
