#!/usr/bin/env node
/**
 * Generates the post-deployment seed scripts from the frontend mock story.
 *
 *   node generate-seed.mjs        (run from database/tools)
 *
 * Reads meridian-capital-ops/src/mocks/*.json (the authoritative sample story —
 * the files stay in the repo and keep driving the frontend's mock mode) and the
 * generated table definitions under ../src (for exact column lists, so schema
 * drift fails generation instead of failing deployment), then writes
 * ../scripts/seed/*.sql plus ../scripts/Script.PostDeployment.sql.
 *
 * Everything is idempotent: each table only seeds when empty. Audit-log seals
 * are computed with the same SHA-256 chain as the backend's AuditSealer, so
 * GET /api/admin/audit reports chainValid against the seeded data.
 */

import { createHash } from "node:crypto";
import { mkdirSync, readFileSync, readdirSync, writeFileSync } from "node:fs";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const here = dirname(fileURLToPath(import.meta.url));
const databaseRoot = resolve(here, "..");
const mocksDir = resolve(databaseRoot, "..", "meridian-capital-ops", "src", "mocks");
const seedDir = join(databaseRoot, "scripts", "seed");

const mock = (name) => JSON.parse(readFileSync(join(mocksDir, `${name}.json`), "utf-8"));

// ---------- table metadata from the generated definitions ----------

const AUDIT_COLUMNS = new Set(["CreatedAtUtc", "CreatedBy", "IsActive", "ModifiedAtUtc", "ModifiedBy", "ValidFrom", "ValidTo"]);

/** schema.Table -> ordered insertable column names (identity + audit columns excluded). */
const tableColumns = new Map();
for (const schema of readdirSync(join(databaseRoot, "src"), { withFileTypes: true })) {
  if (!schema.isDirectory() || schema.name === "Schemas") continue;
  const tablesDir = join(databaseRoot, "src", schema.name, "Tables");
  for (const file of readdirSync(tablesDir)) {
    const sql = readFileSync(join(tablesDir, file), "utf-8");
    const columns = [];
    for (const line of sql.split("\n")) {
      const match = line.match(/^ {4}\[(\w+)\] \S+.*?(NOT NULL|NULL)/);
      if (!match) continue;
      const [, name] = match;
      if (AUDIT_COLUMNS.has(name) || line.includes("IDENTITY")) continue;
      columns.push(name);
    }
    tableColumns.set(`${schema.name}.${file.replace(".sql", "")}`, columns);
  }
}

// ---------- SQL rendering ----------

const quote = (value) => {
  if (value === null || value === undefined) return "NULL";
  if (typeof value === "number") return Number.isFinite(value) ? String(value) : fail(`non-finite number ${value}`);
  if (typeof value === "boolean") return value ? "1" : "0";
  if (typeof value === "object" && value.raw) return value.raw;
  return `N'${String(value).replaceAll("'", "''")}'`;
};

const fail = (message) => {
  throw new Error(message);
};

/** INSERT ... VALUES for every row, guarded so re-deployments don't duplicate. */
function insertBlock(qualified, rows) {
  if (rows.length === 0) return "";
  const [schema, table] = qualified.split(".");
  const columns = tableColumns.get(qualified) ?? fail(`Unknown table ${qualified}`);
  for (const row of rows) {
    const extra = Object.keys(row).filter((key) => !columns.includes(key));
    const missing = columns.filter((column) => !(column in row));
    if (extra.length || missing.length)
      fail(`${qualified}: extra [${extra}] missing [${missing}] — regenerate tables or fix the seed mapping`);
  }
  const target = `[${schema}].[${table}]`;
  const values = rows
    .map((row) => `    (${columns.map((column) => quote(row[column])).join(", ")})`)
    .join(",\n");
  return `IF NOT EXISTS (SELECT 1 FROM ${target})\nINSERT INTO ${target} (${columns.map((c) => `[${c}]`).join(", ")})\nVALUES\n${values};\n`;
}

const files = new Map();
const emit = (file, sql) => files.set(file, (files.get(file) ?? "") + sql + "\n");

// ---------- shared parsing helpers ----------

