using JayyTee.MassTransitSample.Shared.Messaging;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

// ReSharper disable once CheckNamespace
namespace JayyTee.MassTransitSample;

public static class MessagingRegistrationExtensions
{
    public static IServiceCollection AddSendOnlyMessaging(this IServiceCollection services, IConfiguration configuration, Action<IBusRegistrationConfigurator> busConfig)
    {
        services.Configure<RabbitMqSettings>(configuration.GetSection(RabbitMqSettings.SectionName));
        services.AddMassTransit(config =>
        {
            busConfig.Invoke(config);
            config.UsingRabbitMq((context, configurator) => ConfigureRabbitMqFromOptions(context, configurator));
        });

        return services;
    }

    public static IServiceCollection AddMessagingEndpoints(this IServiceCollection services, IConfiguration configuration, IEnumerable<Type> consumerAnchorTypesForRegistration)
    {
        services.Configure<RabbitMqSettings>(configuration.GetSection(RabbitMqSettings.SectionName));
        services.AddMassTransit(config =>
        {
            foreach (var consumerAnchor in consumerAnchorTypesForRegistration)
            {
                config.AddConsumersFromNamespaceContaining(consumerAnchor);
            }

            config.UsingRabbitMq((context, configurator) => ConfigureRabbitMqFromOptions(context, configurator, configureEndpoints: true));
        });

        return services;
    }

    private static void ConfigureRabbitMqFromOptions(IBusRegistrationContext context, IRabbitMqBusFactoryConfigurator cfg, bool configureEndpoints = false)
    {
        var rabbitConfig = context.GetRequiredService<IOptions<RabbitMqSettings>>().Value!;
        cfg.Host(new Uri(rabbitConfig.HostUri), h =>
        {
            h.Username(rabbitConfig.Username);
            h.Password(rabbitConfig.Password);
        });

        if (configureEndpoints)
        {
            cfg.ConfigureEndpoints(context);
        }
    }
}
