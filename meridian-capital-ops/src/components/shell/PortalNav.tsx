"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";

const ITEMS = [
  { label: "Overview", href: "/portal" },
  { label: "My Investments", href: "/portal/investments" },
  { label: "Capital Activity", href: "/portal/activity" },
  { label: "Statements", href: "/portal/statements" },
  { label: "Tax Documents", href: "/portal/tax" },
  { label: "Contact IR", href: "/portal/contact" },
];

export function PortalNav() {
  const pathname = usePathname();
  return (
    <nav aria-label="Portal" className="w-44 flex-none border-r border-line bg-fill py-3">
      {ITEMS.map((item) => {
        const active = item.href === "/portal" ? pathname === "/portal" : pathname.startsWith(item.href);
        return (
          <Link
            key={item.href}
            href={item.href}
            aria-current={active ? "page" : undefined}
            className={`flex items-center gap-2.5 border-l-[3px] px-4 py-2 text-xs font-medium ${
              active ? "border-primary bg-card font-semibold text-ink" : "border-transparent text-ink-secondary hover:text-ink"
            }`}
          >
            <span className={`h-1.5 w-1.5 flex-none rounded-full ${active ? "bg-primary" : "bg-line-strong"}`} />
            {item.label}
          </Link>
        );
      })}
    </nav>
  );
}
