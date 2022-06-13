using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace JayyTee.MassTransitSample.Shared.Logging;

public static class LoggingExtensions
{
    public static ILogger BuildLogger(IConfiguration configuration)
    {
        return BuildLoggerConfiguration(configuration)
            .CreateLogger();
    }

    public static IHostBuilder UseCustomSerilog(this IHostBuilder hostBuilder)
    {
        hostBuilder.UseSerilog((hostContext, loggerConfiguration) => loggerConfiguration
            .ReadFrom.Configuration(hostContext.Configuration)
            .Enrich.FromLogContext(), preserveStaticLogger: true);

        return hostBuilder;
    }

    private static LoggerConfiguration BuildLoggerConfiguration(IConfiguration configuration)
    {
        return new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext();
    }
}