const MONTHS = { Jan: 1, Feb: 2, Mar: 3, Apr: 4, May: 5, Jun: 6, Jul: 7, Aug: 8, Sep: 9, Oct: 10, Nov: 11, Dec: 12 };
const STORY_YEAR = 2026;
const pad = (n) => String(n).padStart(2, "0");

/** "Jul 02" -> 2026-07-02 (mock display dates live in the story year). */
function shortDate(display) {
  const match = display.match(/^([A-Z][a-z]{2}) (\d{2})$/) ?? fail(`Unparseable short date '${display}'`);
  return `${STORY_YEAR}-${pad(MONTHS[match[1]])}-${pad(Number(match[2]))}`;
}

/** "Jul 05 09:41" -> 2026-07-05T09:41:00. */
function shortDateTime(display) {
  const match = display.match(/^([A-Z][a-z]{2}) (\d{2}) (\d{2}):(\d{2})$/) ?? fail(`Unparseable time '${display}'`);
  return `${STORY_YEAR}-${pad(MONTHS[match[1]])}-${pad(Number(match[2]))}T${match[3]}:${match[4]}:00`;
}

const CAPABILITY_LEVELS = { none: 0, view: 1, edit: 2, approve: 3, full: 4 };
const MODULE_KEYS = { Blotter: "Blotter", Approvals: "Approvals", Wires: "Wires", Recon: "Recon", "Ref Data": "RefData", Admin: "Admin" };

// ---------- 01: reference data ----------

const funds = mock("funds");
emit("01-reference", insertBlock("ref.Funds", funds.funds.map((f) => ({
  Id: f.id, Name: f.name, ShortName: f.shortName, Vintage: f.vintage, Committed: f.committed,
  CalledPct: f.calledPct, Strategy: f.strategy, WaterfallType: f.waterfallType,
  BaseCurrency: f.baseCurrency, Status: f.status,
}))));
emit("01-reference", insertBlock("ref.LegalEntities", funds.entities.map((e) => ({
  FundId: e.fundId, Name: e.name, Kind: e.kind,
}))));
emit("01-reference", insertBlock("ref.ShareClasses", funds.shareClasses.map((s) => ({
  FundId: s.fundId, Name: s.name, MgmtFeePct: s.mgmtFeePct, CarryPct: s.carryPct, PrefPct: s.prefPct,
}))));

const deals = mock("deals").deals;
emit("01-reference", insertBlock("ref.Deals", deals.map((d) => ({
  Id: d.id, Name: d.name, Borrower: d.borrower, Sector: d.sector, Country: d.country, FundId: d.fundId,
  Tranche: d.tranche, Invested: d.invested, Outstanding: d.outstanding, Spread: d.spread,
  NetIrrPct: d.netIrrPct, IrrTrend: d.irrTrend, Moic: d.moic, Status: d.status,
}))));

const dealDetails = Object.entries(mock("deal-details"));
emit("01-reference", insertBlock("ref.DealDetails", dealDetails.map(([dealId, d]) => ({
  DealId: dealId, FairValue: d.fairValue, Facility: d.facility, Drawn: d.drawn, Maturity: d.maturity,
  SpreadFloor: d.spreadFloor, UpfrontFeePct: d.upfrontFeePct, InternalRating: d.risk.internalRating,
  RiskTrend: d.risk.trend, Covenants: d.risk.covenants, NetLeverage: d.risk.netLeverage,
  LastReview: d.risk.lastReview,
}))));
emit("01-reference", insertBlock("ref.DealCashflows", dealDetails.flatMap(([dealId, d]) =>
  d.cashflows.map((c) => ({
    Date: c.date, Type: c.type, Amount: c.amount, PrincipalBalance: c.principalBalance, DealDetailDealId: dealId,
  })))));
emit("01-reference", insertBlock("ref.DealLpExposures", dealDetails.flatMap(([dealId, d]) =>
  d.lpExposure.map((x) => ({ Investor: x.investor, Amount: x.amount, DealDetailDealId: dealId })))));
emit("01-reference", insertBlock("ref.DealDocuments", dealDetails.flatMap(([dealId, d]) =>
  d.documents.map((name) => ({ Name: name, DealDetailDealId: dealId })))));

