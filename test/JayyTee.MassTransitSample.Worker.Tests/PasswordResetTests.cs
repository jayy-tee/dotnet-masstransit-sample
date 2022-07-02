using FluentAssertions;
using JayyTee.MassTransitSample.Application.Features.PasswordReset;
using JayyTee.MassTransitSample.Worker.Tests.Infrastructure;
using JayyTee.MassTransitSample.Worker.Tests.Infrastructure.Mailhog;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace JayyTee.MassTransitSample.Worker.Tests;

[TestFixture]
[FixtureLifeCycle(LifeCycle.SingleInstance)]
public class PasswordResetTests : MassTransitTestBase
{
    private MailhogApiClient _mailhog = null!;

    [OneTimeSetUp]
    public void OneTimeTestSetup() => _mailhog = ServiceProvider.GetRequiredService<MailhogApiClient>();

    [SetUp]
    public async Task SetupPasswordResetTests() => await _mailhog.DeleteAllMessagesAsync();

    [Test]
    public async Task When_Password_Reset_Is_Requested_Email_Is_Sent()
    {
        var publisher = Resolve<IPublishEndpoint>();
        var recipient = "therecipient@thedomain.com";
        var expectedEmail = new MailhogMessage { Recipients = new List<string>() { recipient }, FromAddress = "no-reply@localhost.localdomain", Subject = "Password Reset" };

        await publisher.Publish<ResetPassword>(new { EmailAddress = recipient });

        var result = await RetryHelper.EventuallyReturnAsync(AMessageFromMailhog(recipient, expectedEmail), timeoutInSeconds: 10, waitBetweenRetriesInMillis: 100);

        result.Should().NotBeNull();
        result!.Body.Should().ContainAll("password", "account");
    }

    private Func<Task<MailhogMessage?>> AMessageFromMailhog(string recipient, MailhogMessage expectedEmail) =>
        async () =>
        {
            var emails = await _mailhog.FindMessagesByRecipient(recipient);
            emails.Should().HaveCountGreaterThan(0);
            emails.Should().ContainEquivalentOf(expectedEmail, options => options.Excluding(m => m.Body));

            return emails.FirstOrDefault();
        };

    protected override void AddTestServices(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MailhogSettings>(configuration.GetSection(MailhogSettings.SectionName));
        services.AddTransient<MailhogApiClient>();

        services.AddHttpClient();
        services.AddHttpClient<MailhogApiClient>(delegate(IServiceProvider provider, HttpClient client)
        {
            var settings = provider.GetRequiredService<IOptions<MailhogSettings>>().Value;
            client.BaseAddress = new Uri(settings.ApiBaseUri);
        });

        base.AddTestServices(services, configuration);
    }
}
