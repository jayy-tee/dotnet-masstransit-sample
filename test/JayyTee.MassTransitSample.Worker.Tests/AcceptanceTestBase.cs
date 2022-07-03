using System.Diagnostics;
using FluentAssertions;
using JayyTee.MassTransitSample.Shared.Configuration;
using Microsoft.Extensions.Hosting;
using NUnit.Framework.Internal;
using Serilog;

namespace JayyTee.MassTransitSample.Worker.Tests;

public abstract class AcceptanceTestBase
{
    protected virtual bool RequiresInProcess => false;

    private const string EnvironmentVariablePrefix = "JAYYTEE_TEST_";
    private const string DefaultTestExecutionContext = "Local-InProcess";

    protected ServiceProvider ServiceProvider = null!;
    private IServiceScope _testScope = null!;
    private IHost? _host;
    protected TestSettings Settings = null!;
    private string[] _testCategoriesToBeIgnored = null!;

    private List<string> _logsFilesToAttach = new();

    protected T Resolve<T>() where T : notnull => _testScope.ServiceProvider.GetRequiredService<T>();

    protected virtual void AddTestServices(IServiceCollection services, IConfiguration configuration) { }

    [OneTimeSetUp]
    public void InitAcceptanceTestBase()
    {
        TestContext.WriteLine("Initialising test base");

        string? testExecutionContextFromRunSettings = TestContext.Parameters["TestExecutionContext"];
        string testExecutionContext = Environment.GetEnvironmentVariable($"{EnvironmentVariablePrefix}_TEST_EXECUTION_CONTEXT")
                                      ?? testExecutionContextFromRunSettings
                                      ?? DefaultTestExecutionContext;

        _testCategoriesToBeIgnored = TestContext.Parameters["TestCategoriesToBeIgnored"]?.Split(",") ?? Array.Empty<string>();

        SkipForIgnoredTestCategories();

        var testConfigurationPath = Path.Combine("TestExecutionContext", $"testSettings.{testExecutionContext}.json");
        var testConfiguration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(testConfigurationPath, optional: false)
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton(testConfiguration);

        Settings = testConfiguration.GetSection(TestSettings.SectionName).Get<TestSettings>();

        AddTestServices(services, testConfiguration);

        ServiceProvider = services.BuildServiceProvider();
    }

    [SetUp]
    public async Task SetupTestBase()
    {
        TestContext.WriteLine("Base Setup");

        SkipForIgnoredTestCategories();
        SkipIfRequiresInProcessButNotSelfHosting();

        _testScope = ServiceProvider.CreateScope();

        if (!Settings.RunInProcess) return;
        TestContext.WriteLine("Initialising in-process host");

        InitialiseInProcessHost();

        await _host!.StartAsync();
    }

    private void SkipForIgnoredTestCategories()
    {
        string?[] testCategories = TestContext.CurrentContext.Test.Properties["Category"].Select(c => c.ToString()).ToArray();

        if (testCategories.Intersect(_testCategoriesToBeIgnored).Any())
            Assert.Ignore($"Category ignored by runsettings: {string.Join(",", testCategories)}");
    }

    private void SkipIfRequiresInProcessButNotSelfHosting()
    {
        if (RequiresInProcess && Settings.RunInProcess == false)
            Assert.Ignore($"The test {TestContext.CurrentContext.Test.Name} has been skipped as it requires the service to be self-hosted.");
    }

    private void InitialiseInProcessHost()
    {
        var hostConfiguration = ConfigurationSingleton.Instance;

        var workerAssembly = typeof(Program).Assembly;
        var workerAssemblyPath = Path.GetDirectoryName(workerAssembly.Location);

        var logPath = Path.Combine("TestLogs", $"AcceptanceTest_Host_{Guid.NewGuid():N}.log.txt");
        _logsFilesToAttach.Add(logPath);

        _host = new HostBuilder()
            .UseContentRoot(workerAssemblyPath)
            .UseDefaultServiceProvider(options =>
            {
                options.ValidateScopes = true;
                options.ValidateOnBuild = true;
            })
            .ConfigureHostConfiguration(c => c.AddConfiguration(hostConfiguration))
            .ConfigureAppConfiguration(c => c.AddConfiguration(hostConfiguration))
            .ConfigureServices((context, collection) =>
            {
                Program.AddServices(context, collection);
                ConfigureInProcessHostServices(context, collection);
            })
            .UseSerilog(((context, serviceProvider, loggerConfiguration) =>
            {
                loggerConfiguration
                    .Enrich.FromLogContext()
                    .WriteTo.File(logPath);
            }))
            .Build();
    }

    protected virtual void ConfigureInProcessHostServices(HostBuilderContext hostContext, IServiceCollection services)
    {
    }

    [TearDown]
    public async Task TearDownTestBase()
    {
        TestContext.WriteLine("base test tear down");

        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        _testScope?.Dispose();

        foreach (var logfile in _logsFilesToAttach)
        {
            TestContext.AddTestAttachment(logfile);
        }
    }

    [OneTimeTearDown]
    public void TearDownAcceptanceTestBase()
    {
        TestContext.WriteLine("Base Onetime tear-down");

        ServiceProvider?.Dispose();
    }
}