const investors = mock("investors").investors;
emit("01-reference", insertBlock("ref.Investors", investors.map((i) => ({
  Id: i.id, Name: i.name, Type: i.type, KycStatus: i.kycStatus, WireInstructionsOnFile: i.wireInstructionsOnFile,
}))));
emit("01-reference", insertBlock("ref.InvestorCommitments", investors.flatMap((i) =>
  i.commitments.map((c) => ({ FundId: c.fundId, Amount: c.amount, Called: c.called, InvestorId: i.id })))));
emit("01-reference", insertBlock("ref.InvestorProfiles", investors.filter((i) => i.profile).map((i) => ({
  InvestorId: i.id, Bank: i.profile.bank, AbaMasked: i.profile.abaMasked, AccountMasked: i.profile.accountMasked,
  BankingVerified: i.profile.bankingVerified, KycDocs: i.profile.kycDocs, KycReviewDue: i.profile.kycReviewDue,
}))));

const reference = mock("reference-data");
emit("01-reference", insertBlock("ref.Borrowers", reference.borrowers.map((b) => ({
  Name: b.name, Sector: b.sector, Country: b.country, DealName: b.deal, InternalRating: b.internalRating,
}))));
emit("01-reference", insertBlock("ref.CurrencyRates", reference.currencies.map((c) => ({
  Code: c.code, Rate: c.rate, Note: c.note,
}))));
emit("01-reference", insertBlock("ref.SettlementCalendars", reference.calendars.map((c) => ({
  Name: c.name, NextHoliday: c.nextHoliday,
}))));

// ---------- 02: staff, roles, platform config ----------

const users = mock("users");
emit("02-admin", insertBlock("admin.Roles", users.roles.map((role) => ({
  Name: role.name,
  // Same JSON shape the API's EF value converter writes: enum-name keys, numeric levels.
  Capabilities: JSON.stringify(Object.fromEntries(Object.entries(role.capabilities).map(
    ([module, level]) => [MODULE_KEYS[module] ?? fail(`Unknown module ${module}`), CAPABILITY_LEVELS[level]]))),
}))));
emit("02-admin", insertBlock("admin.StaffUsers", [
  ...users.users.map((u) => ({
    Id: u.id, Name: u.name, Initials: u.initials, Email: u.email, RoleName: u.role,
    FundAccess: u.fundAccess, LastActive: u.lastActive, Status: u.status,
  })),
  // Platform administrator (not part of the mock roster): the frontend's default
  // dev principal, holding the full capability matrix.
  {
    Id: "u-admin", Name: "Avery Whitman", Initials: "AW", Email: "avery.whitman@meridiancredit.com",
    RoleName: "Administrator", FundAccess: "All funds", LastActive: "—", Status: "Active",
  },
]));

const integrations = mock("integrations");
emit("02-admin", insertBlock("admin.Integrations", integrations.integrations.map((i) => ({
  Name: i.name, Type: i.type, Direction: i.direction, LastSync: i.lastSync, Status: i.status,
  Warning: i.warning ?? null,
}))));

const notificationRules = mock("notification-rules");
emit("02-admin", insertBlock("admin.NotificationRules", notificationRules.rules.map((r) => ({
  Id: r.id, Name: r.name, Trigger: r.trigger, Channel: r.channel, Recipients: r.recipients, Enabled: r.enabled,
}))));
emit("02-admin", insertBlock("admin.NotificationChannels", notificationRules.channels.map((c) => ({
  Name: c.name, Detail: c.detail, Connected: c.connected,
}))));

// ---------- 03: capital-call workflow configuration ----------

const workflows = mock("workflows");
emit("03-workflow", insertBlock("ops.WorkflowStages", workflows.stages.map((s) => ({
  Order: s.order, Name: s.name, ApproverRole: s.approverRole, SlaDays: s.slaDays,
  AutoAdvance: s.autoAdvance, Required: s.required, Terminal: s.terminal ?? false,
}))));

