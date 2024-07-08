using Contracts;
using Marten;
using Wolverine;

namespace Consumer;

public class WeatherForecastHandler
{
    public static async Task Handle(
        WeatherForecast data,
        ILogger logger,
        IDocumentSession session,
        Envelope envelope,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Envelope Tenant Id: {0}.\r\nSession Tenant Id: {1}.", envelope.TenantId, session.TenantId);
    }
    
}