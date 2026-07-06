import type { Metadata } from "next";
import { getDrawdowns } from "@/lib/data";
import { DrawdownsScreen } from "@/screens/DrawdownsScreen";

export const metadata: Metadata = { title: "Drawdowns" };

/** Screen 4b — draws on the fund's credit facilities (bridge to capital calls). */
export default function DrawdownsPage() {
  const { kpis, drawdowns } = getDrawdowns();
  return <DrawdownsScreen kpis={kpis} drawdowns={drawdowns} />;
}
