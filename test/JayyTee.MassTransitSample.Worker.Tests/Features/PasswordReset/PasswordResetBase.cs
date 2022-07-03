using FluentAssertions;
using JayyTee.MassTransitSample.Worker.Tests.Infrastructure.Mailhog;

namespace JayyTee.MassTransitSample.Worker.Tests.Features.PasswordReset;

public abstract class PasswordResetBase : MassTransitTestBase
{
    protected MailhogApiClient Mailhog = null!;

    [OneTimeSetUp]
    public void OneTimeTestSetup() => Mailhog = ServiceProvider.GetRequiredService<MailhogApiClient>();

    [SetUp]
    public async Task SetupPasswordResetTests() => await Mailhog.DeleteAllMessagesAsync();

    protected Func<Task<MailhogMessage?>> AMessageFromMailhog(string recipient, MailhogMessage expectedEmail) =>
        async () =>
        {
            var emails = await Mailhog.FindMessagesByRecipient(recipient);
            emails.Should().HaveCountGreaterThan(0);
            emails.Should().ContainEquivalentOf(expectedEmail, options => options.Excluding(m => m.Body));

            return emails.FirstOrDefault();
        };

    protected override void AddTestServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddMailhogClient(configuration);

        base.AddTestServices(services, configuration);
    }
}
