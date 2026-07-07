import type { Metadata } from "next";
import { getPortalInvestments } from "@/lib/data";
import { PortalInvestmentsScreen } from "@/screens/portal/PortalInvestmentsScreen";

export const metadata: Metadata = { title: "My Investments · Investor Portal" };

/** Screen 6e — positions by fund + capital-account rollforward. */
export default async function PortalInvestmentsPage() {
  return <PortalInvestmentsScreen investments={await getPortalInvestments()} />;
}
