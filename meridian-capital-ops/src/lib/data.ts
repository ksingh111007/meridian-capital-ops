/**
 * Data-access layer. Every function corresponds to one API endpoint
 * (see docs/API.md). By default (DATA_SOURCE=api) each function fetches the
 * real backend via src/lib/api.ts; with DATA_SOURCE=mock it returns the JSON
 * mock in src/mocks/ exactly as before (kept in the repo for testing).
 *
 * Screens only ever import from this module (server components call it
 * directly; the /api proxy routes wrap the same backend for client mutations).
 */
import type {
  AttentionItem, AuditEvent, Capability, CapitalCall, CashPosition, CurrentUser, Deal,
  DealDetail, Distribution, Drawdown, EscalationRule, Fund, Integration, Investor,
  InvestorAccessConfig, LegalEntity, Module, NotificationRule, PortalAccount,
  PortalActivityRow, PortalDocument, PortalIrInfo, PortalSession, PortalTaxDocument,
  PortfolioSummary, ReconItem, Role, ShareClass, StaffUser, Wire, WorkflowStage,
} from "./types";
import { MODULES } from "./types";
import { apiFind, apiGet, USE_API } from "./api";

import portfolioSummary from "@/mocks/portfolio-summary.json";
import dealsJson from "@/mocks/deals.json";
import dealDetailsJson from "@/mocks/deal-details.json";
import capitalCallsJson from "@/mocks/capital-calls.json";
import workflowsJson from "@/mocks/workflows.json";
import distributionsJson from "@/mocks/distributions.json";
import drawdownsJson from "@/mocks/drawdowns.json";
import wiresJson from "@/mocks/wires.json";
import cashPositionJson from "@/mocks/cash-position.json";
import reconciliationJson from "@/mocks/reconciliation.json";
import fundsJson from "@/mocks/funds.json";
import investorsJson from "@/mocks/investors.json";
import usersJson from "@/mocks/users.json";
import referenceDataJson from "@/mocks/reference-data.json";
import integrationsJson from "@/mocks/integrations.json";
import notificationRulesJson from "@/mocks/notification-rules.json";
import auditLogJson from "@/mocks/audit-log.json";
import investorAccessJson from "@/mocks/investor-access.json";
import needsAttentionJson from "@/mocks/needs-attention.json";
import meJson from "@/mocks/me.json";
import portalAccountJson from "@/mocks/portal-account.json";
import portalInvestmentsJson from "@/mocks/portal-investments.json";
import portalActivityJson from "@/mocks/portal-activity.json";
import portalStatementsJson from "@/mocks/portal-statements.json";
import portalTaxJson from "@/mocks/portal-tax.json";
import portalContactJson from "@/mocks/portal-contact.json";

// ---------- session ----------

export async function getCurrentUser(): Promise<CurrentUser> {
  if (USE_API) return apiGet<CurrentUser>("me");
  // Mock mode: me.json carries no capability matrix — join its role against users.json roles.
  const capabilities =
    (usersJson.roles as Role[]).find((r) => r.name === meJson.role)?.capabilities ??
    (Object.fromEntries(MODULES.map((m) => [m, "none"])) as Record<Module, Capability>);
  return { ...meJson, capabilities };
}

// ---------- portfolio ----------

export async function getPortfolioSummary(): Promise<PortfolioSummary> {
  return USE_API ? apiGet<PortfolioSummary>("portfolio/summary") : (portfolioSummary as PortfolioSummary);
}

export async function getDeals(): Promise<Deal[]> {
  return USE_API ? apiGet<Deal[]>("deals") : (dealsJson.deals as Deal[]);
}

export async function getDeal(id: string): Promise<DealDetail | undefined> {
  if (USE_API) return apiFind<DealDetail>(`deals/${id}`); // backend returns the full merged DealDetail
  const base = (dealsJson.deals as Deal[]).find((d) => d.id === id);
  const detail = (dealDetailsJson as Record<string, Omit<DealDetail, keyof Deal>>)[id];
  return base && detail ? { ...base, ...detail } : undefined;
}

