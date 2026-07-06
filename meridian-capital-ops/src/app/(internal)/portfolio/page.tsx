import type { Metadata } from "next";
import { getDeals, getNeedsAttention, getPortfolioSummary } from "@/lib/data";
import { PortfolioScreen } from "@/screens/PortfolioScreen";

export const metadata: Metadata = { title: "Portfolio" };

/** Screen 6a — default home: whole-book health + per-deal grid + ops inbox. */
export default function PortfolioPage() {
  return <PortfolioScreen summary={getPortfolioSummary()} deals={getDeals()} attention={getNeedsAttention()} />;
}
