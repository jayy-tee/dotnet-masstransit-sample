using FluentAssertions;
using JayyTee.MassTransitSample.Application.Features.PingPong;
using JayyTee.MassTransitSample.Worker.Tests.Infrastructure;

namespace JayyTee.MassTransitSample.Worker.Tests;

public class PingPongTests : MassTransitTestBase
{
    protected override void AddTestServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<MessageList<Pong>>();

        base.AddTestServices(services, configuration);
    }

    protected override void ConfigureBus(IBusRegistrationConfigurator configurator)
    {
        configurator.AddConsumer<SimpleConsumer<Pong>>();
    }

    [Test]
    public async Task When_Ping_Is_Published_Worker_Replies_With_Pong()
    {
        var publishEndpoint = Resolve<IPublishEndpoint>();
        var correlationId = Guid.NewGuid();

        await publishEndpoint.Publish<Ping>(new { Message = "HELLO!", __CorrelationId = correlationId });

        var pongReceiver = Resolve<MessageList<Pong>>();

        RetryHelper.Eventually(() =>
        {
            pongReceiver.Received.Count.Should().BeGreaterThan(0);
            pongReceiver.Received.Select(p => p.CorrelationId).Should().Contain(correlationId);
        }, 5, 200);
    }
}

public class ReceivedMessage<TMessage> where TMessage : class
{
    public TMessage Message { get; set; } = null!;
    public Guid CorrelationId { get; set; }
}

public class MessageList<TMessage> where TMessage : class
{
    private readonly List<ReceivedMessage<TMessage>> _received = new();
    public IReadOnlyCollection<ReceivedMessage<TMessage>> Received => _received;


    public void Add(TMessage message, Guid correlationId)
    {
        var receivedMessage = new ReceivedMessage<TMessage>() { Message = message, CorrelationId = correlationId };

        lock (_received)
        {
            _received.Add(receivedMessage);
        }
    }
}

public class SimpleConsumer<TMessage> : IConsumer<TMessage> where TMessage : class
{
    private readonly MessageList<TMessage> _messageList;

    public SimpleConsumer(MessageList<TMessage> messageList) => _messageList = messageList;

    public Task Consume(ConsumeContext<TMessage> context)
    {
        _messageList.Add(context.Message, context.InitiatorId!.Value);

        return Task.CompletedTask;
    }
}
