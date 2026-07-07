using System.Globalization;
using System.Reflection;
using System.Text.Json;
using Meridian.Domain.Entities;
using Meridian.Infrastructure.Persistence;

namespace Meridian.Infrastructure.Seeding;

/// <summary>
/// Seeds the read models added for the frontend swap from embedded copies of the
/// frontend mock story (Seeding/MockData/*.json — same files as
/// meridian-capital-ops/src/mocks). The Azure SQL post-deployment seed is
/// generated from the same JSONs by database/tools/generate-seed.mjs, so both
/// stores serve the same story. Runs after <see cref="StorySeed"/>, before
/// SaveChanges.
/// </summary>
public static class MockDataSeed
{
    public static void Apply(AppDbContext db)
    {
        SeedDealsAndDetails(db);
        SeedFundStructure(db);
        SeedInvestorExtras(db);
        SeedStaffPresence(db);
        SeedReferenceData(db);
        SeedIntegrations(db);
        SeedNotificationRules(db);
        SeedDrawdowns(db);
        SeedWires(db);
        SeedReconciliation(db);
        SeedCashPosition(db);
        SeedPortfolioSummary(db);
        SeedInvestorAccess(db);
        SeedPortal(db);
        SeedScreenKpis(db);
    }

    // ---------- portfolio & deals ----------

    private static void SeedDealsAndDetails(AppDbContext db)
    {
        // The mock book has deals the story seed doesn't (echo, foxtrot, gale) —
        // add them so every deal drill-down resolves.
        foreach (var deal in Load("deals").GetProperty("deals").EnumerateArray())
        {
            var id = Str(deal, "id");
            if (db.Deals.Local.Any(d => d.Id == id))
                continue;

            db.Deals.Add(new Deal
            {
                Id = id,
                Name = Str(deal, "name"),
                Borrower = Str(deal, "borrower"),
                Sector = Str(deal, "sector"),
                Country = Str(deal, "country"),
                FundId = Str(deal, "fundId"),
                Tranche = Str(deal, "tranche"),
                Invested = Dec(deal, "invested"),
                Outstanding = Dec(deal, "outstanding"),
                Spread = Str(deal, "spread"),
                NetIrrPct = Dec(deal, "netIrrPct"),
                IrrTrend = Str(deal, "irrTrend"),
                Moic = Dec(deal, "moic"),
                Status = Str(deal, "status"),
            });
        }

        foreach (var property in Load("deal-details").EnumerateObject())
        {
            var detail = property.Value;
            var risk = detail.GetProperty("risk");
            db.DealDetails.Add(new DealDetail
            {
                DealId = property.Name,
                FairValue = Dec(detail, "fairValue"),
                Facility = Dec(detail, "facility"),
                Drawn = Dec(detail, "drawn"),
                Maturity = Str(detail, "maturity"),
                SpreadFloor = Str(detail, "spreadFloor"),
                UpfrontFeePct = Dec(detail, "upfrontFeePct"),
                InternalRating = Str(risk, "internalRating"),
                RiskTrend = Str(risk, "trend"),
                Covenants = Str(risk, "covenants"),
                NetLeverage = Str(risk, "netLeverage"),
                LastReview = Str(risk, "lastReview"),
                Cashflows = detail.GetProperty("cashflows").EnumerateArray().Select(c => new DealCashflow
                {
                    Date = Date(c, "date"),
                    Type = Str(c, "type"),
                    Amount = Dec(c, "amount"),
                    PrincipalBalance = Dec(c, "principalBalance"),
                }).ToList(),
                LpExposures = detail.GetProperty("lpExposure").EnumerateArray().Select(x => new DealLpExposure
                {
                    Investor = Str(x, "investor"),
                    Amount = Dec(x, "amount"),
                }).ToList(),
                Documents = detail.GetProperty("documents").EnumerateArray()
                    .Select(d => new DealDocument { Name = d.GetString() ?? "" }).ToList(),
            });
        }
    }

