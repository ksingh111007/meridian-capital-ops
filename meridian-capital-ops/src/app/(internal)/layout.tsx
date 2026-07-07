import { TreeNav } from "@/components/shell/TreeNav";
import { TopBar } from "@/components/shell/TopBar";
import { ToastProvider } from "@/components/ui/Toast";
import { getCurrentUser, getNeedsAttention } from "@/lib/data";

// Data comes from the backend per request (no-store) — never prerender at build.
export const dynamic = "force-dynamic";

/** Internal (staff-facing) app shell: collapsible tree nav + top bar. */
export default async function InternalLayout({ children }: { children: React.ReactNode }) {
  const [user, attention] = await Promise.all([getCurrentUser(), getNeedsAttention()]);
  return (
    <ToastProvider>
      <div className="flex h-dvh overflow-hidden">
        <TreeNav />
        <div className="flex min-w-0 flex-1 flex-col">
          <TopBar user={user} attention={attention} />
          <main className="min-h-0 flex-1 overflow-auto bg-card">{children}</main>
        </div>
      </div>
    </ToastProvider>
  );
}
