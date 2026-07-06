/**
 * Data-access layer. Every function corresponds to one API endpoint
 * (see docs/API.md) and is backed by one JSON mock in src/mocks/.
 *
 * BACKEND HANDOFF: replace the JSON imports with real fetches — the function
 * signatures and return types are the contract. Screens only ever import from
 * this module (server components call it directly; the /api routes wrap it).
 */
import type {
  AttentionItem, AuditEvent, CapitalCall, CashPosition, CurrentUser, Deal, DealDetail,
  Distribution, Drawdown, EscalationRule, Fund, Integration, Investor, InvestorAccessConfig,
  LegalEntity, NotificationRule, PortalAccount, PortalActivityRow, PortalDocument,
  PortalIrInfo, PortalTaxDocument, PortfolioSummary, ReconItem, Role, ShareClass,
  StaffUser, Wire, WorkflowStage,
} from "./types";

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

export function getCurrentUser(): CurrentUser {
  return meJson as CurrentUser;
}

// ---------- portfolio ----------

export function getPortfolioSummary(): PortfolioSummary {
  return portfolioSummary as PortfolioSummary;
}

export function getDeals(): Deal[] {
  return dealsJson.deals as Deal[];
}

export function getDeal(id: string): DealDetail | undefined {
  const base = getDeals().find((d) => d.id === id);
  const detail = (dealDetailsJson as Record<string, Omit<DealDetail, keyof Deal>>)[id];
  return base && detail ? { ...base, ...detail } : undefined;
}

// ---------- capital calls ----------

export function getCapitalCalls(): CapitalCall[] {
  return capitalCallsJson.calls as CapitalCall[];
}

export function getCapitalCall(id: string): CapitalCall | undefined {
  return getCapitalCalls().find((c) => c.id === id);
}

export function getWorkflow(): { workflowName: string; stages: WorkflowStage[]; escalationRules: EscalationRule[] } {
  return workflowsJson as { workflowName: string; stages: WorkflowStage[]; escalationRules: EscalationRule[] };
}

// ---------- distributions & fund ops ----------

export function getDistributions(): Distribution[] {
  return distributionsJson.distributions as Distribution[];
}

export function getDistribution(id: string): Distribution | undefined {
  return getDistributions().find((d) => d.id === id);
}

export function getDrawdowns(): { kpis: typeof drawdownsJson.kpis; drawdowns: Drawdown[] } {
  return drawdownsJson as { kpis: typeof drawdownsJson.kpis; drawdowns: Drawdown[] };
}

export function getWires(): { asOf: string; kpis: typeof wiresJson.kpis; wires: Wire[] } {
  return wiresJson as { asOf: string; kpis: typeof wiresJson.kpis; wires: Wire[] };
}

export function getCashPosition(): CashPosition {
  return cashPositionJson as CashPosition;
}

export function getReconciliation(): { asOf: string; source: string; kpis: typeof reconciliationJson.kpis; items: ReconItem[] } {
  return reconciliationJson as { asOf: string; source: string; kpis: typeof reconciliationJson.kpis; items: ReconItem[] };
}

// ---------- funds / investors / reference ----------

export function getFunds(): { kpis: typeof fundsJson.kpis; funds: Fund[]; entities: LegalEntity[]; shareClasses: ShareClass[] } {
  return fundsJson as { kpis: typeof fundsJson.kpis; funds: Fund[]; entities: LegalEntity[]; shareClasses: ShareClass[] };
}

export function getFund(fundId: string): Fund | undefined {
  return getFunds().funds.find((f) => f.id === fundId);
}

export function getInvestors(): { kpis: typeof investorsJson.kpis; investors: Investor[] } {
  return investorsJson as { kpis: typeof investorsJson.kpis; investors: Investor[] };
}

export function getReferenceData() {
  return referenceDataJson;
}

// ---------- admin ----------

export function getUsersAndRoles(): { kpis: typeof usersJson.kpis; users: StaffUser[]; roles: Role[] } {
  return usersJson as { kpis: typeof usersJson.kpis; users: StaffUser[]; roles: Role[] };
}

export function getIntegrations(): { kpis: typeof integrationsJson.kpis; integrations: Integration[] } {
  return integrationsJson as { kpis: typeof integrationsJson.kpis; integrations: Integration[] };
}

export function getNotificationRules(): { rules: NotificationRule[]; channels: typeof notificationRulesJson.channels } {
  return notificationRulesJson as { rules: NotificationRule[]; channels: typeof notificationRulesJson.channels };
}

export function getAuditLog(): { kpis: typeof auditLogJson.kpis; events: AuditEvent[] } {
  return auditLogJson as { kpis: typeof auditLogJson.kpis; events: AuditEvent[] };
}

export function getInvestorAccess(): InvestorAccessConfig {
  return investorAccessJson as InvestorAccessConfig;
}

// ---------- needs attention (ops inbox) ----------

export function getNeedsAttention(): AttentionItem[] {
  return needsAttentionJson.items as AttentionItem[];
}

// ---------- investor portal (scoped to the authenticated LP) ----------

export function getPortalAccount(): PortalAccount {
  return portalAccountJson as PortalAccount;
}

export function getPortalInvestments() {
  return portalInvestmentsJson;
}

export function getPortalActivity(): { stats: typeof portalActivityJson.stats; rows: PortalActivityRow[] } {
  return portalActivityJson as { stats: typeof portalActivityJson.stats; rows: PortalActivityRow[] };
}

export function getPortalStatements(): { totalCount: number; documents: PortalDocument[] } {
  return portalStatementsJson as { totalCount: number; documents: PortalDocument[] };
}

export function getPortalTax(): { banner: typeof portalTaxJson.banner; documents: PortalTaxDocument[] } {
  return portalTaxJson as { banner: typeof portalTaxJson.banner; documents: PortalTaxDocument[] };
}

export function getPortalIrInfo(): PortalIrInfo & { regardingOptions: string[] } {
  return portalContactJson as PortalIrInfo & { regardingOptions: string[] };
}
