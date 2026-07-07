using Dapper;
using Meridian.Application.Abstractions;
using Meridian.Application.Common;

namespace Meridian.Application.Attention;

public sealed record AttentionItemDto(
    string Id, string Kind, string Title, string Detail, string Href, string Tone, bool? Mine);

/// <summary>
/// The needs-attention inbox is a computed projection, never stored state
/// (docs/API.md). It aggregates across tables per caller, which is exactly the
/// kind of read path we route through Dapper instead of the ORM: pending
/// approvals for the caller, overdue allocations, wire exceptions, recon
/// breaks, integration warnings, and calls due within 7 days.
/// </summary>
public class NeedsAttentionService(IDbConnectionFactory connections, IClock clock, ICurrentUserProvider currentUser)
{
    private sealed class OpenCallRow
    {
        public string Id { get; set; } = "";
        public string Ref { get; set; } = "";
        public string DealName { get; set; } = "";
        public double Amount { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = "";
        public int CurrentStage { get; set; }
        public string StageName { get; set; } = "";
        public string ApproverRole { get; set; } = "";
    }

    private sealed class OverdueRow
    {
        public string Id { get; set; } = "";
        public string Ref { get; set; } = "";
        public DateTime DueDate { get; set; }
        public int LateCount { get; set; }
        public double LateTotal { get; set; }
    }

    private sealed class ExceptionWireRow
    {
        public string Ref { get; set; } = "";
        public string Counterparty { get; set; } = "";
        public string Type { get; set; } = "";
        public string LinkedRef { get; set; } = "";
        public double Amount { get; set; }
        public string? ExceptionReason { get; set; }
    }

    private sealed class ReconBreakRow
    {
        public string Description { get; set; } = "";
        public double Diff { get; set; }
        public double? Book { get; set; }
        public double? Custodian { get; set; }
        public DateTime Date { get; set; }
    }

    private sealed class IntegrationWarningRow
    {
        public string Name { get; set; } = "";
        public string? Warning { get; set; }
    }

