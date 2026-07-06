"use client";

import type { Integration } from "@/lib/types";
import { ScreenHeader } from "@/components/shell/ScreenHeader";
import { Button } from "@/components/ui/Button";
import { DataTable, type Column } from "@/components/ui/DataTable";
import { Kpi, KpiRow } from "@/components/ui/Kpi";
import { Pill } from "@/components/ui/Pill";
import { useToast } from "@/components/ui/Toast";

interface Kpis { connected: number; warnings: number; errors: number; lastSync: string; lastSyncAgo: string }

const columns: Column<Integration>[] = [
  { key: "name", header: "Integration", cellClass: "font-semibold text-ink", sortValue: (i) => i.name },
  { key: "type", header: "Type", render: (i) => <span className="text-ink-muted">{i.type}</span>, sortValue: (i) => i.type },
  { key: "direction", header: "Direction", render: (i) => <span className="text-ink-muted">{i.direction}</span> },
  { key: "lastSync", header: "Last sync", render: (i) => <span className="num text-ink-muted">{i.lastSync}</span>, sortValue: (i) => i.lastSync },
  { key: "status", header: "Status", render: (i) => <Pill>{i.status}</Pill>, sortValue: (i) => i.status },
];

export function IntegrationsScreen({ kpis, integrations }: { kpis: Kpis; integrations: Integration[] }) {
  const toast = useToast();
  const warning = integrations.find((i) => i.status === "Warning" && i.warning);

  return (
    <div>
      <ScreenHeader title={<><span className="font-medium text-ink-faint">Admin /</span> Integrations &amp; Feeds</>}>
        <Button onClick={() => toast.push({ kind: "success", title: "Feeds synced · 09:43" })}>Sync now</Button>
        <Button variant="primary">+ Connect</Button>
      </ScreenHeader>

      <KpiRow>
        <Kpi label="Connected" value={kpis.connected} sub="live feeds" tone="positive" />
        <Kpi label="Warnings" value={kpis.warnings} sub="cert expiring" tone="caution" />
        <Kpi label="Errors" value={kpis.errors} sub="all clear" />
        <Kpi label="Last Sync" value={kpis.lastSync} sub={kpis.lastSyncAgo} />
      </KpiRow>

      <DataTable
        columns={columns}
        rows={integrations}
        rowKey={(i) => i.name}
        rowClass={(i) => (i.status === "Warning" ? "bg-caution-soft/40" : "")}
      />

      {warning && (
        <div className="mx-5 mb-5 mt-4 flex flex-wrap items-center gap-2.5 rounded-lg border border-caution-line bg-caution-soft px-3.5 py-2.5 text-xs text-caution">
          <span><b>⚠ {warning.name}</b> — {warning.warning}</span>
          <Button
            size="sm"
            className="ml-auto border-caution-line bg-card text-caution hover:border-caution"
            onClick={() => toast.push({ kind: "success", title: "Certificate rotation started" })}
          >
            Rotate
          </Button>
        </div>
      )}
    </div>
  );
}
