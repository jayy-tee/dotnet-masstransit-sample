using MassTransit;
using Microsoft.Extensions.Logging;

namespace JayyTee.MassTransitSample.Application.Features.PingPong;

public class PingConsumerDefinition : ConsumerDefinition<PingConsumer>
{
    public PingConsumerDefinition()
    {
        EndpointName = "masstransit-worker";
    }
}

public class PingConsumer : IConsumer<Ping>
{
    private readonly ILogger<PingConsumer> _logger;

    public PingConsumer(ILogger<PingConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<Ping> context)
    {
        _logger.LogInformation("Received Ping: {Message}", context.Message.Message);

        await context.Publish<Pong>(new());
    }
}
