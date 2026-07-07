import type { Metadata } from "next";
import { getWires } from "@/lib/data";
import { WiresScreen } from "@/screens/WiresScreen";

export const metadata: Metadata = { title: "Wire Status" };

/** Screen 4c — every inbound / outbound movement and where it is in settlement. */
export default async function WiresPage() {
  const { asOf, wires } = await getWires();
  return <WiresScreen asOf={asOf} wires={wires} />;
}
