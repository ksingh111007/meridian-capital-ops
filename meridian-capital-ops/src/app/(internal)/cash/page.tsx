import type { Metadata } from "next";
import { getCashPosition } from "@/lib/data";
import { CashPositionScreen } from "@/screens/CashPositionScreen";

export const metadata: Metadata = { title: "Cash Position" };

/** Screen 4d — liquidity now, forecast, and per-account balances. */
export default async function CashPositionPage() {
  return <CashPositionScreen cash={await getCashPosition()} />;
}
