/**
 * Mock API — one registry entry per endpoint, each backed by a JSON file in
 * src/mocks/ via the data layer (src/lib/data.ts).
 *
 * The real backend replaces this whole surface; the contract (paths, methods,
 * shapes) is documented in docs/API.md. Screens do NOT fetch these routes —
 * server components call the data layer directly. These routes exist so the
 * API contract is exercisable (curl, integration tests, future client-side
 * data fetching).
 */
import { NextResponse } from "next/server";
import * as data from "@/lib/data";

const GET_ROUTES: Record<string, (params: string[]) => unknown> = {
  "me": () => data.getCurrentUser(),
  "needs-attention": () => data.getNeedsAttention(),
  "portfolio/summary": () => data.getPortfolioSummary(),
  "deals": () => data.getDeals(),
  "deals/:id": ([id]) => data.getDeal(id),
  "capital-calls": () => data.getCapitalCalls(),
  "capital-calls/:id": ([id]) => data.getCapitalCall(id),
  "workflows/capital-calls": () => data.getWorkflow(),
  "distributions": () => data.getDistributions(),
  "distributions/:id": ([id]) => data.getDistribution(id),
  "drawdowns": () => data.getDrawdowns(),
  "wires": () => data.getWires(),
  "cash/position": () => data.getCashPosition(),
  "reconciliation": () => data.getReconciliation(),
  "admin/funds": () => data.getFunds(),
  "admin/investors": () => data.getInvestors(),
  "admin/users": () => data.getUsersAndRoles(),
  "admin/reference": () => data.getReferenceData(),
  "admin/integrations": () => data.getIntegrations(),
  "admin/notification-rules": () => data.getNotificationRules(),
  "admin/audit": () => data.getAuditLog(),
  "admin/investor-access": () => data.getInvestorAccess(),
  "portal/account": () => data.getPortalAccount(),
  "portal/investments": () => data.getPortalInvestments(),
  "portal/activity": () => data.getPortalActivity(),
  "portal/statements": () => data.getPortalStatements(),
  "portal/tax": () => data.getPortalTax(),
  "portal/contact": () => data.getPortalIrInfo(),
};

/** Mutations accepted by the mock API. State does not persist (no backend yet) —
 *  each returns a realistic acknowledgement so client flows can be exercised. */
const POST_ROUTES = new Set([
  "capital-calls",                 // create call (2c wizard)
  "capital-calls/:id/approve",     // { comment } — advance stage
  "capital-calls/:id/reject",      // { comment } — return to prior stage
  "wires/:id/retry",               // retry an exception wire
  "reconciliation/:id/assign",     // assign a break
  "portal/messages",               // Contact IR secure message
]);

function match(path: string, routes: Iterable<string>): { pattern: string; params: string[] } | null {
  const segs = path.split("/");
  for (const pattern of routes) {
    const psegs = pattern.split("/");
    if (psegs.length !== segs.length) continue;
    const params: string[] = [];
    const ok = psegs.every((p, i) => (p.startsWith(":") ? (params.push(segs[i]), true) : p === segs[i]));
    if (ok) return { pattern, params };
  }
  return null;
}

type Ctx = { params: Promise<{ endpoint: string[] }> };

export async function GET(_req: Request, ctx: Ctx) {
  const { endpoint } = await ctx.params;
  const path = endpoint.join("/");
  const m = match(path, Object.keys(GET_ROUTES));
  if (!m) return NextResponse.json({ error: `Unknown endpoint: GET /api/${path}` }, { status: 404 });
  const result = GET_ROUTES[m.pattern](m.params);
  if (result === undefined) return NextResponse.json({ error: "Not found" }, { status: 404 });
  return NextResponse.json(result);
}

export async function POST(req: Request, ctx: Ctx) {
  const { endpoint } = await ctx.params;
  const path = endpoint.join("/");
  const m = match(path, POST_ROUTES);
  if (!m) return NextResponse.json({ error: `Unknown endpoint: POST /api/${path}` }, { status: 404 });
  const body = await req.json().catch(() => ({}));
  // Mock: acknowledge without persisting. The real backend must enforce RBAC
  // and append an audit event for every mutation (docs/BUSINESS_RULES.md).
  return NextResponse.json({ ok: true, endpoint: m.pattern, received: body, auditEvent: "mock-not-persisted" });
}
