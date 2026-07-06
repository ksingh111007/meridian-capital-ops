"use client";

import Link from "next/link";
import { useState } from "react";
import type { AttentionItem, CurrentUser } from "@/lib/types";
import { Avatar } from "@/components/ui/primitives";

/**
 * Internal shell top bar: persistent fund selector (context stays stable across
 * screens) + the needs-attention bell + current user.
 */
export function TopBar({ user, attention }: { user: CurrentUser; attention: AttentionItem[] }) {
  const [fund, setFund] = useState("Fund III");
  const [open, setOpen] = useState(false);
  const urgent = attention.filter((a) => a.tone === "red" || a.mine).length;

  return (
    <header className="relative z-20 flex h-12 flex-none items-center justify-between gap-3 border-b border-line bg-card px-5">
      <div className="flex items-center gap-2">
        <label htmlFor="fund-select" className="text-[10px] font-semibold uppercase tracking-wider text-ink-faint">Fund</label>
        <select
          id="fund-select"
          value={fund}
          onChange={(e) => setFund(e.target.value)}
          className="rounded-md border border-line-strong bg-card px-2 py-1 text-xs font-semibold text-ink focus:border-primary focus:outline-none"
        >
          <option>Fund III</option>
          <option>Fund II</option>
          <option>Fund I</option>
          <option>All funds</option>
        </select>
      </div>

      <div className="flex items-center gap-3">
        <div className="relative">
          <button
            type="button"
            aria-label={`Needs attention: ${attention.length} items`}
            aria-expanded={open}
            onClick={() => setOpen((o) => !o)}
            className="relative rounded-md border border-line-strong bg-card px-2.5 py-1 text-xs font-semibold text-ink-secondary hover:border-ink"
          >
            Needs attention
            {urgent > 0 && (
              <span className="absolute -right-1.5 -top-1.5 flex h-4 min-w-4 items-center justify-center rounded-full bg-danger px-1 text-[9px] font-bold text-white">
                {urgent}
              </span>
            )}
          </button>
          {open && (
            <div className="absolute right-0 top-9 w-96 rounded-lg border border-line bg-card shadow-pop">
              <div className="border-b border-line px-3.5 py-2 text-[10.5px] font-semibold uppercase tracking-wider text-ink-faint">
                Needs attention · {attention.length}
              </div>
              <ul className="max-h-80 overflow-auto py-1">
                {attention.map((a) => (
                  <li key={a.id}>
                    <Link href={a.href} onClick={() => setOpen(false)} className="flex gap-2.5 px-3.5 py-2 hover:bg-fill">
                      <span aria-hidden className={`mt-1 h-2 w-2 flex-none rounded-full ${
                        a.tone === "red" ? "bg-danger" : a.tone === "amber" ? "bg-caution-strong" : a.tone === "blue" ? "bg-primary" : "bg-line-strong"
                      }`} />
                      <span className="min-w-0">
                        <span className="block truncate text-xs font-semibold text-ink">
                          {a.title} {a.mine && <span className="ml-1 rounded-full bg-primary-soft px-1.5 text-[9px] font-bold text-primary">YOU</span>}
                        </span>
                        <span className="block truncate text-[11px] text-ink-muted">{a.detail}</span>
                      </span>
                    </Link>
                  </li>
                ))}
              </ul>
            </div>
          )}
        </div>
        <span className="flex items-center gap-2 text-xs text-ink-secondary">
          <Avatar initials={user.initials} />
          <span className="hidden sm:block">
            <span className="block font-semibold leading-tight text-ink">{user.name}</span>
            <span className="block text-[10px] leading-tight text-ink-muted">{user.role}</span>
          </span>
        </span>
      </div>
    </header>
  );
}
