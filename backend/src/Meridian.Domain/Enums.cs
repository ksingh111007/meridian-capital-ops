namespace Meridian.Domain;

public enum WaterfallType { European, American }

public enum FundStatus { Active, Investing, Harvesting }

/// <summary>Serialized to the API as "In Review" / "Pending" / "Returned" / "Completed".</summary>
public enum CallStatus { InReview, Pending, Returned, Completed }

public enum WireStatus { Pending, Scheduled, Wired, Confirmed, Overdue }

public enum StageState { Done, Current, Pending }

public enum DistributionStatus { Scheduled, Paying, Paid }

public enum PayoutStatus { Scheduled, Sent, Paid, Exception, Blocked }

/// <summary>Which commitment figure a capital call's pro-rata default was computed from.</summary>
public enum AllocationBasis { Unfunded, Commitment }

/// <summary>RBAC modules — mirrors MODULES in the frontend's types.ts ("Ref Data" = RefData).</summary>
public enum ModuleName { Blotter, Approvals, Wires, Recon, RefData, Admin }

/// <summary>Ordered: a user with Edit also satisfies View, etc.</summary>
public enum Capability { None, View, Edit, Approve, Full }

public enum EscalationRuleKind { AmountThreshold, CrossFundAllocation, NewBankAccount }

public enum ErrorKind { Validation, NotFound, Forbidden, Conflict }
