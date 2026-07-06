import type { Metadata } from "next";
import { getIntegrations } from "@/lib/data";
import { IntegrationsScreen } from "@/screens/admin/IntegrationsScreen";

export const metadata: Metadata = { title: "Integrations" };

/** Screen 5f — custodian / bank / GL / market-data connections. */
export default function IntegrationsPage() {
  const { kpis, integrations } = getIntegrations();
  return <IntegrationsScreen kpis={kpis} integrations={integrations} />;
}