// The display rules in workflows.json, joined with the machine fields the
// engine needs (kind, threshold, injected roles) — same values as StorySeed.
const ESCALATION_MACHINE = [
  { Id: "esc-amount", Kind: "AmountThreshold", ThresholdAmount: 20, RequiredRoles: ["CIO", "Compliance"] },
  { Id: "esc-crossfund", Kind: "CrossFundAllocation", ThresholdAmount: null, RequiredRoles: ["Counsel"] },
  { Id: "esc-newbank", Kind: "NewBankAccount", ThresholdAmount: null, RequiredRoles: [] },
];
emit("03-workflow", insertBlock("ops.EscalationRules", workflows.escalationRules.map((rule, index) => ({
  Id: ESCALATION_MACHINE[index].Id, Kind: ESCALATION_MACHINE[index].Kind, Condition: rule.condition,
  Effect: rule.effect, Enabled: rule.enabled, ThresholdAmount: ESCALATION_MACHINE[index].ThresholdAmount,
  RequiredRoles: JSON.stringify(ESCALATION_MACHINE[index].RequiredRoles),
}))));

// ---------- 04: capital calls ----------

const CALL_STATUS = { "In Review": "InReview", Pending: "Pending", Returned: "Returned", Completed: "Completed" };
const STAGE_STATE = { done: "Done", current: "Current", pending: "Pending" };

const calls = mock("capital-calls").calls;
emit("04-capital-calls", insertBlock("ops.CapitalCalls", calls.map((c) => ({
  Id: c.id, Ref: c.ref, DealId: c.dealId, DealName: c.deal, FundId: c.fundId, Tranche: c.tranche,
  Borrower: c.borrower, Amount: c.amount, DueDate: c.dueDate, Basis: "Unfunded", CurrentStage: c.currentStage,
  Status: CALL_STATUS[c.status] ?? fail(`Unknown call status ${c.status}`),
  PendingEscalations: "[]", EscalationGateStage: null,
}))));
emit("04-capital-calls", insertBlock("ops.CallAllocations", calls.flatMap((c) => c.allocations.map((a) => ({
  InvestorId: a.investorId, InvestorName: a.investor, Commitment: a.commitment, Amount: a.amount,
  WireStatus: a.wireStatus, CapitalCallId: c.id,
})))));
emit("04-capital-calls", insertBlock("ops.CallStageEvents", calls.flatMap((c) => c.stageEvents.map((s) => ({
  Stage: s.stage, State: STAGE_STATE[s.state] ?? fail(`Unknown stage state ${s.state}`),
  Actor: s.actor ?? null, Date: s.date ? shortDate(s.date) : null, Note: s.note ?? null,
  Comment: s.comment ?? null, CapitalCallId: c.id,
})))));
emit("04-capital-calls", insertBlock("ops.CallDocuments", calls.flatMap((c) => c.documents.map((d) => ({
  Name: d.name, By: d.by, Date: shortDate(d.date), CapitalCallId: c.id,
})))));
emit("04-capital-calls", insertBlock("ops.CallAuditEntries", calls.flatMap((c) => c.audit.map((a) => ({
  Title: a.title, By: a.by, At: shortDateTime(a.at), Comment: a.comment ?? null, Tone: a.tone, CapitalCallId: c.id,
})))));

// ---------- 05: distributions ----------

const distributions = mock("distributions").distributions;
emit("05-distributions", insertBlock("ops.Distributions", distributions.map((d) => ({
  Id: d.id, Ref: d.ref, FundId: d.fundId, Distributable: d.distributable, LpTotal: d.lpTotal, GpTotal: d.gpTotal,
  PaymentDate: d.paymentDate, Status: d.status, WaterfallType: d.waterfallType, SourceNote: d.sourceNote,
  Recallable: false,
}))));
emit("05-distributions", insertBlock("ops.DistributionTiers", distributions.flatMap((d) => d.tiers.map((t) => ({
  Tier: t.tier, Basis: t.basis, Rate: t.rate, Distributed: t.distributed, LpShare: t.lpShare, GpShare: t.gpShare,
  PoolLeft: t.poolLeft, DistributionId: d.id,
})))));
emit("05-distributions", insertBlock("ops.DistributionPayouts", distributions.flatMap((d) => d.payouts.map((p) => ({
  InvestorId: p.investorId, InvestorName: p.investor, Commitment: p.commitment, Amount: p.amount,
  PctOfLpTotal: p.pctOfLpTotal, Status: p.status, BlockedReason: p.blockedReason ?? null,
  WireRef: p.wireRef ?? null, DistributionId: d.id,
})))));

