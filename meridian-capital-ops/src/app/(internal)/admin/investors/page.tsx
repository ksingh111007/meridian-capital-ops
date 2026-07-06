import type { Metadata } from "next";
import { getInvestors } from "@/lib/data";
import { InvestorRegistryScreen } from "@/screens/admin/InvestorRegistryScreen";

export const metadata: Metadata = { title: "Investor Registry" };

/** Screen 5d — LP master: commitments, wire instructions, KYC. */
export default function InvestorRegistryPage() {
  const { kpis, investors } = getInvestors();
  return <InvestorRegistryScreen kpis={kpis} investors={investors} />;
}
