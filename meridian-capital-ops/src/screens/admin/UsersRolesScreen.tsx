"use client";

import { useState } from "react";
import type { Capability, Module, Role, StaffUser } from "@/lib/types";
import { MODULES } from "@/lib/types";
import { ScreenHeader } from "@/components/shell/ScreenHeader";
import { Button } from "@/components/ui/Button";
import { DataTable, type Column } from "@/components/ui/DataTable";
import { Kpi, KpiRow } from "@/components/ui/Kpi";
import { Modal } from "@/components/ui/Modal";
import { Pill } from "@/components/ui/Pill";
import { useToast } from "@/components/ui/Toast";
import { Field, SearchInput, SegmentPicker, Select, Tabs, TextInput } from "@/components/ui/controls";
import { Avatar, Card, SectionTitle } from "@/components/ui/primitives";
import { ZeroResults } from "@/components/ui/states";

const TABS = ["Users", "Roles", "Permissions"];

const CAP_META: Record<Capability, { label: string; cls: string }> = {
  view: { label: "V", cls: "bg-fill-strong text-ink-secondary" },
  edit: { label: "E", cls: "bg-primary-soft text-primary" },
  approve: { label: "A", cls: "bg-positive-soft text-positive" },
  full: { label: "✓", cls: "bg-caution-soft text-caution" },
  none: { label: "—", cls: "text-ink-faint" },
};

/** Small mono badge for a role capability (V / E / A / ✓ / —). */
function CapabilityBadge({ cap }: { cap: Capability }) {
  const { label, cls } = CAP_META[cap];
  return <span className={`inline-flex h-5 w-6 items-center justify-center rounded font-mono text-[10px] font-bold ${cls}`}>{label}</span>;
}

const CAP_TO_SEG: Record<Capability, string> = { none: "—", view: "V", edit: "E", approve: "A", full: "✓" };

interface Kpis { users: number; active: number; pendingInvites: number; roles: number }