// ---------- capital calls ----------

export async function getCapitalCalls(): Promise<CapitalCall[]> {
  return USE_API ? apiGet<CapitalCall[]>("capital-calls") : (capitalCallsJson.calls as CapitalCall[]);
}

export async function getCapitalCall(id: string): Promise<CapitalCall | undefined> {
  if (USE_API) return apiFind<CapitalCall>(`capital-calls/${id}`);
  return (capitalCallsJson.calls as CapitalCall[]).find((c) => c.id === id);
}

export async function getWorkflow(): Promise<{ workflowName: string; stages: WorkflowStage[]; escalationRules: EscalationRule[] }> {
  type WorkflowPayload = { workflowName: string; stages: WorkflowStage[]; escalationRules: EscalationRule[] };
  return USE_API ? apiGet<WorkflowPayload>("workflows/capital-calls") : (workflowsJson as WorkflowPayload);
}

// ---------- distributions & fund ops ----------

export async function getDistributions(): Promise<Distribution[]> {
  return USE_API ? apiGet<Distribution[]>("distributions") : (distributionsJson.distributions as Distribution[]);
}

export async function getDistribution(id: string): Promise<Distribution | undefined> {
  if (USE_API) return apiFind<Distribution>(`distributions/${id}`);
  return (distributionsJson.distributions as Distribution[]).find((d) => d.id === id);
}

export async function getDrawdowns(): Promise<{ kpis: typeof drawdownsJson.kpis; drawdowns: Drawdown[] }> {
  type Payload = { kpis: typeof drawdownsJson.kpis; drawdowns: Drawdown[] };
  return USE_API ? apiGet<Payload>("drawdowns") : (drawdownsJson as Payload);
}

export async function getWires(): Promise<{ asOf: string; kpis: typeof wiresJson.kpis; wires: Wire[] }> {
  type Payload = { asOf: string; kpis: typeof wiresJson.kpis; wires: Wire[] };
  return USE_API ? apiGet<Payload>("wires") : (wiresJson as Payload);
}

export async function getCashPosition(): Promise<CashPosition> {
  return USE_API ? apiGet<CashPosition>("cash/position") : (cashPositionJson as CashPosition);
}

export async function getReconciliation(): Promise<{ asOf: string; source: string; kpis: typeof reconciliationJson.kpis; items: ReconItem[] }> {
  type Payload = { asOf: string; source: string; kpis: typeof reconciliationJson.kpis; items: ReconItem[] };
  return USE_API ? apiGet<Payload>("reconciliation") : (reconciliationJson as Payload);
}

// ---------- funds / investors / reference ----------

export async function getFunds(): Promise<{ kpis: typeof fundsJson.kpis; funds: Fund[]; entities: LegalEntity[]; shareClasses: ShareClass[] }> {
  type Payload = { kpis: typeof fundsJson.kpis; funds: Fund[]; entities: LegalEntity[]; shareClasses: ShareClass[] };
  return USE_API ? apiGet<Payload>("admin/funds") : (fundsJson as Payload);
}

export async function getFund(fundId: string): Promise<Fund | undefined> {
  return (await getFunds()).funds.find((f) => f.id === fundId);
}

export async function getInvestors(): Promise<{ kpis: typeof investorsJson.kpis; investors: Investor[] }> {
  type Payload = { kpis: typeof investorsJson.kpis; investors: Investor[] };
  return USE_API ? apiGet<Payload>("admin/investors") : (investorsJson as Payload);
}

export async function getReferenceData(): Promise<typeof referenceDataJson> {
  return USE_API ? apiGet<typeof referenceDataJson>("admin/reference") : referenceDataJson;
}

// ---------- admin ----------

