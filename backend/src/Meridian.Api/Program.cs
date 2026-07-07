using System.Text.Json.Serialization;
using Meridian.Api.Auth;
using Meridian.Api.Middleware;
using Meridian.Application;
using Meridian.Application.Abstractions;
using Meridian.Domain.Entities;
using Meridian.Infrastructure;
using Meridian.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)
    .AddOData(options => options
        .AddRouteComponents("odata", BuildEdmModel())
        .Select().Filter().OrderBy().Count().SetMaxTop(200));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>();
builder.Services.AddHttpContextAccessor();

builder.Services
    .AddAuthentication(HeaderAuthentication.Scheme)
    .AddScheme<AuthenticationSchemeOptions, HeaderAuthenticationHandler>(HeaderAuthentication.Scheme, null);

// Secure by default: every endpoint requires an authenticated principal unless
// explicitly opted out (health checks); capability policies layer on top.
builder.Services.AddAuthorization(options =>
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
builder.Services.AddSingleton<IAuthorizationPolicyProvider, CapabilityPolicyProvider>();
builder.Services.AddSingleton<IAuthorizationHandler, CapabilityAuthorizationHandler>();
builder.Services.AddScoped<ICurrentUserProvider, HttpCurrentUserProvider>();

var app = builder.Build();

app.UseMiddleware<DomainExceptionMiddleware>();

if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/healthz").AllowAnonymous();

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().Initialize();
}

app.Run();

static IEdmModel BuildEdmModel()
{
    var modelBuilder = new ODataConventionModelBuilder();
    modelBuilder.EnableLowerCamelCase();
    modelBuilder.EntitySet<Deal>("Deals");
    return modelBuilder.GetEdmModel();
}

/// <summary>Exposed for WebApplicationFactory-based integration tests.</summary>
public partial class Program;
