import type { Metadata } from "next";
import { getPortalIrInfo } from "@/lib/data";
import { PortalContactScreen } from "@/screens/portal/PortalContactScreen";

export const metadata: Metadata = { title: "Contact IR · Investor Portal" };

/** Screen 6i — message the IR team + relationship contacts. */
export default function PortalContactPage() {
  return <PortalContactScreen info={getPortalIrInfo()} />;
}
