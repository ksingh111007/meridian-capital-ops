/**
 * Server-side fetch helper for the real backend (backend/ — see docs/API.md).
 *
 * Configuration (all optional, sensible dev defaults):
 * - MERIDIAN_API_URL          backend base URL          (default http://localhost:8080)
 * - MERIDIAN_API_USER         staff user id header      (default u-admin — full capability matrix)
 * - MERIDIAN_PORTAL_CONTACT   portal contact id header  (default pc-1 — Karen Doyle, Redwood Pension)
 * - DATA_SOURCE               "api" (default) | "mock"  — "mock" keeps the JSON imports in data.ts
 *
 * The backend authenticates via a dev header: `X-User-Id: <staff user id>` for
 * internal endpoints and `X-User-Id: <portal contact id>` for /api/portal/*.
 * All fetches are `cache: "no-store"` so pages render at request time.
 */

export const API_BASE_URL = process.env.MERIDIAN_API_URL ?? "http://localhost:8080";
export const STAFF_USER_ID = process.env.MERIDIAN_API_USER ?? "u-admin";
export const PORTAL_CONTACT_ID = process.env.MERIDIAN_PORTAL_CONTACT ?? "pc-1";

/** True when the data layer should hit the real backend (DATA_SOURCE=api, the default). */
export const USE_API = (process.env.DATA_SOURCE ?? "api") !== "mock";

/** The dev-auth header for a given API path (portal/* endpoints act as the LP contact). */
export function authHeader(path: string): Record<string, string> {
  const isPortal = path === "portal" || path.startsWith("portal/");
  return { "X-User-Id": isPortal ? PORTAL_CONTACT_ID : STAFF_USER_ID };
}

async function request(path: string): Promise<Response> {
  const url = `${API_BASE_URL}/api/${path}`;
  let res: Response;
  try {
    res = await fetch(url, { cache: "no-store", headers: authHeader(path) });
  } catch (cause) {
    throw new Error(
      `Meridian API unreachable: GET ${url} — is the backend running? ` +
        `(MERIDIAN_API_URL=${API_BASE_URL}, or set DATA_SOURCE=mock)`,
      { cause },
    );
  }
  return res;
}

/** GET an API endpoint; throws a descriptive Error on any non-OK response. */
export async function apiGet<T>(path: string): Promise<T> {
  const res = await request(path);
  if (!res.ok) {
    const body = await res.text().catch(() => "");
    throw new Error(`Meridian API error: GET ${API_BASE_URL}/api/${path} → ${res.status} ${res.statusText}${body ? ` — ${body.slice(0, 300)}` : ""}`);
  }
  return (await res.json()) as T;
}

/** GET an API endpoint; returns undefined on 404 (entity lookups), throws on other errors. */
export async function apiFind<T>(path: string): Promise<T | undefined> {
  const res = await request(path);
  if (res.status === 404) return undefined;
  if (!res.ok) {
    const body = await res.text().catch(() => "");
    throw new Error(`Meridian API error: GET ${API_BASE_URL}/api/${path} → ${res.status} ${res.statusText}${body ? ` — ${body.slice(0, 300)}` : ""}`);
  }
  return (await res.json()) as T;
}
