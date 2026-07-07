import type { Metadata } from "next";
import { getPortalAccount, getPortalActivity, getPortalStatements } from "@/lib/data";
import { PortalOverviewScreen } from "@/screens/portal/PortalOverviewScreen";

export const metadata: Metadata = { title: "Overview · Investor Portal" };

/** Screen 6b — external LP view: positions + statement downloads. */
export default async function PortalOverviewPage() {
  const [account, activity, statements] = await Promise.all([
    getPortalAccount(), getPortalActivity(), getPortalStatements(),
  ]);
  return <PortalOverviewScreen account={account} activity={activity.rows} documents={statements.documents} />;
}
