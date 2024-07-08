using Contracts;
using Marten;
using Wolverine;
using Wolverine.Http;

namespace WebApp;

public static class Endpoints
{
    [WolverineGet("/fails")]
    public static async Task<IResult> GetWeatherFails(IDocumentSession session, ILogger logger)
    {
        var weather = new WeatherForecast(DateTime.Now, Random.Shared.Next(30), "");

        // tenant id is picked up here, but not forwarded to consumer
        logger.LogInformation("Tenant {0}", session.TenantId);
        
        session.Events.StartStream(Guid.NewGuid().ToString(), weather);
        await session.SaveChangesAsync();
        return Results.Ok();
    }
    
    [WolverineGet("/works")]
    public static async Task<IResult> GetWeatherWorks(IDocumentSession session, ILogger logger, IMessageBus bus)
    {
        var weather = new WeatherForecast(DateTime.Now, Random.Shared.Next(30), "");

        // tenant id is picked up here, and IS forwarded correctly to consumer
        logger.LogInformation("Tenant {0}", session.TenantId);

        await bus.PublishAsync(weather);
        return Results.Ok();
    }
}