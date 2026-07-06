import type { Metadata } from "next";
import { getPortalAccount, getPortalActivity, getPortalStatements } from "@/lib/data";
import { PortalOverviewScreen } from "@/screens/portal/PortalOverviewScreen";

export const metadata: Metadata = { title: "Overview · Investor Portal" };

/** Screen 6b — external LP view: positions + statement downloads. */
export default function PortalOverviewPage() {
  return (
    <PortalOverviewScreen
      account={getPortalAccount()}
      activity={getPortalActivity().rows}
      documents={getPortalStatements().documents}
    />
  );
}