    private static void SeedPortfolioSummary(AppDbContext db)
    {
        var summary = Load("portfolio-summary");
        var mix = summary.GetProperty("exposureMix");
        db.PortfolioSnapshots.Add(new PortfolioSnapshot
        {
            Id = "current",
            AsOf = Date(summary, "asOf"),
            InvestedCapital = Dec(summary, "investedCapital"),
            ActiveDeals = Int(summary, "activeDeals"),
            NetIrrPct = Dec(summary, "netIrrPct"),
            BlendedMoic = Dec(summary, "blendedMoic"),
            OnWatchCount = Int(summary, "onWatchCount"),
            OnWatchExposure = Dec(summary, "onWatchExposure"),
            PerformingPct = Dec(mix, "performingPct"),
            WatchPct = Dec(mix, "watchPct"),
            NonAccrualPct = Dec(mix, "nonAccrualPct"),
            ValueTrend = summary.GetProperty("valueTrend").EnumerateArray()
                .Select((v, i) => new PortfolioTrendPoint { SortOrder = i + 1, Value = v.GetInt32() }).ToList(),
        });
    }

    // ---------- fund structure, investors, reference ----------

    private static void SeedFundStructure(AppDbContext db)
    {
        var funds = Load("funds");
        foreach (var entity in funds.GetProperty("entities").EnumerateArray())
        {
            db.LegalEntities.Add(new LegalEntity
            {
                FundId = Str(entity, "fundId"),
                Name = Str(entity, "name"),
                Kind = Str(entity, "kind"),
            });
        }

        foreach (var shareClass in funds.GetProperty("shareClasses").EnumerateArray())
        {
            db.ShareClasses.Add(new ShareClass
            {
                FundId = Str(shareClass, "fundId"),
                Name = Str(shareClass, "name"),
                MgmtFeePct = Dec(shareClass, "mgmtFeePct"),
                CarryPct = Dec(shareClass, "carryPct"),
                PrefPct = Dec(shareClass, "prefPct"),
            });
        }
    }

    private static void SeedInvestorExtras(AppDbContext db)
    {
        foreach (var investor in Load("investors").GetProperty("investors").EnumerateArray())
        {
            if (!investor.TryGetProperty("profile", out var profile))
                continue;

            db.InvestorProfiles.Add(new InvestorProfile
            {
                InvestorId = Str(investor, "id"),
                Bank = Str(profile, "bank"),
                AbaMasked = Str(profile, "abaMasked"),
                AccountMasked = Str(profile, "accountMasked"),
                BankingVerified = Str(profile, "bankingVerified"),
                KycDocs = Str(profile, "kycDocs"),
                KycReviewDue = Str(profile, "kycReviewDue"),
            });
        }
    }

    /// <summary>Overlays mock last-active/fund-access display values onto the story-seeded staff.</summary>
    private static void SeedStaffPresence(AppDbContext db)
    {
        foreach (var user in Load("users").GetProperty("users").EnumerateArray())
        {
            var id = Str(user, "id");
            var staff = db.StaffUsers.Local.FirstOrDefault(u => u.Id == id);
            if (staff is null)
                continue;

            staff.LastActive = Str(user, "lastActive");
            staff.FundAccess = Str(user, "fundAccess");
        }
    }

    private static void SeedReferenceData(AppDbContext db)
    {
        var reference = Load("reference-data");
        foreach (var borrower in reference.GetProperty("borrowers").EnumerateArray())
        {
            db.Borrowers.Add(new Borrower
            {
                Name = Str(borrower, "name"),
                Sector = Str(borrower, "sector"),
                Country = Str(borrower, "country"),
                DealName = Str(borrower, "deal"),
                InternalRating = Str(borrower, "internalRating"),
            });
        }

        foreach (var currency in reference.GetProperty("currencies").EnumerateArray())
        {
            db.CurrencyRates.Add(new CurrencyRate
            {
                Code = Str(currency, "code"),
                Rate = Dec(currency, "rate"),
                Note = Str(currency, "note"),
            });
        }

        foreach (var calendar in reference.GetProperty("calendars").EnumerateArray())
        {
            db.SettlementCalendars.Add(new SettlementCalendar
            {
                Name = Str(calendar, "name"),
                NextHoliday = Str(calendar, "nextHoliday"),
            });
        }
    }

