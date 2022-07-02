using JayyTee.MassTransitSample.Shared.Configuration;
using JayyTee.MassTransitSample.Shared.Logging;
using Serilog;

const string ServiceEnvironmentVariablePrefix = "JAYYTEE_WORKER";

var configuration = ConfigurationSingleton.Initialise(ServiceEnvironmentVariablePrefix);
Log.Logger = LoggingExtensions.BuildLogger(configuration);

try
{
    Log.Information("Starting Host...");

    var host = Host.CreateDefaultBuilder(args)
        .ConfigureHostConfiguration(c => c.AddConfiguration(configuration))
        .UseCustomSerilog()
        .ConfigureServices(AddServices)
        .Build();

    await host.RunAsync();

    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");

    return 1;
}
finally
{
    Log.CloseAndFlush();
}


public partial class Program
{
    /*
    * We encapsulate the infra bootstrapping and so forth here, that way, the configuration can be leveraged by
    * self-hosted test assemblies.
    */
    public static void AddServices(HostBuilderContext hostContext, IServiceCollection services)
    {
        services.Configure<HostOptions>(opts => opts.ShutdownTimeout = TimeSpan.FromSeconds(15));
        services.AddApplicationServices();
        services.AddSharedServices(hostContext.Configuration);
        services.AddMessagingEndpoints(hostContext.Configuration,
            consumerAnchorTypesForRegistration: new[] { typeof(ConsumerAnchor), typeof(JayyTee.MassTransitSample.Application.ConsumerAnchor) });
    }
}
