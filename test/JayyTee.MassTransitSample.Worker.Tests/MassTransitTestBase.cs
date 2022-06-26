using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace JayyTee.MassTransitSample.Worker.Tests;

public abstract class MassTransitTestBase : AcceptanceTestBase
{
    private IBusControl _busControl;

    protected override void AddTestServices(IServiceCollection services)
    {
        services.AddMassTransit(configurator =>
        {
            ConfigureBus(configurator);

            configurator.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri("amqp://localhost:5672"), h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                string temporaryEndpointName = $"AcceptanceTest-{Guid.NewGuid().ToString("N")}";

                cfg.ReceiveEndpoint(temporaryEndpointName, endpointConfigurator =>
                {
                    endpointConfigurator.Durable = false;
                    endpointConfigurator.AutoDelete = true;

                    endpointConfigurator.ConfigureConsumers(context);
                });
            });
        });
    }

    protected virtual void ConfigureBus(IBusRegistrationConfigurator configurator)
    {
    }

    [OneTimeSetUp]
    public async Task InitMassTransitBase()
    {
        _busControl = ServiceProvider.GetRequiredService<IBusControl>();
        await _busControl.StartAsync();
    }

    [OneTimeTearDown]
    public async Task TearDownMassTransitBase()
    {
        await _busControl.StopAsync();
    }

}
