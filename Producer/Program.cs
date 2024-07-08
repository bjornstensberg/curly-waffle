using System.Security.Claims;
using Contracts;
using Marten;
using Marten.Events;
using Marten.Events.Daemon.Resiliency;
using Marten.Storage;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Weasel.Core;
using Wolverine;
using Wolverine.Http;
using Wolverine.Marten;
using Wolverine.Postgresql;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Database");
builder.Services.AddNpgsqlDataSource(connectionString!);

builder.Services.AddAuthentication().AddCookie();
builder.Services
    .AddMarten(options =>
    {
        options.Events.StreamIdentity = StreamIdentity.AsString;
        options.Policies.AllDocumentsAreMultiTenanted();
        options.Events.TenancyStyle = TenancyStyle.Conjoined;

    })
    .IntegrateWithWolverine(transportSchemaName: "public", autoCreate: AutoCreate.CreateOrUpdate)
    .PublishEventsToWolverine("Everything", relay =>
    {
        relay.PublishEvent<WeatherForecast>(
            // async (@event, bus) =>
            // {
            //     // bus.TenantId // this is going to be "Marten"
            //     await bus.PublishAsync(@event);
            // }
            );
    })
    .AddAsyncDaemon(DaemonMode.Solo)
    .UseNpgsqlDataSource()
    .ApplyAllDatabaseChangesOnStartup()
    .OptimizeArtifactWorkflow()
    .UseLightweightSessions();

builder.Host.UseWolverine(opts =>
{
    opts.Policies.AutoApplyTransactions();
    opts.PublishMessage<WeatherForecast>().ToPostgresqlQueue("weather");
    if(builder.Environment.IsDevelopment())
    {
        opts.Durability.Mode = DurabilityMode.Solo;
    }

    opts.OptimizeArtifactWorkflow();
});

var app = builder.Build();

app.Use(async (context, @delegate) =>
{
    Claim[] claims = [ new Claim("tenantId", "abc") ];

    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);
    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
    
    await @delegate(context);
});

app.MapWolverineEndpoints(options =>
{
    options.TenantId.IsClaimTypeNamed("tenantId");
    options.TenantId.AssertExists();
});

app.Run();
