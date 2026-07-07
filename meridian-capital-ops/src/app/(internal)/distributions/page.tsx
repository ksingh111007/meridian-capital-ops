import type { Metadata } from "next";
import { getDistributions } from "@/lib/data";
import { DistributionsScreen } from "@/screens/DistributionsScreen";

export const metadata: Metadata = { title: "Distributions" };

/** Screen 4a — allocation summary over the waterfall calculation ledger. */
export default async function DistributionsPage() {
  return <DistributionsScreen distributions={await getDistributions()} />;
}
