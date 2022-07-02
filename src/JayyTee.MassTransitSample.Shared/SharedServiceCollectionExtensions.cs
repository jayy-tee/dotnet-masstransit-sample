using JayyTee.MassTransitSample.Shared.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JayyTee.MassTransitSample;

public static class SharedServiceCollectionExtensions
{
    public static IServiceCollection AddSharedServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEmail(configuration);

        return services;
    }
}
