using Contracts;
using Marten;
using Wolverine.Http;

namespace WebApp;

public static class Endpoints
{
    [WolverineGet("/produce")]
    public static async Task<IResult> GetWeather(IDocumentSession session, ILogger logger)
    {
        var weather = new WeatherForecast(DateTime.Now, Random.Shared.Next(30), "");

        logger.LogInformation("Tenant {0}", session.TenantId);
        
        session.Events.StartStream(Guid.NewGuid().ToString(), weather);
        await session.SaveChangesAsync();
        return Results.Ok();
    }
}