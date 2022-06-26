using Microsoft.Extensions.DependencyInjection;

namespace JayyTee.MassTransitSample.Worker.Tests;

public abstract class AcceptanceTestBase
{
    protected ServiceProvider ServiceProvider = null!;
    private IServiceScope _testScope = null!;

    protected T Resolve<T>() where T : notnull => _testScope.ServiceProvider.GetRequiredService<T>();

    protected virtual void AddTestServices(IServiceCollection services) { }

    [OneTimeSetUp]
    public void InitAcceptanceTestBase()
    {
        var services = new ServiceCollection();

        AddTestServices(services);

        ServiceProvider = services.BuildServiceProvider();
    }

    [SetUp]
    public void SetupTestBase()
    {
        _testScope = ServiceProvider.CreateScope();
    }

    [TearDown]
    public void TearDownTestBase()
    {
        _testScope.Dispose();
    }

    [OneTimeTearDown]
    public void TearDownAcceptanceTestBase()
    {
        ServiceProvider.Dispose();
    }
}
