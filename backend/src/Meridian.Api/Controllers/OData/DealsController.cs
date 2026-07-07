using Meridian.Api.Auth;
using Meridian.Domain;
using Meridian.Domain.Entities;
using Meridian.Infrastructure.Persistence;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;

namespace Meridian.Api.Controllers.OData;

/// <summary>
/// OData entity set at /odata/Deals — server-side $filter/$orderby/$top/$count
/// for production-volume books (BACKEND_TODO flags client-side filtering as a
/// mock-only shortcut). The query composes into SQL via EF Core.
/// </summary>
public class DealsController(AppDbContext db) : ODataController
{
    [EnableQuery(PageSize = 50, MaxTop = 200)]
    [RequireCapability(ModuleName.Blotter, Capability.View)]
    public IQueryable<Deal> Get() => db.Deals.AsNoTracking();
}