// ---------- 06: fund ops (drawdowns, wires, recon, treasury, portfolio) ----------

emit("06-fund-ops", insertBlock("ops.Drawdowns", mock("drawdowns").drawdowns.map((d) => ({
  Id: d.id, Facility: d.facility, Lender: d.lender, Purpose: d.purpose, DealId: d.dealId ?? null,
  LinkedCallId: d.linkedCallId ?? null, Amount: d.amount, Rate: d.rate, DrawDate: d.drawDate,
  RepayBy: d.repayBy, Status: d.status,
}))));
emit("06-fund-ops", insertBlock("ops.Wires", mock("wires").wires.map((w) => ({
  Id: w.id, Ref: w.ref, Direction: w.direction, Counterparty: w.counterparty, Type: w.type,
  LinkedRef: w.linkedRef, Amount: w.amount, Time: w.time, Date: w.date, Rail: w.rail, Status: w.status,
  ExceptionReason: w.exceptionReason ?? null,
}))));
emit("06-fund-ops", insertBlock("ops.ReconItems", mock("reconciliation").items.map((i) => ({
  Id: i.id, Date: i.date, Description: i.description, Source: i.source, Book: i.book, Custodian: i.custodian,
  Diff: i.diff, Status: i.status, Assignee: i.assignee ?? null,
}))));

const cash = mock("cash-position");
emit("06-fund-ops", insertBlock("ops.CashAccounts", cash.accounts.map((a) => ({
  Custodian: a.custodian, Account: a.account, Currency: a.currency, Type: a.type, Balance: a.balance,
}))));
emit("06-fund-ops", insertBlock("ops.CashPositionSnapshots", [{
  Id: "current", AsOf: cash.asOf, FundId: cash.fundId, CashOnHand: cash.cashOnHand,
  AccountsCount: cash.accountsCount, UncalledCapital: cash.uncalledCapital, UncalledLps: cash.uncalledLps,
  FacilityHeadroom: cash.facilityHeadroom, FacilityLimit: cash.facilityLimit,
  Net30DayProjection: cash.net30DayProjection, CoverageRatio: cash.coverageRatio,
}]));
emit("06-fund-ops", insertBlock("ops.CashForecastBars", cash.forecastBars.map((height, index) => ({
  SortOrder: index + 1, Height: height, CashPositionSnapshotId: "current",
}))));
emit("06-fund-ops", insertBlock("ops.CashForecastWeeks", cash.weeks.map((w, index) => ({
  SortOrder: index + 1, Label: w.label, Inflows: w.inflows, Outflows: w.outflows, Net: w.net,
  ProjectedBalance: w.projectedBalance, CashPositionSnapshotId: "current",
}))));

const portfolio = mock("portfolio-summary");
emit("06-fund-ops", insertBlock("ops.PortfolioSnapshots", [{
  Id: "current", AsOf: portfolio.asOf, InvestedCapital: portfolio.investedCapital,
  ActiveDeals: portfolio.activeDeals, NetIrrPct: portfolio.netIrrPct, BlendedMoic: portfolio.blendedMoic,
  OnWatchCount: portfolio.onWatchCount, OnWatchExposure: portfolio.onWatchExposure,
  PerformingPct: portfolio.exposureMix.performingPct, WatchPct: portfolio.exposureMix.watchPct,
  NonAccrualPct: portfolio.exposureMix.nonAccrualPct,
}]));
emit("06-fund-ops", insertBlock("ops.PortfolioTrendPoints", portfolio.valueTrend.map((value, index) => ({
  SortOrder: index + 1, Value: value, PortfolioSnapshotId: "current",
}))));

// ---------- 07: published KPI strips ----------

