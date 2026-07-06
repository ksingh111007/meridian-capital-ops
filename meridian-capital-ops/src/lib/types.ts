/**
 * Domain types for Meridian Capital Ops.
 *
 * These mirror the entities in docs/DATA_MODEL.md and are the contract the
 * future backend must satisfy. All money amounts are USD millions unless a
 * field name says otherwise (documented per field).
 */

// ---------- shared ----------

export type FundId = "fund-i" | "fund-ii" | "fund-iii";

export type PillTone = "neutral" | "amber" | "green" | "blue" | "red";

// ---------- funds & structure ----------

export interface Fund {
  id: FundId;
  name: string;
  shortName: string; // "Fund III"
  vintage: number;
  committed: number;
  calledPct: number; // 0–100
  strategy: string;
  waterfallType: "European" | "American";
  baseCurrency: string;
  status: "Active" | "Investing" | "Harvesting";
}

export interface LegalEntity {
  fundId: FundId;
  name: string;
  kind: string; // GP | Master | Feeder | Cayman | Blocker
}

export interface ShareClass {
  fundId: FundId;
  name: string;
  mgmtFeePct: number;
  carryPct: number;
  prefPct: number;
}

// ---------- investors ----------

export interface InvestorCommitment {
  fundId: FundId;
  amount: number;
  called: number; // amount called to date; unfunded = amount - called
}

export interface Investor {
  id: string;
  name: string;
  type: string;
  commitments: InvestorCommitment[];
  kycStatus: "Verified" | "In review";
  wireInstructionsOnFile: boolean;
  profile?: {
    bank: string;
    abaMasked: string;
    accountMasked: string;
    bankingVerified: string; // "Jun 2026"
    kycDocs: string;
    kycReviewDue: string;
  };
}

// ---------- deals / portfolio ----------

export type DealStatus = "Performing" | "Watch" | "Non-accrual";

export interface Deal {
  id: string;
  name: string; // "Project Beacon"
  borrower: string;
  sector: string;
  country: string;
  fundId: FundId;
  tranche: string;
  invested: number;
  outstanding: number;
  spread: string; // "S+2.50%"
  netIrrPct: number;
  irrTrend: "up" | "down" | "flat";
  moic: number;
  status: DealStatus;
}

export interface DealDetail extends Deal {
  fairValue: number;
  facility: number;
  drawn: number;
  maturity: string;
  spreadFloor: string; // "SOFR + 2.50% · 1.00%"
  upfrontFeePct: number;
  cashflows: { date: string; type: string; amount: number; principalBalance: number }[];
  risk: {
    internalRating: string;
    trend: string;
    covenants: string;
    netLeverage: string;
    lastReview: string;
  };
  lpExposure: { investor: string; amount: number }[];
  documents: string[];
}

export interface PortfolioSummary {
  asOf: string;
  investedCapital: number;
  activeDeals: number;
  netIrrPct: number;
  blendedMoic: number;
  onWatchCount: number;
  onWatchExposure: number;
  valueTrend: number[]; // 8 quarters, relative heights 0–100
  exposureMix: { performingPct: number; watchPct: number; nonAccrualPct: number };
}

// ---------- capital calls ----------

export type WireStatus =
  | "Pending"
  | "Scheduled"
  | "Wired"
  | "Confirmed"
  | "Overdue";

export type CallStatus = "In Review" | "Pending" | "Returned" | "Completed";

export interface CallAllocation {
  investorId: string;
  investor: string;
  commitment: number; // LP's commitment in the call's fund
  amount: number;
  wireStatus: WireStatus;
}

export interface StageEvent {
  stage: number; // 1-based stage order
  state: "done" | "current" | "pending";
  actor?: string;
  date?: string; // "Jul 02"
  note?: string; // status note for the current stage, e.g. "In review"
  comment?: string;
}

export interface CapitalCall {
  id: string;
  ref: string; // "#C-2041"
  dealId: string;
  deal: string;
  fundId: FundId;
  tranche: string;
  borrower: string;
  amount: number;
  dueDate: string; // ISO date
  currentStage: number; // 1–9
  status: CallStatus;
  allocations: CallAllocation[];
  stageEvents: StageEvent[];
  documents: { name: string; by: string; date: string }[];
  audit: { title: string; by: string; at: string; comment?: string; tone: PillTone }[];
}

export interface WorkflowStage {
  order: number;
  name: string;
  approverRole: string; // "System" for automated stages
  slaDays: number | null; // null = auto / terminal
  autoAdvance: boolean;
  required: boolean;
  terminal?: boolean;
}

export interface EscalationRule {
  condition: string;
  effect: string;
  enabled: boolean;
}

// ---------- distributions ----------

export interface WaterfallTier {
  tier: string; // "1 · Return of Capital"
  basis: string;
  rate: string;
  distributed: number;
  lpShare: number | null;
  gpShare: number | null;
  poolLeft: number;
}

export type PayoutStatus =
  | "Scheduled"
  | "Sent"
  | "Paid"
  | "Exception"
  | "Blocked";

export interface InvestorPayout {
  investorId: string;
  investor: string;
  commitment: number;
  amount: number;
  pctOfLpTotal: number;
  status: PayoutStatus;
  blockedReason?: string; // e.g. "No wire instructions on file"
  wireRef?: string;
}

