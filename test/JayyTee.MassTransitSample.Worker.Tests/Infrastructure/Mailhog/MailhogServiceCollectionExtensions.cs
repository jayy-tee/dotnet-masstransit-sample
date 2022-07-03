using Microsoft.Extensions.Options;

namespace JayyTee.MassTransitSample.Worker.Tests.Infrastructure.Mailhog;

public static class MailhogServiceCollectionExtensions
{
    public static IServiceCollection AddMailhogClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MailhogSettings>(configuration.GetSection(MailhogSettings.SectionName));
        services.AddTransient<MailhogApiClient>();

        services.AddHttpClient();
        services.AddHttpClient<MailhogApiClient>(delegate(IServiceProvider provider, HttpClient client)
        {
            var settings = provider.GetRequiredService<IOptions<MailhogSettings>>().Value;
            client.BaseAddress = new Uri(settings.ApiBaseUri);
        });

        return services;
    }
}
