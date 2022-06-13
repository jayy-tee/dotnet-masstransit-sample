using MassTransit;
using Microsoft.Extensions.Logging;

namespace JayyTee.MassTransitSample.Application.Features.PingPong;

public class PongConsumerDefinition : ConsumerDefinition<PongConsumer>
{
    public PongConsumerDefinition()
    {
        EndpointName = "masstransit-worker";
    }
}

public class PongConsumer : IConsumer<Pong>
{
    private readonly ILogger<PongConsumer> _logger;

    public PongConsumer(ILogger<PongConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<Pong> context)
    {
        _logger.LogInformation("Received Pong!");

        return Task.CompletedTask;
    }
}
