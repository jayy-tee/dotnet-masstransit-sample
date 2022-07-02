using JayyTee.MassTransitSample.Application.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JayyTee.MassTransitSample.Shared.Email;

public static class EmailServiceCollectionExtensions
{
    public static IServiceCollection AddEmail(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
        services.AddSingleton<IEmailSender, LocalEmailSender>();

        return services;
    }
}