    public async Task<IReadOnlyList<AttentionItemDto>> ComputeAsync(CancellationToken ct = default)
    {
        var user = await currentUser.GetRequiredAsync(ct);
        var today = clock.Today;

        using var connection = connections.CreateOpenConnection();

        var openCalls = (await connection.QueryAsync<OpenCallRow>(
            $"""
            SELECT c.Id, c.Ref, c.DealName, c.Amount, c.DueDate, c.Status, c.CurrentStage,
                   s.Name AS StageName, s.ApproverRole
            FROM {Ops("CapitalCalls")} c
            JOIN {Ops("WorkflowStages")} s ON s."Order" = c.CurrentStage
            WHERE c.Status <> 'Completed'
            ORDER BY c.DueDate
            """)).ToList();

        var overdue = (await connection.QueryAsync<OverdueRow>(
            $"""
            SELECT c.Id, c.Ref, c.DueDate, COUNT(*) AS LateCount, SUM(a.Amount) AS LateTotal
            FROM {Ops("CallAllocations")} a
            JOIN {Ops("CapitalCalls")} c ON c.Id = a.CapitalCallId
            WHERE a.WireStatus = 'Overdue'
            GROUP BY c.Id, c.Ref, c.DueDate
            ORDER BY c.DueDate
            """)).ToList();

        var exceptionWires = (await connection.QueryAsync<ExceptionWireRow>(
            $"""
            SELECT w.Ref, w.Counterparty, w.Type, w.LinkedRef, w.Amount, w.ExceptionReason
            FROM {Ops("Wires")} w
            WHERE w.Status = 'Exception'
            ORDER BY w.Ref
            """)).ToList();

        var reconBreaks = (await connection.QueryAsync<ReconBreakRow>(
            $"""
            SELECT r.Description, r.Diff, r.Book, r.Custodian, r.Date
            FROM {Ops("ReconItems")} r
            WHERE r.Status = 'Break'
            ORDER BY r.Date DESC
            """)).ToList();

        var integrationWarnings = (await connection.QueryAsync<IntegrationWarningRow>(
            $"""
            SELECT i.Name, i.Warning
            FROM {connections.Table(DbSchemas.Admin, "Integrations")} i
            WHERE i.Status = 'Warning'
            ORDER BY i.Name
            """)).ToList();

        var items = new List<AttentionItemDto>();

        foreach (var call in openCalls.Where(c => c.Status == "Returned"))
        {
            items.Add(Item(items.Count, "approval", "amber", $"/capital-calls/{call.Id}",
                $"Call {call.Ref} · {call.DealName} returned to {call.StageName}",
                $"{Display.Money((decimal)call.Amount)} · awaiting {call.ApproverRole}",
                mine: call.ApproverRole == user.RoleName));
        }

        foreach (var call in openCalls.Where(c => c.Status != "Returned" && c.ApproverRole == user.RoleName))
        {
            items.Add(Item(items.Count, "approval", "blue", $"/capital-calls/{call.Id}",
                $"Call {call.Ref} · {call.DealName} awaits {call.StageName} approval",
                $"{Display.Money((decimal)call.Amount)} · due {ShortDate(call.DueDate)} · you are the stage approver",
                mine: true));
        }

        foreach (var row in overdue)
        {
            var daysLate = today.DayNumber - DateOnly.FromDateTime(row.DueDate).DayNumber;
            items.Add(Item(items.Count, "overdue", "red", $"/capital-calls/{row.Id}",
                $"{row.LateCount} LP wire{(row.LateCount == 1 ? "" : "s")} overdue on Call {row.Ref}",
                $"{Display.Money((decimal)row.LateTotal)} total · due {ShortDate(row.DueDate)} ({daysLate}d late)",
                mine: false));
        }

        foreach (var wire in exceptionWires)
        {
            items.Add(Item(items.Count, "exception", "red", "/wires",
                $"Wire {wire.Ref} exception — {wire.Counterparty} {Display.Money((decimal)wire.Amount)}",
                $"{wire.ExceptionReason ?? "Payment failed"} · {wire.Type} {wire.LinkedRef}",
                mine: false));
        }

        foreach (var reconBreak in reconBreaks)
        {
            items.Add(Item(items.Count, "break", "red", "/reconciliation",
                $"Recon break {Display.Money((decimal)reconBreak.Diff)} — {reconBreak.Description}",
                $"Book {MoneyOrDash(reconBreak.Book)} vs custodian {MoneyOrDash(reconBreak.Custodian)} · {ShortDate(reconBreak.Date)}",
                mine: false));
        }

        foreach (var warning in integrationWarnings)
        {
            items.Add(Item(items.Count, "warning", "amber", "/admin/integrations",
                $"{warning.Name} integration warning",
                warning.Warning ?? "Connection requires attention",
                mine: false));
        }

        foreach (var call in openCalls)
        {
            var due = DateOnly.FromDateTime(call.DueDate);
            if (due >= today && due <= today.AddDays(7))
            {
                items.Add(Item(items.Count, "due-soon", "amber", $"/capital-calls/{call.Id}",
                    $"Call {call.Ref} · {call.DealName} due {ShortDate(call.DueDate)}",
                    $"{Display.Money((decimal)call.Amount)} · stage {call.CurrentStage} · {call.StageName}",
                    mine: false));
            }
        }

        return items;
    }

    private string Ops(string table) => connections.Table(DbSchemas.Ops, table);

    private static AttentionItemDto Item(int index, string kind, string tone, string href,
        string title, string detail, bool mine) =>
        new($"att-{index + 1}", kind, title, detail, href, tone, mine ? true : null);

    private static string MoneyOrDash(double? amount) =>
        amount is { } value ? Display.Money((decimal)value) : "—";

    private static string ShortDate(DateTime date) => Display.ShortDate(DateOnly.FromDateTime(date));
}