const kpiRows = [];
const addKpis = (screen, kpis) => {
  for (const [metric, value] of Object.entries(kpis)) {
    kpiRows.push({
      ScreenKey: screen, MetricKey: metric,
      NumericValue: typeof value === "number" ? value : null,
      TextValue: typeof value === "string" ? value : null,
    });
  }
};
addKpis("funds", funds.kpis);
addKpis("investors", mock("investors").kpis);
addKpis("users", users.kpis);
addKpis("integrations", integrations.kpis);
addKpis("drawdowns", mock("drawdowns").kpis);
addKpis("investor-access", mock("investor-access").kpis);
// Wires/reconciliation KPI counts are computed from the rows at read time — only
// the published metadata is seeded.
addKpis("wires", { asOf: mock("wires").asOf });
const recon = mock("reconciliation");
addKpis("reconciliation", { asOf: recon.asOf, source: recon.source });
addKpis("reference-data", { currenciesUpdated: reference.currenciesUpdated });
addKpis(`portal-statements/${mock("portal-account").investorId}`, { totalCount: mock("portal-statements").totalCount });
const taxBanner = mock("portal-tax").banner;
addKpis("portal-tax", { bannerHeadline: taxBanner.headline, bannerDetail: taxBanner.detail });
emit("07-kpis", insertBlock("ops.KpiSnapshots", kpiRows));

// ---------- 08: investor portal ----------

const access = mock("investor-access");
emit("08-portal", insertBlock("portal.Contacts", access.contacts.map((c) => ({
  Id: c.id, Name: c.name, Initials: c.initials, InvestorId: c.investorId, InvestorName: c.investor,
  Role: c.role, FundsVisible: c.fundsVisible, Statements: c.statements, Status: c.status,
}))));
emit("08-portal", insertBlock("portal.Capabilities", access.capabilities.map((c, index) => ({
  Label: c.label, SortOrder: index + 1, Enabled: c.enabled,
}))));
emit("08-portal", insertBlock("portal.DocumentTypes", access.documentTypes.map((t, index) => ({
  Label: t.label, SortOrder: index + 1, Exposed: t.exposed,
}))));

const account = mock("portal-account");
const investments = mock("portal-investments");
const activity = mock("portal-activity");
const lpId = account.investorId;

const positionsByName = new Map(investments.positions.map((p) => [p.fund, p]));
emit("08-portal", insertBlock("portal.FundPositions", account.funds.map((f) => {
  const position = positionsByName.get(f.name);
  return {
    InvestorId: lpId, FundId: f.fundId, FundName: f.name, Vintage: f.vintage, Commitment: f.commitment,
    PaidIn: position?.paidIn ?? 0, Distributions: position?.distributions ?? 0, Nav: f.nav,
    NetIrrPct: f.netIrrPct, Tvpi: position?.tvpi ?? 0, Dpi: f.dpi, CalledPct: f.calledPct,
    CalledAmount: f.calledAmount,
  };
})));
emit("08-portal", insertBlock("portal.AccountSnapshots", [{
  InvestorId: lpId, AsOf: account.asOf, Commitment: account.stats.commitment, PaidIn: account.stats.paidIn,
  Distributions: account.stats.distributions, Nav: account.stats.nav, NetIrrPct: account.stats.netIrrPct,
  Tvpi: investments.totals.tvpi, NetInvested: activity.stats.netInvested,
  NextCallDue: activity.stats.nextCallDue ?? null,
}]));

emit("08-portal", insertBlock("portal.RollforwardLines", investments.rollforward.lines.map((line, index) => ({
  InvestorId: lpId, Period: investments.rollforward.period, SortOrder: index + 1, Label: line.line,
  Kind: line.kind, Total: line.total,
}))));
// Per-fund rollforward cells reference the identity of their line — resolved by
// (investor, sort order), which the block above just inserted.
const rollforwardCells = investments.rollforward.lines.flatMap((line, index) => [
  ...(line.fundIII !== null ? [{ FundId: "fund-iii", Amount: line.fundIII, SortOrder: index + 1 }] : []),
  ...(line.fundII !== null ? [{ FundId: "fund-ii", Amount: line.fundII, SortOrder: index + 1 }] : []),
]);
emit("08-portal", `IF NOT EXISTS (SELECT 1 FROM [portal].[RollforwardAmounts])
INSERT INTO [portal].[RollforwardAmounts] ([FundId], [Amount], [PortalRollforwardLineId])
SELECT v.FundId, v.Amount, l.[Id]
FROM (VALUES
${rollforwardCells.map((c) => `    (N'${c.FundId}', ${c.Amount}, ${c.SortOrder})`).join(",\n")}
) v (FundId, Amount, SortOrder)
JOIN [portal].[RollforwardLines] l ON l.[InvestorId] = N'${lpId}' AND l.[SortOrder] = v.SortOrder;
`);

