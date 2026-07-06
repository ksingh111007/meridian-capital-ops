import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { getCapitalCall, getCapitalCalls, getCurrentUser, getUsersAndRoles, getWorkflow } from "@/lib/data";
import { CallDetailScreen } from "@/screens/CallDetailScreen";

export async function generateMetadata({ params }: { params: Promise<{ callId: string }> }): Promise<Metadata> {
  const { callId } = await params;
  const call = getCapitalCall(callId);
  return { title: call ? `${call.deal} · Call ${call.ref}` : "Capital Call" };
}

export function generateStaticParams() {
  return getCapitalCalls().map((c) => ({ callId: c.id }));
}

/** Screen 2b — row detail: major details, 9-stage DD pipeline, approve/reject, docs, allocations, audit. */
export default async function CallDetailPage({ params }: { params: Promise<{ callId: string }> }) {
  const { callId } = await params;
  const call = getCapitalCall(callId);
  if (!call) notFound();
  const user = getCurrentUser();
  const { roles } = getUsersAndRoles();
  const canApprove = roles.find((r) => r.name === user.role)?.capabilities.Approvals === "approve" ||
    roles.find((r) => r.name === user.role)?.capabilities.Approvals === "full";
  return <CallDetailScreen call={call} stages={getWorkflow().stages} user={user} userCanApprove={canApprove} />;
}
