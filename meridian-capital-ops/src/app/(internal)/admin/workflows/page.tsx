import type { Metadata } from "next";
import { getWorkflow } from "@/lib/data";
import { WorkflowsScreen } from "@/screens/admin/WorkflowsScreen";

export const metadata: Metadata = { title: "Approval Workflows" };

/** Screen 5b — the editable DD pipeline behind the capital-calls detail. */
export default function WorkflowsPage() {
  const { workflowName, stages, escalationRules } = getWorkflow();
  return <WorkflowsScreen workflowName={workflowName} stages={stages} escalationRules={escalationRules} />;
}
