using Microsoft.Extensions.Configuration;

namespace JayyTee.MassTransitSample.Shared.Configuration;

public static class ConfigurationSingleton
{
    public const string EnvironmentVariablePrefix = "JAYYTEE_MTSAMPLE_";

    private static IConfiguration? _instance;
    private static readonly object _padlock = new object();

    public static IConfiguration Instance
    {
        get
        {
            lock (_padlock)
            {
                _instance ??= BuildConfiguration();

                return _instance;
            }
        }
    }

    private static IConfiguration BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("serilogSettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "LocalhostToPlaypen"}.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"serilogSettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "LocalhostToPlaypen"}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables(EnvironmentVariablePrefix);

        return builder.Build();
    }

}
