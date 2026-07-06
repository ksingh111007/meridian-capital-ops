import type { Metadata } from "next";
import { getCapitalCalls, getWorkflow } from "@/lib/data";
import { CapitalCallsScreen } from "@/screens/CapitalCallsScreen";

export const metadata: Metadata = { title: "Capital Calls" };

/** Screen 2a — the blotter, grouped by transaction with per-investor child rows. */
export default function CapitalCallsPage() {
  return <CapitalCallsScreen calls={getCapitalCalls()} stages={getWorkflow().stages} />;
}