    // ---------- admin config ----------

    private static void SeedIntegrations(AppDbContext db)
    {
        foreach (var integration in Load("integrations").GetProperty("integrations").EnumerateArray())
        {
            db.Integrations.Add(new Integration
            {
                Name = Str(integration, "name"),
                Type = Str(integration, "type"),
                Direction = Str(integration, "direction"),
                LastSync = Str(integration, "lastSync"),
                Status = Str(integration, "status"),
                Warning = OptStr(integration, "warning"),
            });
        }
    }

    private static void SeedNotificationRules(AppDbContext db)
    {
        var config = Load("notification-rules");
        foreach (var rule in config.GetProperty("rules").EnumerateArray())
        {
            db.NotificationRules.Add(new NotificationRule
            {
                Id = Str(rule, "id"),
                Name = Str(rule, "name"),
                Trigger = Str(rule, "trigger"),
                Channel = Str(rule, "channel"),
                Recipients = Str(rule, "recipients"),
                Enabled = rule.GetProperty("enabled").GetBoolean(),
            });
        }

        foreach (var channel in config.GetProperty("channels").EnumerateArray())
        {
            db.NotificationChannels.Add(new NotificationChannel
            {
                Name = Str(channel, "name"),
                Detail = Str(channel, "detail"),
                Connected = channel.GetProperty("connected").GetBoolean(),
            });
        }
    }

    // ---------- fund ops ----------

    private static void SeedDrawdowns(AppDbContext db)
    {
        foreach (var draw in Load("drawdowns").GetProperty("drawdowns").EnumerateArray())
        {
            db.Drawdowns.Add(new Drawdown
            {
                Id = Str(draw, "id"),
                Facility = Str(draw, "facility"),
                Lender = Str(draw, "lender"),
                Purpose = Str(draw, "purpose"),
                DealId = OptStr(draw, "dealId"),
                LinkedCallId = OptStr(draw, "linkedCallId"),
                Amount = Dec(draw, "amount"),
                Rate = Str(draw, "rate"),
                DrawDate = Date(draw, "drawDate"),
                RepayBy = OptDate(draw, "repayBy"),
                Status = Str(draw, "status"),
            });
        }
    }

    private static void SeedWires(AppDbContext db)
    {
        foreach (var wire in Load("wires").GetProperty("wires").EnumerateArray())
        {
            db.Wires.Add(new Wire
            {
                Id = Str(wire, "id"),
                Ref = Str(wire, "ref"),
                Direction = Str(wire, "direction"),
                Counterparty = Str(wire, "counterparty"),
                Type = Str(wire, "type"),
                LinkedRef = Str(wire, "linkedRef"),
                Amount = Dec(wire, "amount"),
                Time = Str(wire, "time"),
                Date = Date(wire, "date"),
                Rail = Str(wire, "rail"),
                Status = Str(wire, "status"),
                ExceptionReason = OptStr(wire, "exceptionReason"),
            });
        }
    }

    private static void SeedReconciliation(AppDbContext db)
    {
        foreach (var item in Load("reconciliation").GetProperty("items").EnumerateArray())
        {
            db.ReconItems.Add(new ReconItem
            {
                Id = Str(item, "id"),
                Date = Date(item, "date"),
                Description = Str(item, "description"),
                Source = Str(item, "source"),
                Book = OptDec(item, "book"),
                Custodian = OptDec(item, "custodian"),
                Diff = Dec(item, "diff"),
                Status = Str(item, "status"),
                Assignee = OptStr(item, "assignee"),
            });
        }
    }