export async function getUsersAndRoles(): Promise<{ kpis: typeof usersJson.kpis; users: StaffUser[]; roles: Role[] }> {
  type Payload = { kpis: typeof usersJson.kpis; users: StaffUser[]; roles: Role[] };
  return USE_API ? apiGet<Payload>("admin/users") : (usersJson as Payload);
}

export async function getIntegrations(): Promise<{ kpis: typeof integrationsJson.kpis; integrations: Integration[] }> {
  type Payload = { kpis: typeof integrationsJson.kpis; integrations: Integration[] };
  return USE_API ? apiGet<Payload>("admin/integrations") : (integrationsJson as Payload);
}

export async function getNotificationRules(): Promise<{ rules: NotificationRule[]; channels: typeof notificationRulesJson.channels }> {
  type Payload = { rules: NotificationRule[]; channels: typeof notificationRulesJson.channels };
  return USE_API ? apiGet<Payload>("admin/notification-rules") : (notificationRulesJson as Payload);
}

export async function getAuditLog(): Promise<{ kpis: typeof auditLogJson.kpis; events: AuditEvent[] }> {
  type Payload = { kpis: typeof auditLogJson.kpis; events: AuditEvent[] };
  return USE_API ? apiGet<Payload>("admin/audit") : (auditLogJson as Payload);
}

export async function getInvestorAccess(): Promise<InvestorAccessConfig> {
  return USE_API ? apiGet<InvestorAccessConfig>("admin/investor-access") : (investorAccessJson as InvestorAccessConfig);
}

// ---------- needs attention (ops inbox) ----------

export async function getNeedsAttention(): Promise<AttentionItem[]> {
  return USE_API ? apiGet<AttentionItem[]>("needs-attention") : (needsAttentionJson.items as AttentionItem[]);
}

// ---------- investor portal (scoped to the authenticated LP) ----------

export async function getPortalAccount(): Promise<PortalAccount> {
  return USE_API ? apiGet<PortalAccount>("portal/account") : (portalAccountJson as PortalAccount);
}

/** Lightweight identity for the portal shell — allowed for every contact role (incl. Tax-only). */
export async function getPortalSession(): Promise<PortalSession> {
  if (USE_API) return apiGet<PortalSession>("portal/session");
  const account = portalAccountJson as PortalAccount;
  return {
    contactId: "pc-1",
    contactName: account.contactName,
    contactInitials: account.contactInitials,
    investorId: account.investorId,
    investor: account.investor,
    role: "Primary",
  };
}

export async function getPortalInvestments(): Promise<typeof portalInvestmentsJson> {
  return USE_API ? apiGet<typeof portalInvestmentsJson>("portal/investments") : portalInvestmentsJson;
}

export async function getPortalActivity(): Promise<{ stats: typeof portalActivityJson.stats; rows: PortalActivityRow[] }> {
  type Payload = { stats: typeof portalActivityJson.stats; rows: PortalActivityRow[] };
  return USE_API ? apiGet<Payload>("portal/activity") : (portalActivityJson as Payload);
}

export async function getPortalStatements(): Promise<{ totalCount: number; documents: PortalDocument[] }> {
  type Payload = { totalCount: number; documents: PortalDocument[] };
  return USE_API ? apiGet<Payload>("portal/statements") : (portalStatementsJson as Payload);
}

export async function getPortalTax(): Promise<{ banner: typeof portalTaxJson.banner; documents: PortalTaxDocument[] }> {
  type Payload = { banner: typeof portalTaxJson.banner; documents: PortalTaxDocument[] };
  return USE_API ? apiGet<Payload>("portal/tax") : (portalTaxJson as Payload);
}

export async function getPortalIrInfo(): Promise<PortalIrInfo & { regardingOptions: string[] }> {
  type Payload = PortalIrInfo & { regardingOptions: string[] };
  return USE_API ? apiGet<Payload>("portal/contact") : (portalContactJson as Payload);
}
