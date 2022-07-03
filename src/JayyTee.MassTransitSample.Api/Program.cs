using JayyTee.MassTransitSample.Api.PingPong;
using JayyTee.MassTransitSample.Shared.Configuration;
using JayyTee.MassTransitSample.Shared.Logging;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Serilog;

var configuration = ConfigurationSingleton.Instance;
Log.Logger = LoggingExtensions.BuildLogger(configuration);

try
{
    Log.Information("Starting Host...");

    var builder = WebApplication.CreateBuilder(args);
    ConfigureWebApplicationBuilder(builder);

    var app = builder.Build();
    ConfigureWebApplication(app);

    app.Run();

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
    public static void ConfigureWebApplicationBuilder(WebApplicationBuilder builder)
    {
        builder.Configuration.AddConfiguration(ConfigurationSingleton.Instance);
        builder.Host.UseCustomSerilog();
        builder.Services.Configure<HostOptions>(opts => opts.ShutdownTimeout = TimeSpan.FromSeconds(5));
        builder.Services.Configure<RouteOptions>(opts => opts.LowercaseUrls = true);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddControllers();

        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Acme.RaceClub.UserService", Version = "v1" });
            c.TagActionsBy(api =>
            {
                if (api.GroupName != null)
                    return new List<string> { api.GroupName };

                var controllerActionDescriptor = api.ActionDescriptor as ControllerActionDescriptor;
                return new List<string> { controllerActionDescriptor.ControllerName };
            });

            c.DocInclusionPredicate((name, api) => true);
        });

        builder.Services.AddSendOnlyMessaging(builder.Configuration);
    }

    public static void ConfigureWebApplication(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Acme.RaceClub.UserService v1"));

        app.UseHttpsRedirection();

        app.MapPingPongApiEndpoints();

        app.MapControllers();
    }
}