    private static void SeedCashPosition(AppDbContext db)
    {
        var cash = Load("cash-position");
        db.CashPositionSnapshots.Add(new CashPositionSnapshot
        {
            Id = "current",
            AsOf = Date(cash, "asOf"),
            FundId = Str(cash, "fundId"),
            CashOnHand = Dec(cash, "cashOnHand"),
            AccountsCount = Int(cash, "accountsCount"),
            UncalledCapital = Dec(cash, "uncalledCapital"),
            UncalledLps = Int(cash, "uncalledLps"),
            FacilityHeadroom = Dec(cash, "facilityHeadroom"),
            FacilityLimit = Dec(cash, "facilityLimit"),
            Net30DayProjection = Dec(cash, "net30DayProjection"),
            CoverageRatio = Dec(cash, "coverageRatio"),
            ForecastBars = cash.GetProperty("forecastBars").EnumerateArray()
                .Select((v, i) => new CashForecastBar { SortOrder = i + 1, Height = v.GetInt32() }).ToList(),
            Weeks = cash.GetProperty("weeks").EnumerateArray().Select((w, i) => new CashForecastWeek
            {
                SortOrder = i + 1,
                Label = Str(w, "label"),
                Inflows = Dec(w, "inflows"),
                Outflows = Dec(w, "outflows"),
                Net = Dec(w, "net"),
                ProjectedBalance = Dec(w, "projectedBalance"),
            }).ToList(),
        });

        foreach (var account in cash.GetProperty("accounts").EnumerateArray())
        {
            db.CashAccounts.Add(new CashAccount
            {
                Custodian = Str(account, "custodian"),
                Account = Str(account, "account"),
                Currency = Str(account, "currency"),
                Type = Str(account, "type"),
                Balance = Dec(account, "balance"),
            });
        }
    }

    // ---------- portal ----------

    private static void SeedInvestorAccess(AppDbContext db)
    {
        var access = Load("investor-access");
        foreach (var contact in access.GetProperty("contacts").EnumerateArray())
        {
            db.PortalContacts.Add(new PortalContact
            {
                Id = Str(contact, "id"),
                Name = Str(contact, "name"),
                Initials = Str(contact, "initials"),
                InvestorId = Str(contact, "investorId"),
                InvestorName = Str(contact, "investor"),
                Role = Str(contact, "role"),
                FundsVisible = Str(contact, "fundsVisible"),
                Statements = Str(contact, "statements"),
                Status = Str(contact, "status"),
            });
        }

        var capabilityOrder = 0;
        foreach (var capability in access.GetProperty("capabilities").EnumerateArray())
        {
            db.PortalCapabilities.Add(new PortalCapability
            {
                SortOrder = ++capabilityOrder,
                Label = Str(capability, "label"),
                Enabled = capability.GetProperty("enabled").GetBoolean(),
            });
        }

        var typeOrder = 0;
        foreach (var documentType in access.GetProperty("documentTypes").EnumerateArray())
        {
            db.PortalDocumentTypes.Add(new PortalDocumentType
            {
                SortOrder = ++typeOrder,
                Label = Str(documentType, "label"),
                Exposed = documentType.GetProperty("exposed").GetBoolean(),
            });
        }
    }

