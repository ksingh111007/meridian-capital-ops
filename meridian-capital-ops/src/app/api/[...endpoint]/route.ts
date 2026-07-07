/**
 * /api/* — in api mode (DATA_SOURCE=api, the default) this is a thin proxy
 * that forwards GET and POST requests to the real backend at
 * `${MERIDIAN_API_URL}/api/<path>`, attaching the dev-auth `X-User-Id` header
 * (staff user, or the portal contact for portal/* paths). Client-side
 * mutation POSTs (approve/reject, create call, wire retry, recon assign,
 * portal message) go through here and now actually persist.
 *
 * In mock mode (DATA_SOURCE=mock) it keeps the previous behavior: GETs are
 * served from the JSON mocks via the data layer, POSTs acknowledge without
 * persisting. The contract (paths, methods, shapes) is docs/API.md.
 */
import { NextResponse } from "next/server";
import * as data from "@/lib/data";
import { API_BASE_URL, USE_API, authHeader } from "@/lib/api";

// ---------- api mode: proxy to the backend ----------

async function proxy(req: Request, path: string, method: "GET" | "POST"): Promise<Response> {
  const search = new URL(req.url).search;
  const url = `${API_BASE_URL}/api/${path}${search}`;
  const headers: Record<string, string> = authHeader(path);
  let body: string | undefined;
  if (method === "POST") {
    body = await req.text();
    if (body) headers["Content-Type"] = req.headers.get("content-type") ?? "application/json";
  }
  let res: Response;
  try {
    res = await fetch(url, { method, headers, body, cache: "no-store" });
  } catch {
    return NextResponse.json(
      { error: `Meridian API unreachable: ${method} ${url} — is the backend running?` },
      { status: 502 },
    );
  }
  const text = await res.text();
  return new NextResponse(text, {
    status: res.status,
    headers: { "Content-Type": res.headers.get("content-type") ?? "application/json" },
  });
}

// ---------- mock mode: one registry entry per endpoint ----------

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

/** Mutations accepted by the mock API. State does not persist in mock mode —
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

export async function GET(req: Request, ctx: Ctx) {
  const { endpoint } = await ctx.params;
  const path = endpoint.join("/");
  if (USE_API) return proxy(req, path, "GET");
  const m = match(path, Object.keys(GET_ROUTES));
  if (!m) return NextResponse.json({ error: `Unknown endpoint: GET /api/${path}` }, { status: 404 });
  const result = await GET_ROUTES[m.pattern](m.params);
  if (result === undefined) return NextResponse.json({ error: "Not found" }, { status: 404 });
  return NextResponse.json(result);
}

export async function POST(req: Request, ctx: Ctx) {
  const { endpoint } = await ctx.params;
  const path = endpoint.join("/");
  if (USE_API) return proxy(req, path, "POST");
  const m = match(path, POST_ROUTES);
  if (!m) return NextResponse.json({ error: `Unknown endpoint: POST /api/${path}` }, { status: 404 });
  const body = await req.json().catch(() => ({}));
  // Mock: acknowledge without persisting. The real backend enforces RBAC and
  // appends an audit event for every mutation (docs/BUSINESS_RULES.md).
  return NextResponse.json({ ok: true, endpoint: m.pattern, received: body, auditEvent: "mock-not-persisted" });
}
