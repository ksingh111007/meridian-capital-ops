import { NextResponse } from "next/server";

// Liveness probe for the App Service health check (see ../infra). Deliberately
// does not call the backend: it answers "is this container serving requests",
// not "is the whole stack up".
export function GET() {
  return NextResponse.json({ status: "ok" });
}