export function UsersRolesScreen({ kpis, users, roles }: { kpis: Kpis; users: StaffUser[]; roles: Role[] }) {
  const toast = useToast();
  const [tab, setTab] = useState(TABS[0]);
  const [query, setQuery] = useState("");

  // invite dialog
  const [inviteOpen, setInviteOpen] = useState(false);
  const [inviteEmail, setInviteEmail] = useState("");
  const [inviteRole, setInviteRole] = useState(roles[0].name);
  const [inviteFundAccess, setInviteFundAccess] = useState<"All funds" | "Fund III only">("All funds");

  // edit-role dialog (local draft only)
  const [editRole, setEditRole] = useState<Role | null>(null);
  const [draft, setDraft] = useState<Record<Module, string> | null>(null);

  const q = query.trim().toLowerCase();
  const filtered = users.filter(
    (u) => !q || u.name.toLowerCase().includes(q) || u.email.toLowerCase().includes(q) || u.role.toLowerCase().includes(q),
  );

  const userColumns: Column<StaffUser>[] = [
    {
      key: "user", header: "User", sortValue: (u) => u.name,
      render: (u) => (
        <span className="inline-flex items-center gap-2">
          <Avatar initials={u.initials} muted={u.status === "Invited"} />
          <span className={u.status === "Invited" ? "font-semibold text-ink-faint" : "font-semibold text-ink"}>{u.name}</span>
        </span>
      ),
    },
    { key: "role", header: "Role", sortValue: (u) => u.role },
    { key: "fundAccess", header: "Fund access", render: (u) => <span className="text-ink-muted">{u.fundAccess}</span> },
    { key: "lastActive", header: "Last active", render: (u) => <span className="text-ink-muted">{u.lastActive}</span> },
    { key: "status", header: "Status", render: (u) => <Pill>{u.status}</Pill>, sortValue: (u) => u.status },
  ];

  const matrixColumns: Column<Role>[] = [
    { key: "role", header: "Role", cellClass: "font-semibold text-ink", sortValue: (r) => r.name, render: (r) => r.name },
    ...MODULES.map((m): Column<Role> => ({
      key: m, header: m, render: (r) => <CapabilityBadge cap={r.capabilities[m]} />,
    })),
  ];

  function openEdit(role: Role) {
    setEditRole(role);
    setDraft(Object.fromEntries(MODULES.map((m) => [m, CAP_TO_SEG[role.capabilities[m]]])) as Record<Module, string>);
  }

  function sendInvite() {
    toast.push({ kind: "success", title: `Invite sent · ${inviteEmail}`, detail: `${inviteRole} · ${inviteFundAccess}` });
    setInviteOpen(false);
    setInviteEmail("");
  }

  function capabilitySummary(r: Role): string {
    return MODULES.map((m) => `${m} ${CAP_TO_SEG[r.capabilities[m]]}`).join(" · ");
  }

  const matrix = (
    <>
      <SectionTitle>Role → capability matrix</SectionTitle>
      <DataTable columns={matrixColumns} rows={roles} rowKey={(r) => r.name} />
      <div className="flex flex-wrap items-center gap-4 px-5 pb-4 pt-2.5 text-[10px] text-ink-faint">
        <span className="inline-flex items-center gap-1.5"><CapabilityBadge cap="view" /> view</span>
        <span className="inline-flex items-center gap-1.5"><CapabilityBadge cap="edit" /> edit</span>
        <span className="inline-flex items-center gap-1.5"><CapabilityBadge cap="approve" /> approve</span>
        <span className="inline-flex items-center gap-1.5"><CapabilityBadge cap="full" /> full</span>
        <span className="inline-flex items-center gap-1.5"><CapabilityBadge cap="none" /> none</span>
      </div>
    </>
  );

  return (
    <div>
      <ScreenHeader title={<><span className="font-medium text-ink-faint">Admin /</span> Users &amp; Roles</>}>
        <SearchInput value={query} onChange={setQuery} placeholder="Search users…" />
        <Button>Export</Button>
        <Button variant="primary" onClick={() => setInviteOpen(true)}>+ Invite user</Button>
      </ScreenHeader>

      <KpiRow>
        <Kpi label="Users" value={kpis.users} sub="this workspace" />
        <Kpi label="Active" value={kpis.active} sub="signed in ≤ 30d" />
        <Kpi label="Pending Invites" value={kpis.pendingInvites} sub="awaiting accept" tone="primary" />
        <Kpi label="Roles" value={kpis.roles} sub="defined" />
      </KpiRow>

      <Tabs tabs={TABS} active={tab} onChange={setTab} />

      {tab === "Users" && (
        <>
          <DataTable
            columns={userColumns}
            rows={filtered}
            rowKey={(u) => u.id}
            emptyState={<ZeroResults onClear={() => setQuery("")} />}
          />
          {matrix}
        </>
      )}

      {tab === "Roles" && (
        <div className="grid grid-cols-1 gap-3 px-5 py-4 sm:grid-cols-2">
          {roles.map((r) => (
            <Card key={r.name} className="flex items-center justify-between gap-3 px-3.5 py-3">
              <div className="min-w-0">
                <div className="text-xs font-bold text-ink">{r.name}</div>
                <div className="mt-1 truncate font-mono text-[10.5px] text-ink-muted">{capabilitySummary(r)}</div>
              </div>
              <Button size="sm" onClick={() => openEdit(r)}>Edit role</Button>
            </Card>
          ))}
        </div>
      )}

      {tab === "Permissions" && matrix}

      {/* Invite user dialog (5i) */}
      <Modal
        open={inviteOpen}
        onClose={() => setInviteOpen(false)}
        title="Invite user"
        footer={
          <>
            <Button onClick={() => setInviteOpen(false)}>Cancel</Button>
            <Button variant="primary" disabled={!inviteEmail.includes("@")} onClick={sendInvite}>Send invite</Button>
          </>
        }
      >
        <div className="flex flex-col gap-3.5">
          <Field label="Email">
            <TextInput placeholder="name@firm.com" value={inviteEmail} onChange={(e) => setInviteEmail(e.target.value)} />
          </Field>
          <Field label="Role">
            <Select options={roles.map((r) => r.name)} value={inviteRole} onChange={(e) => setInviteRole(e.target.value)} />
          </Field>
          <Field label="Fund access">
            <div className="flex flex-col gap-1.5">
              {(["All funds", "Fund III only"] as const).map((opt) => (
                <button
                  key={opt}
                  type="button"
                  aria-pressed={inviteFundAccess === opt}
                  onClick={() => setInviteFundAccess(opt)}
                  className={`flex items-center gap-2 text-left text-xs ${inviteFundAccess === opt ? "text-ink" : "text-ink-muted"}`}
                >
                  {inviteFundAccess === opt
                    ? <span aria-hidden className="flex h-4 w-4 flex-none items-center justify-center rounded bg-primary text-[10px] font-bold text-white">✓</span>
                    : <span aria-hidden className="h-4 w-4 flex-none rounded border border-line-strong" />}
                  {opt}
                </button>
              ))}
            </div>
          </Field>
          <p className="text-[11px] text-ink-muted">Permissions follow the role — see the matrix below.</p>
        </div>
      </Modal>

      {/* Edit role dialog (5i) — local draft only */}
      <Modal
        open={editRole !== null}
        onClose={() => setEditRole(null)}
        title={`Edit role · ${editRole?.name ?? ""}`}
        footer={
          <>
            <Button onClick={() => setEditRole(null)}>Cancel</Button>
            <Button
              variant="primary"
              onClick={() => { toast.push({ kind: "success", title: `Role updated · ${editRole?.name}` }); setEditRole(null); }}
            >
              Save role
            </Button>
          </>
        }
      >
        {editRole && draft && (
          <div className="flex flex-col gap-2.5">
            <span className="text-[10.5px] font-semibold uppercase tracking-wider text-ink-faint">Capabilities</span>
            {MODULES.map((m) => (
              <div key={m} className="flex items-center justify-between gap-3 text-xs text-ink">
                {m}
                <SegmentPicker
                  value={draft[m]}
                  onChange={(v) => setDraft((d) => (d ? { ...d, [m]: v } : d))}
                  segments={Object.values(editRole.capabilities).includes("full") ? ["—", "V", "E", "A", "✓"] : ["—", "V", "E", "A"]}
                />
              </div>
            ))}
          </div>
        )}
      </Modal>
    </div>
  );
}
