import type { Metadata } from "next";
import { getPortalStatements } from "@/lib/data";
import { PortalStatementsScreen } from "@/screens/portal/PortalStatementsScreen";

export const metadata: Metadata = { title: "Statements · Investor Portal" };

/** Screen 6f — filterable document library with downloads. */
export default function PortalStatementsPage() {
  const { totalCount, documents } = getPortalStatements();
  return <PortalStatementsScreen totalCount={totalCount} documents={documents} />;
}
