import type { Metadata } from "next";
import { getDeals, getNeedsAttention, getPortfolioSummary } from "@/lib/data";
import { PortfolioScreen } from "@/screens/PortfolioScreen";

export const metadata: Metadata = { title: "Portfolio" };

/** Screen 6a — default home: whole-book health + per-deal grid + ops inbox. */
export default async function PortfolioPage() {
  const [summary, deals, attention] = await Promise.all([getPortfolioSummary(), getDeals(), getNeedsAttention()]);
  return <PortfolioScreen summary={summary} deals={deals} attention={attention} />;
}
