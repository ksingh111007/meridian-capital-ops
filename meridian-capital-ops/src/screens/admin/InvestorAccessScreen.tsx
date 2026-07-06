"use client";

import Link from "next/link";
import { useState } from "react";
import type { Investor, InvestorAccessConfig, PortalContact } from "@/lib/types";
import { ScreenHeader } from "@/components/shell/ScreenHeader";
import { Button } from "@/components/ui/Button";
import { DataTable, type Column } from "@/components/ui/DataTable";
import { Kpi, KpiRow } from "@/components/ui/Kpi";
import { Modal } from "@/components/ui/Modal";
import { Pill } from "@/components/ui/Pill";
import { useToast } from "@/components/ui/Toast";
import { Field, Select, TextInput, Toggle } from "@/components/ui/controls";
import { Avatar, SectionTitle } from "@/components/ui/primitives";

const PORTAL_ROLES = ["Primary", "Viewer", "Tax-only"];
const FUND_OPTIONS = ["All funds", "Fund III only", "Fund II only", "Fund II & III"];

function statementsCell(c: PortalContact) {
  if (c.statements === "full") return <span className="font-bold text-positive">✓</span>;
  if (c.statements === "tax") return <span className="text-ink-muted">Tax only</span>;
  return <span className="text-ink-muted">—</span>;
}

export function InvestorAccessScreen({ config, investors }: { config: InvestorAccessConfig; investors: Investor[] }) {
  const toast = useToast();
  const { kpis, contacts } = config;
  const [capabilities, setCapabilities] = useState(config.capabilities);
  const [documentTypes, setDocumentTypes] = useState(config.documentTypes);

  // invite dialog
  const [inviteOpen, setInviteOpen] = useState(false);
  const [email, setEmail] = useState("");
  const [investor, setInvestor] = useState(investors[0]?.name ?? "");
  const [role, setRole] = useState(PORTAL_ROLES[0]);
  const [funds, setFunds] = useState(FUND_OPTIONS[0]);

  function sendInvite() {
    toast.push({ kind: "success", title: `Invite sent · ${email}`, detail: `${investor} · ${role} · ${funds}` });
    setInviteOpen(false);
    setEmail("");
  }

  function toggleCapability(label: string, on: boolean) {
    setCapabilities((prev) => prev.map((c) => (c.label === label ? { ...c, enabled: on } : c)));
    toast.push({ kind: "success", title: `Capability ${on ? "enabled" : "disabled"}`, detail: label });
  }

  function toggleDocument(label: string) {
    const next = !documentTypes.find((d) => d.label === label)?.exposed;
    setDocumentTypes((prev) => prev.map((d) => (d.label === label ? { ...d, exposed: next } : d)));
    toast.push({ kind: "success", title: `Document type ${next ? "exposed" : "hidden"}`, detail: label });
  }

  const columns: Column<PortalContact>[] = [
    {
      key: "contact", header: "Contact", sortValue: (c) => c.name,
      render: (c) => (
        <span className="inline-flex items-center gap-2">
          <Avatar initials={c.initials} muted={c.status !== "Active"} />
          <span className={c.status === "Active" ? "font-semibold text-ink" : "font-semibold text-ink-faint"}>{c.name}</span>
        </span>
      ),
    },
    { key: "investor", header: "Investor", sortValue: (c) => c.investor },
    { key: "role", header: "Role", render: (c) => <Pill>{c.role}</Pill>, sortValue: (c) => c.role },
    { key: "fundsVisible", header: "Funds visible", render: (c) => <span className="text-ink-muted">{c.fundsVisible}</span> },
    { key: "statements", header: "Statements", render: statementsCell, sortValue: (c) => c.statements },
    { key: "status", header: "Status", render: (c) => <Pill>{c.status}</Pill>, sortValue: (c) => c.status },
  ];

  return (
    <div>
      <ScreenHeader title={<><span className="font-medium text-ink-faint">Admin /</span> Investor Access</>}>
        <Link
          href="/portal"
          className="inline-flex items-center gap-1.5 whitespace-nowrap rounded-md border border-line-strong bg-card px-3 py-1.5 text-xs font-semibold text-ink transition-colors hover:border-ink"
        >
          Preview portal →
        </Link>
        <Button variant="primary" onClick={() => setInviteOpen(true)}>+ Invite investor</Button>
      </ScreenHeader>

      <KpiRow>
        <Kpi label="Portal Users" value={kpis.portalUsers} sub="external contacts" />
        <Kpi label="Active" value={kpis.active} sub="can sign in" />
        <Kpi label="Pending Invites" value={kpis.pendingInvites} sub="not yet accepted" tone="primary" />
        <Kpi label="Investors w/ Access" value={kpis.investorsWithAccess} sub={`${kpis.notEnrolled} not enrolled`} />
      </KpiRow>

      <DataTable columns={columns} rows={contacts} rowKey={(c) => c.id} />

      <div className="flex flex-col border-t border-line sm:flex-row">
        <div className="flex-1 border-b border-dashed border-line pb-4 sm:border-b-0 sm:border-r">
          <SectionTitle>Default portal capabilities</SectionTitle>
          <div className="flex flex-col gap-2 px-5 pt-1">
            {capabilities.map((c) => (
              <label key={c.label} className={`flex items-center gap-2.5 text-xs ${c.enabled ? "text-ink" : "text-ink-muted"}`}>
                <Toggle on={c.enabled} onChange={(on) => toggleCapability(c.label, on)} label={c.label} />
                {c.label}
              </label>
            ))}
          </div>
        </div>
        <div className="flex-1 pb-4">
          <SectionTitle>Documents exposed to investors</SectionTitle>
          <div className="flex flex-col gap-2 px-5 pt-1">
            {documentTypes.map((d) => (
              <button
                key={d.label}
                type="button"
                aria-pressed={d.exposed}
                onClick={() => toggleDocument(d.label)}
                className={`flex items-center gap-2.5 text-left text-xs ${d.exposed ? "text-ink" : "text-ink-muted"}`}
              >
                {d.exposed
                  ? <span aria-hidden className="flex h-4 w-4 flex-none items-center justify-center rounded bg-primary text-[10px] font-bold text-white">✓</span>
                  : <span aria-hidden className="h-4 w-4 flex-none rounded border border-line-strong" />}
                {d.label}
              </button>
            ))}
          </div>
        </div>
      </div>

      {/* Invite investor dialog */}
      <Modal
        open={inviteOpen}
        onClose={() => setInviteOpen(false)}
        title="Invite investor"
        footer={
          <>
            <Button onClick={() => setInviteOpen(false)}>Cancel</Button>
            <Button variant="primary" disabled={!email.includes("@")} onClick={sendInvite}>Send invite</Button>
          </>
        }
      >
        <div className="flex flex-col gap-3.5">
          <Field label="Email">
            <TextInput placeholder="name@investor.com" value={email} onChange={(e) => setEmail(e.target.value)} />
          </Field>
          <Field label="Investor">
            <Select options={investors.map((i) => i.name)} value={investor} onChange={(e) => setInvestor(e.target.value)} />
          </Field>
          <Field label="Role">
            <Select options={PORTAL_ROLES} value={role} onChange={(e) => setRole(e.target.value)} />
          </Field>
          <Field label="Funds visible">
            <Select options={FUND_OPTIONS} value={funds} onChange={(e) => setFunds(e.target.value)} />
          </Field>
        </div>
      </Modal>
    </div>
  );
}