    private static void SeedPortal(AppDbContext db)
    {
        var account = Load("portal-account");
        var investments = Load("portal-investments");
        var activity = Load("portal-activity");
        var investorId = Str(account, "investorId");

        // Positions merge the portal-home fund cards with the investments screen
        // figures (paid-in, distributions, TVPI) — one capital account per fund.
        var positionsByName = investments.GetProperty("positions").EnumerateArray()
            .ToDictionary(p => Str(p, "fund"));
        foreach (var fund in account.GetProperty("funds").EnumerateArray())
        {
            var name = Str(fund, "name");
            positionsByName.TryGetValue(name, out var position);
            db.PortalFundPositions.Add(new PortalFundPosition
            {
                InvestorId = investorId,
                FundId = Str(fund, "fundId"),
                FundName = name,
                Vintage = Int(fund, "vintage"),
                Commitment = Dec(fund, "commitment"),
                PaidIn = position.ValueKind == JsonValueKind.Object ? Dec(position, "paidIn") : 0m,
                Distributions = position.ValueKind == JsonValueKind.Object ? Dec(position, "distributions") : 0m,
                Nav = Dec(fund, "nav"),
                NetIrrPct = Dec(fund, "netIrrPct"),
                Tvpi = position.ValueKind == JsonValueKind.Object ? Dec(position, "tvpi") : 0m,
                Dpi = Dec(fund, "dpi"),
                CalledPct = Dec(fund, "calledPct"),
                CalledAmount = Dec(fund, "calledAmount"),
            });
        }

        var stats = account.GetProperty("stats");
        var totals = investments.GetProperty("totals");
        var activityStats = activity.GetProperty("stats");
        db.PortalAccountSnapshots.Add(new PortalAccountSnapshot
        {
            InvestorId = investorId,
            AsOf = Date(account, "asOf"),
            Commitment = Dec(stats, "commitment"),
            PaidIn = Dec(stats, "paidIn"),
            Distributions = Dec(stats, "distributions"),
            Nav = Dec(stats, "nav"),
            NetIrrPct = Dec(stats, "netIrrPct"),
            Tvpi = Dec(totals, "tvpi"),
            NetInvested = Dec(activityStats, "netInvested"),
            NextCallDue = OptDate(activityStats, "nextCallDue"),
        });

        var rollforward = investments.GetProperty("rollforward");
        var period = Str(rollforward, "period");
        var lineOrder = 0;
        foreach (var line in rollforward.GetProperty("lines").EnumerateArray())
        {
            var amounts = new List<PortalRollforwardAmount>();
            if (OptDec(line, "fundIII") is { } fundIii)
                amounts.Add(new PortalRollforwardAmount { FundId = "fund-iii", Amount = fundIii });
            if (OptDec(line, "fundII") is { } fundIi)
                amounts.Add(new PortalRollforwardAmount { FundId = "fund-ii", Amount = fundIi });

            db.PortalRollforwardLines.Add(new PortalRollforwardLine
            {
                InvestorId = investorId,
                Period = period,
                SortOrder = ++lineOrder,
                Label = Str(line, "line"),
                Kind = Str(line, "kind"),
                Total = Dec(line, "total"),
                Amounts = amounts,
            });
        }

        foreach (var row in activity.GetProperty("rows").EnumerateArray())
        {
            db.PortalActivityRows.Add(new PortalActivityRow
            {
                InvestorId = investorId,
                Date = Date(row, "date"),
                Fund = Str(row, "fund"),
                Type = Str(row, "type"),
                Reference = Str(row, "reference"),
                Amount = Dec(row, "amount"),
                Status = Str(row, "status"),
            });
        }

        foreach (var document in Load("portal-statements").GetProperty("documents").EnumerateArray())
        {
            db.PortalDocuments.Add(new PortalDocument
            {
                Id = Str(document, "id"),
                InvestorId = investorId,
                Name = Str(document, "name"),
                Fund = Str(document, "fund"),
                Period = Str(document, "period"),
                Type = Str(document, "type"),
                Date = Date(document, "date"),
            });
        }

        foreach (var document in Load("portal-tax").GetProperty("documents").EnumerateArray())
        {
            db.PortalTaxDocuments.Add(new PortalTaxDocument
            {
                Id = Str(document, "id"),
                InvestorId = investorId,
                Name = Str(document, "name"),
                Fund = Str(document, "fund"),
                TaxYear = Int(document, "taxYear"),
                Type = Str(document, "type"),
                Status = Str(document, "status"),
                ExpectedDate = OptStr(document, "expectedDate"),
            });
        }

        var contact = Load("portal-contact");
        var manager = contact.GetProperty("manager");
        db.PortalIrConfigs.Add(new PortalIrConfig
        {
            Id = "current",
            ManagerName = Str(manager, "name"),
            ManagerInitials = Str(manager, "initials"),
            ManagerTitle = Str(manager, "title"),
            Email = Str(contact, "email"),
            Phone = Str(contact, "phone"),
            Hours = Str(contact, "hours"),
        });

        var optionOrder = 0;
        foreach (var option in contact.GetProperty("regardingOptions").EnumerateArray())
        {
            db.PortalIrRegardingOptions.Add(new PortalIrRegardingOption
            {
                SortOrder = ++optionOrder,
                Label = option.GetString() ?? "",
            });
        }

        foreach (var request in contact.GetProperty("recentRequests").EnumerateArray())
        {
            db.PortalIrRequests.Add(new PortalIrRequest
            {
                InvestorId = investorId,
                Subject = Str(request, "subject"),
                Ref = Str(request, "ref"),
                Date = ShortDate(Str(request, "date")),
                Status = Str(request, "status"),
            });
        }
    }

