using Marten;
using Marten.Storage;
using Weasel.Core;
using Wolverine;
using Wolverine.Marten;
using Wolverine.Postgresql;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseWolverine(opts =>
{
    opts.ServiceName = "Consumer";

    opts.ListenToPostgresqlQueue("weather").UseDurableInbox();
    opts.OptimizeArtifactWorkflow();
    if(builder.Environment.IsDevelopment())
    {
        opts.Durability.Mode = DurabilityMode.Solo;
    }
});

builder.Services.AddNpgsqlDataSource(builder.Configuration.GetConnectionString("Database")!);
builder.Services.AddMarten(options =>
    {
        options.Policies.AllDocumentsAreMultiTenanted();
        options.Events.TenancyStyle = TenancyStyle.Conjoined;
    })
    .UseLightweightSessions()
    .IntegrateWithWolverine(transportSchemaName:"public", autoCreate: AutoCreate.CreateOrUpdate)
    .OptimizeArtifactWorkflow()
    .UseNpgsqlDataSource();

var app = builder.Build();
app.Run();
