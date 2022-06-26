using System.Diagnostics;
using FluentAssertions;
using JayyTee.MassTransitSample.Application.Features.PingPong;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace JayyTee.MassTransitSample.Worker.Tests;

public class PingPongTests : MassTransitTestBase
{
    protected override void AddTestServices(IServiceCollection services)
    {
        services.AddSingleton<MessageList<Pong>>();

        base.AddTestServices(services);
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

        await EventuallyAsync(async () =>
        {
            pongReceiver.Received.Count.Should().BeGreaterThan(0);
            pongReceiver.Received.Select(p => p.CorrelationId).Should().Contain(correlationId);
        }, 5, 200);
    }

    private static void Eventually(Action callback, int timeoutInSeconds, int waitBetweenRetriesInMillis)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var timeout = timeoutInSeconds * 1000;
        bool successIndicator = false;
        int retries = 0;

        Exception? lastException = null;

        do
        {
            try
            {
                callback.Invoke();
                successIndicator = true;
                break;
            }
            catch (Exception e)
            {
                lastException = e;

                retries++;
                Console.WriteLine($"Retry Attempt {retries}: Waiting {waitBetweenRetriesInMillis}ms");
                Thread.Sleep(waitBetweenRetriesInMillis);
            }
        } while (stopwatch.ElapsedMilliseconds <= timeout);

        if (successIndicator is false)
        {
            Console.WriteLine($"Retry Timeout expired after {timeoutInSeconds}s. Failing test.");
            Assert.Fail(lastException!.Message);
        }

        stopwatch.Stop();
    }

    private static async Task EventuallyAsync(Func<Task> callback, int timeoutInSeconds, int waitBetweenRetriesInMillis)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var timeout = timeoutInSeconds * 1000;
        bool successIndicator = false;
        int retries = 0;

        Exception? lastException = null;

        do
        {
            try
            {
                await callback();
                successIndicator = true;
                break;
            }
            catch (Exception e)
            {
                lastException = e;

                retries++;
                Console.WriteLine($"Retry Attempt {retries}: Waiting {waitBetweenRetriesInMillis}ms");
                Thread.Sleep(waitBetweenRetriesInMillis);
            }
        } while (stopwatch.ElapsedMilliseconds <= timeout);

        if (successIndicator is false)
        {
            Console.WriteLine($"Retry Timeout expired after {timeoutInSeconds}s. Failing test.");
            Assert.Fail(lastException!.Message);
        }

        stopwatch.Stop();
    }
}

public class ReceivedMessage<TMessage> where TMessage : class
{
    public TMessage Message { get; set; }
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
