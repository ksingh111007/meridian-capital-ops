"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useEffect, useState } from "react";

/**
 * Collapsible tree navigation for the internal app.
 * Group expand/collapse state is persisted per user (localStorage for the mock;
 * user preferences service in production).
 */

const NAV_GROUPS = [
  { label: "Inflows", items: [
    { label: "Capital Calls", href: "/capital-calls" },
    { label: "Drawdowns", href: "/drawdowns" },
  ]},
  { label: "Outflows", items: [
    { label: "Distributions", href: "/distributions" },
    { label: "Wire Status", href: "/wires" },
  ]},
  { label: "Fund", items: [
    { label: "Cash Position", href: "/cash" },
    { label: "Reconciliation", href: "/reconciliation" },
  ]},
  { label: "Admin", items: [
    { label: "Users & Roles", href: "/admin/users" },
    { label: "Approval Workflows", href: "/admin/workflows" },
    { label: "Funds & Entities", href: "/admin/funds" },
    { label: "Investor Registry", href: "/admin/investors" },
    { label: "Reference Data", href: "/admin/reference" },
    { label: "Integrations", href: "/admin/integrations" },
    { label: "Notifications", href: "/admin/notifications" },
    { label: "Investor Access", href: "/admin/investor-access" },
    { label: "Audit Log", href: "/admin/audit" },
  ]},
];

const STORAGE_KEY = "mco.nav.collapsed";

export function TreeNav() {
  const pathname = usePathname();
  const [collapsed, setCollapsed] = useState<Record<string, boolean>>({});

  useEffect(() => {
    try {
      const saved = localStorage.getItem(STORAGE_KEY);
      if (saved) setCollapsed(JSON.parse(saved));
    } catch { /* first visit */ }
  }, []);

  const toggle = (group: string) => {
    setCollapsed((prev) => {
      const next = { ...prev, [group]: !prev[group] };
      try { localStorage.setItem(STORAGE_KEY, JSON.stringify(next)); } catch { /* private mode */ }
      return next;
    });
  };

  const isActive = (href: string) => pathname === href || pathname.startsWith(`${href}/`);

  return (
    <nav aria-label="Primary" className="flex w-46 flex-none flex-col border-r border-line bg-fill py-3">
      <Link href="/portfolio" className="px-4 pb-3">
        <span className="block text-[17px] font-bold leading-none tracking-tight text-ink">Meridian</span>
        <span className="mt-1 block text-[8.5px] font-semibold uppercase tracking-[0.14em] text-ink-faint">Capital Ops</span>
      </Link>

      <Link
        href="/portfolio"
        className={`flex items-center gap-2.5 border-l-[3px] px-4 py-2 text-xs font-semibold ${
          isActive("/portfolio") ? "border-primary bg-card text-ink" : "border-transparent text-ink-secondary hover:text-ink"
        }`}
      >
        <span className={`h-3 w-3 flex-none rounded-sm border-[1.5px] ${isActive("/portfolio") ? "border-primary bg-primary" : "border-ink-faint"}`} />
        Portfolio
      </Link>

      {NAV_GROUPS.map((group) => (
        <div key={group.label}>
          <button
            type="button"
            aria-expanded={!collapsed[group.label]}
            onClick={() => toggle(group.label)}
            className="flex w-full items-center gap-2 px-4 pb-1 pt-3 text-[9.5px] font-bold uppercase tracking-[0.12em] text-ink-faint hover:text-ink"
          >
            <span aria-hidden className="w-2 text-[8px]">{collapsed[group.label] ? "▸" : "▾"}</span>
            {group.label}
          </button>
          {!collapsed[group.label] &&
            group.items.map((item) => (
              <Link
                key={item.href}
                href={item.href}
                aria-current={isActive(item.href) ? "page" : undefined}
                className={`flex items-center gap-2 border-l-[3px] py-1.5 pl-7 pr-3 text-[11.5px] font-medium ${
                  isActive(item.href) ? "border-primary bg-card font-semibold text-ink" : "border-transparent text-ink-secondary hover:text-ink"
                }`}
              >
                <span className={`h-1.5 w-1.5 flex-none rounded-full ${isActive(item.href) ? "bg-primary" : "bg-line-strong"}`} />
                {item.label}
              </Link>
            ))}
        </div>
      ))}

      <div className="mt-auto px-4 pt-4 text-[10px] text-ink-faint">
        <Link href="/portal" className="font-medium text-primary hover:underline">Investor Portal ↗</Link>
      </div>
    </nav>
  );
}
