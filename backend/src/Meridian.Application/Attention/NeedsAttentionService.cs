using System.Globalization;
using Dapper;
using Meridian.Application.Abstractions;
using Meridian.Application.Common;

namespace Meridian.Application.Attention;

public sealed record AttentionItemDto(
    string Id, string Kind, string Title, string Detail, string Href, string Tone, bool? Mine);

/// <summary>
/// The needs-attention inbox is a computed projection, never stored state
/// (docs/API.md). It aggregates across tables per caller, which is exactly the
/// kind of read path we route through Dapper instead of the ORM.
/// Wire exceptions, recon breaks and integration warnings join here once those
/// modules land (see backend README roadmap).
/// </summary>
public class NeedsAttentionService(IDbConnectionFactory connections, IClock clock, ICurrentUserProvider currentUser)
{
    private sealed class OpenCallRow
    {
        public string Id { get; set; } = "";
        public string Ref { get; set; } = "";
        public string DealName { get; set; } = "";
        public double Amount { get; set; }
        public string DueDate { get; set; } = "";
        public string Status { get; set; } = "";
        public int CurrentStage { get; set; }
        public string StageName { get; set; } = "";
        public string ApproverRole { get; set; } = "";
    }

    private sealed class OverdueRow
    {
        public string Id { get; set; } = "";
        public string Ref { get; set; } = "";
        public string DueDate { get; set; } = "";
        public int LateCount { get; set; }
        public double LateTotal { get; set; }
    }

    public async Task<IReadOnlyList<AttentionItemDto>> ComputeAsync(CancellationToken ct = default)
    {
        var user = await currentUser.GetRequiredAsync(ct);
        var today = clock.Today;

        using var connection = connections.CreateOpenConnection();

        var openCalls = (await connection.QueryAsync<OpenCallRow>(
            """
            SELECT c.Id, c.Ref, c.DealName, c.Amount, c.DueDate, c.Status, c.CurrentStage,
                   s.Name AS StageName, s.ApproverRole
            FROM CapitalCalls c
            JOIN WorkflowStages s ON s."Order" = c.CurrentStage
            WHERE c.Status <> 'Completed'
            ORDER BY c.DueDate
            """)).ToList();

        var overdue = (await connection.QueryAsync<OverdueRow>(
            """
            SELECT c.Id, c.Ref, c.DueDate, COUNT(*) AS LateCount, SUM(a.Amount) AS LateTotal
            FROM CallAllocations a
            JOIN CapitalCalls c ON c.Id = a.CapitalCallId
            WHERE a.WireStatus = 'Overdue'
            GROUP BY c.Id, c.Ref, c.DueDate
            ORDER BY c.DueDate
            """)).ToList();

        var items = new List<AttentionItemDto>();

        foreach (var call in openCalls.Where(c => c.Status == "Returned"))
        {
            items.Add(Item(items.Count, "approval", "amber", call.Id,
                $"Call {call.Ref} · {call.DealName} returned to {call.StageName}",
                $"{Display.Money((decimal)call.Amount)} · awaiting {call.ApproverRole}",
                mine: call.ApproverRole == user.RoleName));
        }

        foreach (var call in openCalls.Where(c => c.Status != "Returned" && c.ApproverRole == user.RoleName))
        {
            items.Add(Item(items.Count, "approval", "blue", call.Id,
                $"Call {call.Ref} · {call.DealName} awaits {call.StageName} approval",
                $"{Display.Money((decimal)call.Amount)} · due {ShortDate(call.DueDate)} · you are the stage approver",
                mine: true));
        }

        foreach (var row in overdue)
        {
            var daysLate = today.DayNumber - ParseDate(row.DueDate).DayNumber;
            items.Add(Item(items.Count, "overdue", "red", row.Id,
                $"{row.LateCount} LP wire{(row.LateCount == 1 ? "" : "s")} overdue on Call {row.Ref}",
                $"{Display.Money((decimal)row.LateTotal)} total · due {ShortDate(row.DueDate)} ({daysLate}d late)",
                mine: false));
        }

        foreach (var call in openCalls)
        {
            var due = ParseDate(call.DueDate);
            if (due >= today && due <= today.AddDays(7))
            {
                items.Add(Item(items.Count, "due-soon", "amber", call.Id,
                    $"Call {call.Ref} · {call.DealName} due {ShortDate(call.DueDate)}",
                    $"{Display.Money((decimal)call.Amount)} · stage {call.CurrentStage} · {call.StageName}",
                    mine: false));
            }
        }

        return items;
    }

    private static AttentionItemDto Item(int index, string kind, string tone, string callId,
        string title, string detail, bool mine) =>
        new($"att-{index + 1}", kind, title, detail, $"/capital-calls/{callId}", tone, mine ? true : null);

    private static DateOnly ParseDate(string iso) =>
        DateOnly.ParseExact(iso, "yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static string ShortDate(string iso) => Display.ShortDate(ParseDate(iso));
}
