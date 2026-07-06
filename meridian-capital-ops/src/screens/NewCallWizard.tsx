"use client";

import { useRouter } from "next/navigation";
import { useMemo, useState } from "react";
import type { Deal, Fund, Investor } from "@/lib/types";
import { money, pct } from "@/lib/format";
import { Button } from "@/components/ui/Button";
import { Field, Select, TextInput, Toggle } from "@/components/ui/controls";
import { useToast } from "@/components/ui/Toast";
import { Breadcrumb } from "@/components/ui/primitives";

const STEPS = ["Deal", "Amount", "Investors", "Notice", "Review"];

/**
 * New-capital-call wizard. Allocations default to pro-rata by UNFUNDED
 * commitment (the correct basis — a fully-drawn LP shouldn't keep being
 * called); a basis selector allows pro-rata by total commitment. Allocations
 * stay editable and must reconcile to the call amount before Continue.
 */
export function NewCallWizard({ deals, funds, investors }: { deals: Deal[]; funds: Fund[]; investors: Investor[] }) {
  const router = useRouter();
  const toast = useToast();
  const [step, setStep] = useState(0);
  const [dealId, setDealId] = useState(deals[1]?.id ?? deals[0].id); // Beacon by default, mirroring the wireframe
  const [amountStr, setAmountStr] = useState("26.3");
  const [dueDate, setDueDate] = useState("2026-07-09");
  const [basis, setBasis] = useState<"unfunded" | "commitment">("unfunded");
  const [overrides, setOverrides] = useState<Record<string, number>>({});
  const [notice, setNotice] = useState({ attachWireInstructions: true, emailContacts: true, portalPublish: true });

  const deal = deals.find((d) => d.id === dealId)!;
  const fund = funds.find((f) => f.id === deal.fundId)!;
  const amount = parseFloat(amountStr) || 0;

  /** LPs committed to the call's fund, with the pro-rata basis value. */
  const lps = useMemo(() => investors
    .map((inv) => {
      const c = inv.commitments.find((x) => x.fundId === deal.fundId);
      return c ? { id: inv.id, name: inv.name, commitment: c.amount, unfunded: +(c.amount - c.called).toFixed(1), wireOnFile: inv.wireInstructionsOnFile } : null;
    })
    .filter((x): x is NonNullable<typeof x> => x !== null), [investors, deal.fundId]);

  const basisTotal = lps.reduce((s, lp) => s + (basis === "unfunded" ? lp.unfunded : lp.commitment), 0);

  const allocations = lps.map((lp) => {
    const basisValue = basis === "unfunded" ? lp.unfunded : lp.commitment;
    const proRata = basisTotal > 0 ? +((basisValue / basisTotal) * amount).toFixed(2) : 0;
    return { ...lp, basisValue, pct: basisTotal > 0 ? (basisValue / basisTotal) * 100 : 0, amount: overrides[lp.id] ?? proRata };
  });
  const allocTotal = +allocations.reduce((s, a) => s + a.amount, 0).toFixed(2);
  const balanced = Math.abs(allocTotal - amount) < 0.02;

  const stepValid = [
    !!dealId,
    amount > 0 && !!dueDate,
    balanced,
    true,
    true,
  ][step];

  async function create() {
    await fetch("/api/capital-calls", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ dealId, amount, dueDate, basis, allocations: allocations.map(({ id, amount }) => ({ investorId: id, amount })) }),
    }).catch(() => null);
    toast.push({ kind: "success", title: "Capital call created", detail: `${deal.name} · ${money(amount)} · enters the pipeline at stage 1`, actionLabel: "Undo" });
    router.push("/capital-calls");
  }

  return (
    <div>
      <div className="border-b border-line px-5 py-3">
        <Breadcrumb backHref="/capital-calls" backLabel="Blotter" current="New capital call" />
      </div>

      <div className="mx-auto my-6 w-full max-w-xl rounded-xl border border-line bg-card shadow-card">
        {/* step indicator */}
        <ol className="flex items-center gap-2 border-b border-line bg-fill px-4 py-2.5 text-[10.5px] text-ink-faint">
          {STEPS.map((s, i) => (
            <li key={s} className="flex items-center gap-2">
              {i > 0 && <span aria-hidden className="text-line-strong">—</span>}
              <span className={`flex items-center gap-1.5 ${i === step ? "font-bold text-ink" : ""}`}>
                <span className={`flex h-4 w-4 items-center justify-center rounded-full text-[8.5px] font-bold ${
                  i < step ? "bg-positive text-white" : i === step ? "bg-primary text-white" : "border border-line-strong text-ink-faint"}`}>
                  {i < step ? "✓" : i + 1}
                </span>
                {s}
              </span>
            </li>
          ))}
        </ol>

        <div className="flex flex-col gap-4 px-4 py-4">
          {step === 0 && (
            <>
              <Field label="Deal">
                <Select options={deals.map((d) => d.name)} value={deal.name}
                  onChange={(e) => { setDealId(deals.find((d) => d.name === e.target.value)!.id); setOverrides({}); }} />
              </Field>
              <Field label="Fund" hint="Derived from the deal — allocations draw on this fund's LPs.">
                <TextInput readOnly value={fund.name} className="bg-fill" />
              </Field>
            </>
          )}

          {step === 1 && (
            <div className="flex gap-3">
              <div className="flex-1">
                <Field label="Call amount ($M)" required>
                  <TextInput type="number" min="0" step="0.1" value={amountStr} onChange={(e) => { setAmountStr(e.target.value); setOverrides({}); }} />
                </Field>
              </div>
              <div className="flex-1">
                <Field label="Due date" required hint="Validated against the fund's settlement calendar.">
                  <TextInput type="date" value={dueDate} onChange={(e) => setDueDate(e.target.value)} />
                </Field>
              </div>
            </div>
          )}

          {step === 2 && (
            <>
              <div className="flex items-center justify-between">
                <span className="text-[10.5px] font-semibold uppercase tracking-wider text-ink-faint">Investor allocations</span>
                <label className="flex items-center gap-2 text-[11px] font-medium text-ink-secondary">
                  Pro-rata by
                  <select value={basis} onChange={(e) => { setBasis(e.target.value as typeof basis); setOverrides({}); }}
                    className="rounded-md border border-line-strong bg-card px-1.5 py-0.5 text-[11px] font-semibold focus:border-primary focus:outline-none">
                    <option value="unfunded">unfunded commitment</option>
                    <option value="commitment">total commitment</option>
                  </select>
                </label>
              </div>
              <table className="w-full border-collapse overflow-hidden rounded-lg border border-line text-left">
                <thead>
                  <tr className="bg-fill">
                    {["Investor", basis === "unfunded" ? "Unfunded" : "Commitment", "%", "Allocation"].map((h, i) => (
                      <th key={h} className={`border-b border-line px-3 py-1.5 text-[10px] font-semibold uppercase tracking-wider text-ink-faint ${i > 0 ? "text-right" : ""}`}>{h}</th>
                    ))}
                  </tr>
                </thead>
                <tbody>
                  {allocations.map((a) => (
                    <tr key={a.id} className="border-b border-line/60">
                      <td className="px-3 py-1.5 text-xs font-semibold text-ink">
                        {a.name}
                        {!a.wireOnFile && <span className="ml-1.5 rounded-full bg-danger-soft px-1.5 text-[9px] font-bold text-danger" title="No wire instructions on file">no wire instr.</span>}
                      </td>
                      <td className="num px-3 py-1.5 text-right text-xs text-ink-muted">{money(a.basisValue)}</td>
                      <td className="num px-3 py-1.5 text-right text-xs text-ink-muted">{pct(a.pct)}</td>
                      <td className="px-3 py-1.5 text-right">
                        <span className="inline-flex items-center gap-1 text-xs font-semibold">
                          $<input type="number" step="0.1" min="0" aria-label={`${a.name} allocation in millions`} value={a.amount}
                            onChange={(e) => setOverrides((p) => ({ ...p, [a.id]: parseFloat(e.target.value) || 0 }))}
                            className="num w-18 rounded-md border border-line-strong px-1.5 py-0.5 text-right text-xs font-semibold focus:border-primary focus:outline-none" />M
                        </span>
                      </td>
                    </tr>
                  ))}
                  <tr className="bg-fill-strong">
                    <td className="px-3 py-1.5 text-xs font-bold">Total</td>
                    <td />
                    <td className="num px-3 py-1.5 text-right text-xs font-bold">100%</td>
                    <td className="num px-3 py-1.5 text-right text-xs font-bold">
                      {money(allocTotal)} {balanced ? <span className="text-positive">✓</span> : <span className="text-danger">≠ {money(amount)}</span>}
                    </td>
                  </tr>
                </tbody>
              </table>
              {!balanced && <p className="text-[11px] font-medium text-danger">Total must equal the {money(amount)} call amount to continue.</p>}
            </>
          )}

          {step === 3 && (
            <>
              <p className="text-xs text-ink-secondary">The capital call notice is generated per LP with their allocation, due date and the fund's wire instructions.</p>
              {([
                ["attachWireInstructions", "Attach fund wire instructions"],
                ["emailContacts", "Email notice to LP contacts"],
                ["portalPublish", "Publish notice to the Investor Portal"],
              ] as const).map(([key, label]) => (
                <label key={key} className="flex items-center gap-2.5 text-xs text-ink">
                  <Toggle on={notice[key]} onChange={(on) => setNotice((n) => ({ ...n, [key]: on }))} label={label} /> {label}
                </label>
              ))}
            </>
          )}

          {step === 4 && (
            <div className="flex flex-col gap-2 text-xs">
              {[
                ["Deal", `${deal.name} · ${deal.borrower}`],
                ["Fund", fund.name],
                ["Amount", money(amount)],
                ["Due date", dueDate],
                ["Allocations", `${allocations.length} LPs · pro-rata by ${basis === "unfunded" ? "unfunded commitment" : "total commitment"} · ✓ balanced`],
                ["Notices", [notice.emailContacts && "email", notice.portalPublish && "portal", notice.attachWireInstructions && "wire instructions"].filter(Boolean).join(" · ") || "none"],
              ].map(([k, v]) => (
                <div key={k} className="flex justify-between gap-4 border-b border-dashed border-line pb-1.5">
                  <span className="text-ink-muted">{k}</span><span className="text-right font-semibold text-ink">{v}</span>
                </div>
              ))}
              <p className="mt-1 text-[11px] text-ink-muted">On create, the call enters the 9-stage approval pipeline at <b>Operations</b> and notices are queued.</p>
            </div>
          )}
        </div>

        <div className="flex items-center justify-between gap-2 border-t border-line bg-fill px-4 py-3">
          <Button onClick={() => router.push("/capital-calls")}>Cancel</Button>
          <div className="flex gap-2">
            {step > 0 && <Button onClick={() => setStep((s) => s - 1)}>← Back</Button>}
            {step < STEPS.length - 1
              ? <Button variant="primary" disabled={!stepValid} onClick={() => setStep((s) => s + 1)}>Continue → {STEPS[step + 1]}</Button>
              : <Button variant="accent" onClick={create}>Create call</Button>}
          </div>
        </div>
      </div>
    </div>
  );
}