export interface Distribution {
  id: string;
  ref: string; // "#D-118"
  fundId: FundId;
  distributable: number;
  lpTotal: number;
  gpTotal: number;
  paymentDate: string;
  status: "Scheduled" | "Paying" | "Paid";
  waterfallType: "European" | "American";
  sourceNote: string; // "Loan repayments + interest"
  tiers: WaterfallTier[];
  payouts: InvestorPayout[];
}

// ---------- fund ops ----------

export interface Drawdown {
  id: string;
  facility: string;
  lender: string;
  purpose: string; // "Bridge — Project Atlas"
  dealId?: string;
  linkedCallId?: string;
  amount: number;
  rate: string;
  drawDate: string;
  repayBy: string | null;
  status: "Outstanding" | "Requested" | "Repaid";
}

export interface Wire {
  id: string;
  ref: string; // "W-8842"
  direction: "In" | "Out";
  counterparty: string;
  type: "Capital Call" | "Distribution" | "Facility Repay";
  linkedRef: string; // "#C-2041" / "#D-119"
  amount: number;
  time: string; // "09:02"
  date: string;
  rail: "Fedwire" | "ACH" | "SWIFT";
  status: "Queued" | "Sent" | "Acknowledged" | "Settled" | "Exception";
  exceptionReason?: string;
}

export interface CashPosition {
  asOf: string;
  fundId: FundId;
  cashOnHand: number;
  accountsCount: number;
  uncalledCapital: number;
  uncalledLps: number;
  facilityHeadroom: number;
  facilityLimit: number;
  net30DayProjection: number;
  forecastBars: number[]; // 13-week forecast, relative heights
  coverageRatio: number; // e.g. 1.4
  weeks: { label: string; inflows: number; outflows: number; net: number; projectedBalance: number }[];
  accounts: { custodian: string; account: string; currency: string; type: string; balance: number }[];
}

export interface ReconItem {
  id: string;
  date: string;
  description: string;
  source: string;
  book: number | null;
  custodian: number | null;
  diff: number;
  status: "Matched" | "Break" | "Unmatched";
  assignee?: string;
}

// ---------- admin ----------

export type Capability = "none" | "view" | "edit" | "approve" | "full";
export const MODULES = ["Blotter", "Approvals", "Wires", "Recon", "Ref Data", "Admin"] as const;
export type Module = (typeof MODULES)[number];

export interface StaffUser {
  id: string;
  name: string;
  initials: string;
  email: string;
  role: string;
  fundAccess: string;
  lastActive: string;
  status: "Active" | "Invited";
}

export interface Role {
  name: string;
  capabilities: Record<Module, Capability>;
}

export interface Integration {
  name: string;
  type: string;
  direction: "Inbound" | "Outbound" | "Two-way";
  lastSync: string;
  status: "Connected" | "Warning" | "Error";
  warning?: string;
}

export interface NotificationRule {
  id: string;
  name: string;
  trigger: string;
  channel: string;
  recipients: string;
  enabled: boolean;
}

export interface AuditEvent {
  time: string;
  actor: string;
  action: string;
  tone: PillTone;
  object: string;
  detail: string;
  seal: string;
}

export interface PortalContact {
  id: string;
  name: string; // display name or email if not yet accepted
  initials: string;
  investorId: string;
  investor: string;
  role: "Primary" | "Viewer" | "Tax-only";
  fundsVisible: string;
  statements: string; // "✓" | "Tax only" | "—" semantics: full/tax/none
  status: "Active" | "Invited" | "Disabled";
}

export interface InvestorAccessConfig {
  kpis: { portalUsers: number; active: number; pendingInvites: number; investorsWithAccess: string; notEnrolled: number };
  contacts: PortalContact[];
  capabilities: { label: string; enabled: boolean }[];
  documentTypes: { label: string; exposed: boolean }[];
}

// ---------- needs attention (ops inbox) ----------

export interface AttentionItem {
  id: string;
  kind: "approval" | "overdue" | "exception" | "break" | "due-soon" | "warning";
  title: string;
  detail: string;
  href: string;
  tone: PillTone;
  mine?: boolean; // true when the current user is the required actor
}

// ---------- investor portal (external, scoped to logged-in LP) ----------

export interface PortalAccount {
  investorId: string;
  investor: string;
  contactName: string;
  contactInitials: string;
  asOf: string;
  stats: { commitment: number; paidIn: number; distributions: number; nav: number; netIrrPct: number };
  funds: {
    fundId: FundId;
    name: string;
    vintage: number;
    commitment: number;
    nav: number;
    netIrrPct: number;
    dpi: number;
    calledPct: number;
    calledAmount: number;
  }[];
}

export interface PortalActivityRow {
  date: string;
  fund: string;
  type: "Capital Call" | "Distribution";
  reference: string;
  amount: number; // negative = call (outflow for LP)
  status: "Due" | "Funded" | "Paid" | "Processing";
}

export interface PortalDocument {
  id: string;
  name: string;
  fund: string;
  period: string;
  type: "Capital account" | "Notice" | "Tax" | "Report";
  date: string;
}

export interface PortalTaxDocument {
  id: string;
  name: string;
  fund: string;
  taxYear: number;
  type: string;
  status: "Available" | "Pending";
  expectedDate?: string;
}

export interface PortalIrInfo {
  manager: { name: string; initials: string; title: string };
  email: string;
  phone: string;
  hours: string;
  recentRequests: { subject: string; ref: string; date: string; status: string }[];
}

export interface CurrentUser {
  id: string;
  name: string;
  initials: string;
  role: string;
}
