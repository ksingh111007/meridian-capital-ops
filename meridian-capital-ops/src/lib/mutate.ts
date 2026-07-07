/**
 * Client-safe mutation helper for the "use client" screens. POSTs to the
 * /api proxy (src/app/api/[...endpoint]/route.ts) and reports whether the
 * backend accepted the mutation, so screens only apply their optimistic
 * local-state update + success toast on ok and show an error toast otherwise.
 *
 * Must stay importable from client components — no imports from data.ts or
 * anything server-only.
 */
export async function postJson(path: string, body?: unknown): Promise<{ ok: boolean; error?: string }> {
  let res: Response;
  try {
    res = await fetch(`/api/${path}`, {
      method: "POST",
      ...(body !== undefined && {
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(body),
      }),
    });
  } catch {
    return { ok: false, error: "Network error — the request never reached the server." };
  }
  if (res.ok) return { ok: true };
  // Backend errors are RFC 7807 ProblemDetails ({ detail, title }); the proxy
  // itself returns { error } when the backend is unreachable.
  const payload = (await res.json().catch(() => null)) as
    | { detail?: string; title?: string; error?: string }
    | null;
  return {
    ok: false,
    error: payload?.detail || payload?.error || payload?.title || `Request failed (HTTP ${res.status})`,
  };
}
