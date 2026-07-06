"use client";

import { useState } from "react";
import type { NotificationRule } from "@/lib/types";
import { ScreenHeader } from "@/components/shell/ScreenHeader";
import { Button } from "@/components/ui/Button";
import { DataTable, type Column } from "@/components/ui/DataTable";
import { Modal } from "@/components/ui/Modal";
import { Pill } from "@/components/ui/Pill";
import { useToast } from "@/components/ui/Toast";
import { Field, Select, TextInput, Toggle } from "@/components/ui/controls";

interface Channel { name: string; detail: string; connected: boolean }

const TRIGGERS = [
  "Capital call due in 3 days",
  "Wire failed / rejected",
  "Pending approval > SLA",
  "Break > $100k",
  "Distribution finalised",
  "LP commitment added",
  "Utilisation > 80%",
];

const CHANNEL_OPTIONS = ["Email", "Slack", "SMS"];

export function NotificationRulesScreen({ rules: initialRules, channels }: { rules: NotificationRule[]; channels: Channel[] }) {
  const toast = useToast();
  const [rules, setRules] = useState(initialRules);

  // new-rule dialog (5i)
  const [open, setOpen] = useState(false);
  const [name, setName] = useState("");
  const [trigger, setTrigger] = useState(TRIGGERS[0]);
  const [ruleChannels, setRuleChannels] = useState<string[]>(["Email"]);
  const [recipients, setRecipients] = useState("");
  const [enabled, setEnabled] = useState(true);

  function toggleRule(rule: NotificationRule, on: boolean) {
    setRules((prev) => prev.map((r) => (r.id === rule.id ? { ...r, enabled: on } : r)));
    toast.push({ kind: "success", title: `Rule ${on ? "enabled" : "disabled"} · ${rule.name}` });
  }

  function createRule() {
    const rule: NotificationRule = {
      id: `nr-${rules.length + 1}-${Date.now()}`,
      name: name.trim(),
      trigger,
      channel: ruleChannels.join(" + ") || "Email",
      recipients: recipients.trim() || "—",
      enabled,
    };
    setRules((prev) => [...prev, rule]);
    toast.push({ kind: "success", title: `Rule created · ${rule.name}`, detail: `${rule.trigger} → ${rule.channel}` });
    setOpen(false);
    setName("");
    setRecipients("");
    setRuleChannels(["Email"]);
    setEnabled(true);
  }

  const columns: Column<NotificationRule>[] = [
    { key: "name", header: "Rule", cellClass: "font-semibold text-ink", sortValue: (r) => r.name },
    { key: "trigger", header: "Trigger", render: (r) => <span className="text-ink-muted">{r.trigger}</span> },
    { key: "channel", header: "Channel", sortValue: (r) => r.channel },
    { key: "recipients", header: "Recipients", render: (r) => <span className="text-ink-muted">{r.recipients}</span> },
    {
      key: "on", header: "On", align: "center",
      render: (r) => <Toggle on={r.enabled} onChange={(on) => toggleRule(r, on)} label={`${r.name} enabled`} />,
    },
  ];

  return (
    <div>
      <ScreenHeader title={<><span className="font-medium text-ink-faint">Admin /</span> Notification Rules</>}>
        <Button variant="primary" onClick={() => setOpen(true)}>+ New rule</Button>
      </ScreenHeader>

      <DataTable columns={columns} rows={rules} rowKey={(r) => r.id} />

      <div className="flex flex-wrap items-center gap-2 border-t border-line bg-fill px-5 py-3.5">
        <span className="mr-1 text-[10.5px] font-semibold uppercase tracking-wider text-ink-faint">Channels</span>
        {channels.map((c) => (
          <Pill key={c.name} tone={c.connected ? "green" : "neutral"}>{c.name} · {c.detail}</Pill>
        ))}
      </div>

      {/* New notification rule dialog (5i) */}
      <Modal
        open={open}
        onClose={() => setOpen(false)}
        title="New notification rule"
        footer={
          <>
            <Button onClick={() => setOpen(false)}>Cancel</Button>
            <Button variant="primary" disabled={!name.trim()} onClick={createRule}>Create rule</Button>
          </>
        }
      >
        <div className="flex flex-col gap-3.5">
          <Field label="Name">
            <TextInput placeholder="e.g. Large call sign-off" value={name} onChange={(e) => setName(e.target.value)} />
          </Field>
          <Field label="When">
            <Select options={TRIGGERS} value={trigger} onChange={(e) => setTrigger(e.target.value)} />
          </Field>
          <Field label="Channel">
            <div className="flex gap-1.5">
              {CHANNEL_OPTIONS.map((c) => {
                const on = ruleChannels.includes(c);
                return (
                  <button
                    key={c}
                    type="button"
                    aria-pressed={on}
                    onClick={() => setRuleChannels((prev) => (on ? prev.filter((x) => x !== c) : [...prev, c]))}
                    className={`rounded-full border px-2.5 py-0.5 text-[10.5px] font-semibold leading-4 transition-colors ${
                      on ? "border-positive-line bg-positive-soft text-positive" : "border-line-strong bg-fill-strong text-ink-secondary hover:border-ink"
                    }`}
                  >
                    {c}
                  </button>
                );
              })}
            </div>
          </Field>
          <Field label="Recipients">
            <TextInput placeholder="Ops + Deal Lead" value={recipients} onChange={(e) => setRecipients(e.target.value)} />
          </Field>
          <label className="flex items-center gap-2.5 text-xs text-ink">
            <Toggle on={enabled} onChange={setEnabled} label="Enabled" /> Enabled
          </label>
        </div>
      </Modal>
    </div>
  );
}
