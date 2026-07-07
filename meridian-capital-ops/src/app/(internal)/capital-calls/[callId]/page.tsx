import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { getCapitalCall, getCurrentUser, getWorkflow } from "@/lib/data";
import { CallDetailScreen } from "@/screens/CallDetailScreen";

export async function generateMetadata({ params }: { params: Promise<{ callId: string }> }): Promise<Metadata> {
  const { callId } = await params;
  const call = await getCapitalCall(callId);
  return { title: call ? `${call.deal} · Call ${call.ref}` : "Capital Call" };
}

/** Screen 2b — row detail: major details, 9-stage DD pipeline, approve/reject, docs, allocations, audit. */
export default async function CallDetailPage({ params }: { params: Promise<{ callId: string }> }) {
  const { callId } = await params;
  const [call, user, workflow] = await Promise.all([
    getCapitalCall(callId), getCurrentUser(), getWorkflow(),
  ]);
  if (!call) notFound();
  const canApprove = user.capabilities.Approvals === "approve" || user.capabilities.Approvals === "full";
  return <CallDetailScreen call={call} stages={workflow.stages} user={user} userCanApprove={canApprove} />;
}
