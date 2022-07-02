using Microsoft.Extensions.Configuration;

namespace JayyTee.MassTransitSample.Shared.Configuration;

public static class ConfigurationSingleton
{
    private const string EnvironmentPrefixDefault = "JAYYTEE";
    private static string _environmentVariablePrefix = null;
    private static IConfiguration? _instance;
    private static readonly object _padlock = new object();

    public static IConfiguration Instance => _instance ?? throw new InvalidOperationException($"Configuration has not been initialised. Did you forget to call {nameof(Initialise)}");

    public static IConfiguration Initialise(string? environmentVariablePrefix)
    {
        if (_instance is not null) return _instance;

        lock (_padlock)
        {
            _environmentVariablePrefix = environmentVariablePrefix ?? _environmentVariablePrefix;
            _instance ??= BuildConfiguration();
        }

        return _instance;
    }

    private static IConfiguration BuildConfiguration()
    {
        string configurationName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                                   ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                                   ?? "LocalhostToPlaypen";

        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("serilogSettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{configurationName}.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"serilogSettings.{configurationName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables(_environmentVariablePrefix);

        return builder.Build();
    }

}