emit("08-portal", insertBlock("portal.ActivityRows", activity.rows.map((r) => ({
  InvestorId: lpId, Date: r.date, Fund: r.fund, Type: r.type, Reference: r.reference, Amount: r.amount,
  Status: r.status,
}))));
emit("08-portal", insertBlock("portal.Documents", mock("portal-statements").documents.map((d) => ({
  Id: d.id, InvestorId: lpId, Name: d.name, Fund: d.fund, Period: d.period, Type: d.type, Date: d.date,
}))));
emit("08-portal", insertBlock("portal.TaxDocuments", mock("portal-tax").documents.map((d) => ({
  Id: d.id, InvestorId: lpId, Name: d.name, Fund: d.fund, TaxYear: d.taxYear, Type: d.type, Status: d.status,
  ExpectedDate: d.expectedDate ?? null,
}))));

const contact = mock("portal-contact");
emit("08-portal", insertBlock("portal.IrConfig", [{
  Id: "current", ManagerName: contact.manager.name, ManagerInitials: contact.manager.initials,
  ManagerTitle: contact.manager.title, Email: contact.email, Phone: contact.phone, Hours: contact.hours,
}]));
emit("08-portal", insertBlock("portal.IrRegardingOptions", contact.regardingOptions.map((label, index) => ({
  Label: label, SortOrder: index + 1,
}))));
emit("08-portal", insertBlock("portal.IrRequests", contact.recentRequests.map((r) => ({
  InvestorId: lpId, Subject: r.subject, Regarding: null, Message: null, Ref: r.ref,
  Date: shortDate(r.date), Status: r.status,
}))));

// ---------- 09: audit log (hash-chained, oldest first) ----------

/** Mirrors Meridian.Domain.Services.AuditSealer.ComputeSeal exactly. */
function computeSeal(previousSeal, atIso, actor, action, subject, detail) {
  const at = `${atIso}.0000000`; // seconds precision in the mock; C# format is fffffff
  const payload = `${previousSeal ?? ""}|${at}|${actor}|${action}|${subject}|${detail}`;
  return createHash("sha256").update(payload, "utf-8").digest("hex").slice(0, 12);
}

const auditEvents = mock("audit-log").events
  .map((e) => ({ ...e, at: shortDateTime(e.time) }))
  .reverse(); // the mock lists newest first; the chain appends oldest first

let previousSeal = null;
let auditSql = "IF NOT EXISTS (SELECT 1 FROM [audit].[Events])\nBEGIN\n";
for (const event of auditEvents) {
  const seal = computeSeal(previousSeal, event.at, event.actor, event.action, event.object, event.detail);
  auditSql += `    INSERT INTO [audit].[Events] ([At], [Actor], [Action], [Tone], [Subject], [Detail], [Seal], [PreviousSeal])\n`
    + `    VALUES (${[event.at, event.actor, event.action, event.tone, event.object, event.detail, seal, previousSeal].map(quote).join(", ")});\n`;
  previousSeal = seal;
}
auditSql += "END;\n";
emit("09-audit", auditSql);

// ---------- write files ----------

mkdirSync(seedDir, { recursive: true });
const bom = "﻿";
const header = "-- Generated from meridian-capital-ops/src/mocks by tools/generate-seed.mjs — do not edit by hand.\n\n";
const names = [...files.keys()].sort();
for (const name of names) {
  writeFileSync(join(seedDir, `${name}.sql`), bom + header + files.get(name), "utf-8");
}
writeFileSync(join(databaseRoot, "scripts", "Script.PostDeployment.sql"),
  bom
  + "-- Post-deployment seed: the sample story shared with the frontend mocks.\n"
  + "-- Regenerate with: node tools/generate-seed.mjs (see tools/README.md).\n\n"
  + names.map((name) => `:r .\\seed\\${name}.sql`).join("\n") + "\n",
  "utf-8");
console.log(`Wrote ${names.length} seed scripts to ${seedDir}`);
