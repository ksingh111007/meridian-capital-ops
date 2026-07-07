import Link from "next/link";
import { ToastProvider } from "@/components/ui/Toast";
import { PortalNav } from "@/components/shell/PortalNav";
import { Avatar } from "@/components/ui/primitives";
import { getPortalAccount } from "@/lib/data";

/**
 * External Investor Portal shell — a separate authenticated experience.
 * An LP only ever sees their own capital account; in production this lives
 * behind its own auth boundary (see docs/ARCHITECTURE.md).
 */
// Data comes from the backend per request (no-store) — never prerender at build.
export const dynamic = "force-dynamic";

export default async function PortalLayout({ children }: { children: React.ReactNode }) {
  const account = await getPortalAccount();
  return (
    <ToastProvider>
      <div className="flex h-dvh flex-col overflow-hidden">
        <header className="flex h-13 flex-none items-center justify-between border-b border-line bg-fill px-5 py-3">
          <Link href="/portal" className="flex items-baseline gap-2.5">
            <span className="text-[17px] font-bold tracking-tight text-ink">Meridian</span>
            <span className="rounded-full border border-primary-line bg-primary-soft px-2 py-0.5 text-[8.5px] font-bold uppercase tracking-[0.12em] text-primary">
              Investor Portal
            </span>
          </Link>
          <div className="flex items-center gap-2.5 text-xs font-medium text-ink-secondary">
            <span>{account.investor}</span>
            <Avatar initials={account.contactInitials} />
            <button type="button" className="text-ink-faint hover:text-ink">Log out</button>
          </div>
        </header>
        <div className="flex min-h-0 flex-1">
          <PortalNav />
          <main className="min-w-0 flex-1 overflow-auto bg-fill px-6 py-5">{children}</main>
        </div>
      </div>
    </ToastProvider>
  );
}