    // ---------- published KPI strips ----------

    private static void SeedScreenKpis(AppDbContext db)
    {
        AddKpis(db, "funds", Load("funds").GetProperty("kpis"));
        AddKpis(db, "investors", Load("investors").GetProperty("kpis"));
        AddKpis(db, "users", Load("users").GetProperty("kpis"));
        AddKpis(db, "integrations", Load("integrations").GetProperty("kpis"));
        AddKpis(db, "drawdowns", Load("drawdowns").GetProperty("kpis"));
        AddKpis(db, "investor-access", Load("investor-access").GetProperty("kpis"));

        var wires = Load("wires");
        AddKpis(db, "wires", wires.GetProperty("kpis"));
        AddKpi(db, "wires", "asOf", wires.GetProperty("asOf"));

        var recon = Load("reconciliation");
        AddKpis(db, "reconciliation", recon.GetProperty("kpis"));
        AddKpi(db, "reconciliation", "asOf", recon.GetProperty("asOf"));
        AddKpi(db, "reconciliation", "source", recon.GetProperty("source"));

        AddKpi(db, "reference-data", "currenciesUpdated", Load("reference-data").GetProperty("currenciesUpdated"));

        AddKpi(db, "portal-statements", "totalCount", Load("portal-statements").GetProperty("totalCount"));
        var taxBanner = Load("portal-tax").GetProperty("banner");
        AddKpi(db, "portal-tax", "bannerHeadline", taxBanner.GetProperty("headline"));
        AddKpi(db, "portal-tax", "bannerDetail", taxBanner.GetProperty("detail"));
    }

    private static void AddKpis(AppDbContext db, string screen, JsonElement kpis)
    {
        foreach (var kpi in kpis.EnumerateObject())
            AddKpi(db, screen, kpi.Name, kpi.Value);
    }

    private static void AddKpi(AppDbContext db, string screen, string metric, JsonElement value)
    {
        db.KpiSnapshots.Add(new KpiSnapshot
        {
            ScreenKey = screen,
            MetricKey = metric,
            NumericValue = value.ValueKind == JsonValueKind.Number ? value.GetDecimal() : null,
            TextValue = value.ValueKind == JsonValueKind.String ? value.GetString() : null,
        });
    }

    // ---------- JSON helpers ----------

    private static JsonElement Load(string name)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resource = $"Meridian.Infrastructure.Seeding.MockData.{name}.json";
        using var stream = assembly.GetManifestResourceStream(resource)
            ?? throw new InvalidOperationException($"Embedded mock '{resource}' not found.");
        using var document = JsonDocument.Parse(stream);
        return document.RootElement.Clone();
    }

    private static string Str(JsonElement element, string name) => element.GetProperty(name).GetString() ?? "";

    private static string? OptStr(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static decimal Dec(JsonElement element, string name) => element.GetProperty(name).GetDecimal();

    private static decimal? OptDec(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number
            ? value.GetDecimal()
            : null;

    private static int Int(JsonElement element, string name) => element.GetProperty(name).GetInt32();

    private static DateOnly Date(JsonElement element, string name) =>
        DateOnly.ParseExact(Str(element, name), "yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static DateOnly? OptDate(JsonElement element, string name) =>
        OptStr(element, name) is { } value
            ? DateOnly.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture)
            : null;

    /// <summary>Mock display dates like "Jun 24" are in the story year (2026).</summary>
    private static DateOnly ShortDate(string display) =>
        DateOnly.ParseExact($"{display} 2026", "MMM dd yyyy", CultureInfo.InvariantCulture);
}
