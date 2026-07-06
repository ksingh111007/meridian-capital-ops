"use client";

import { useState } from "react";
import type { PortalIrInfo } from "@/lib/types";
import { fmtDate, TODAY } from "@/lib/format";
import { Button } from "@/components/ui/Button";
import { Field, Select, TextArea, TextInput } from "@/components/ui/controls";
import { Avatar, Card, DefRow } from "@/components/ui/primitives";
import { Pill } from "@/components/ui/Pill";
import { useToast } from "@/components/ui/Toast";
import { PortalPageHeader } from "./shared";

type IrRequest = PortalIrInfo["recentRequests"][number];

const NEW_REQUEST_REF = "#REQ-3402";

/** Screen 6i — secure messaging + relationship contacts; requests are ticketed & tracked. */
export function PortalContactScreen({ info }: { info: PortalIrInfo & { regardingOptions: string[] } }) {
  const toast = useToast();
  const [subject, setSubject] = useState("");
  const [regarding, setRegarding] = useState(info.regardingOptions[0]);
  const [message, setMessage] = useState("");
  const [requests, setRequests] = useState<IrRequest[]>(info.recentRequests);

  const canSend = subject.trim().length > 0 && message.trim().length > 0;

  async function send() {
    await fetch("/api/portal/messages", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ subject, regarding, message }),
    }).catch(() => null);
    toast.push({
      kind: "success",
      title: "Message sent",
      detail: `${NEW_REQUEST_REF} · IR will reply within 1 business day`,
    });
    setRequests((prev) => [
      { subject: subject.trim(), ref: NEW_REQUEST_REF, date: fmtDate(TODAY, "short"), status: "Open" },
      ...prev,
    ]);
    setSubject("");
    setRegarding(info.regardingOptions[0]);
    setMessage("");
  }

  return (
    <div>
      <PortalPageHeader
        title="Contact Investor Relations"
        subtitle="Send a secure message — we typically reply within one business day"
      />

      <div className="mt-4 flex flex-col items-start gap-4 lg:flex-row">
        <Card className="w-full min-w-0 flex-[1.25] p-4">
          <div className="flex flex-col gap-3">
            <Field label="Subject" required>
              <TextInput
                value={subject}
                placeholder="e.g. Wire instruction update"
                onChange={(e) => setSubject(e.target.value)}
              />
            </Field>
            <Field label="Regarding">
              <Select options={info.regardingOptions} value={regarding} onChange={(e) => setRegarding(e.target.value)} />
            </Field>
            <Field label="Message" required>
              <TextArea rows={5} value={message} onChange={(e) => setMessage(e.target.value)} />
            </Field>
            <div className="flex items-center justify-between">
              <button type="button" className="flex items-center gap-1.5 text-[10.5px] text-ink-muted hover:text-ink">
                <span aria-hidden className="inline-block h-4 w-4 rounded border-[1.3px] border-dashed border-line-strong" />
                ⬆ Attach a file
              </button>
              <Button variant="primary" disabled={!canSend} onClick={send}>Send message</Button>
            </div>
          </div>
        </Card>

        <div className="flex w-full min-w-0 flex-1 flex-col gap-3">
          <Card className="p-4">
            <h2 className="mb-2.5 text-[10.5px] font-semibold uppercase tracking-wider text-ink-faint">Your IR team</h2>
            <div className="flex items-center gap-2.5">
              <Avatar initials={info.manager.initials} size="lg" />
              <div>
                <div className="text-xs font-bold text-ink">{info.manager.name}</div>
                <div className="text-[10px] text-ink-muted">{info.manager.title}</div>
              </div>
            </div>
            <div className="mt-3 flex flex-col gap-1.5">
              <DefRow label="Email">{info.email}</DefRow>
              <DefRow label="Direct">{info.phone}</DefRow>
              <DefRow label="Hours">{info.hours}</DefRow>
            </div>
          </Card>

          <Card className="p-4">
            <h2 className="mb-2.5 text-[10.5px] font-semibold uppercase tracking-wider text-ink-faint">Recent requests</h2>
            <div className="flex flex-col gap-2.5">
              {requests.map((r) => (
                <div key={r.ref} className="flex items-center justify-between gap-3">
                  <div className="min-w-0">
                    <div className="truncate text-[11px] font-semibold text-ink">{r.subject}</div>
                    <div className="text-[9.5px] text-ink-muted">{r.ref} · {r.date}</div>
                  </div>
                  <Pill tone={r.status === "Open" ? "blue" : undefined}>{r.status}</Pill>
                </div>
              ))}
            </div>
          </Card>
        </div>
      </div>
    </div>
  );
}
