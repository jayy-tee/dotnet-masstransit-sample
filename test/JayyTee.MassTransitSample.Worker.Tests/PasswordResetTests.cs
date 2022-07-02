using FluentAssertions;
using JayyTee.MassTransitSample.Application.Features.PasswordReset;
using JayyTee.MassTransitSample.Worker.Tests.Infrastructure.Mailhog;
using MassTransit;

namespace JayyTee.MassTransitSample.Worker.Tests;

[TestFixture]
[FixtureLifeCycle(LifeCycle.SingleInstance)]
public class PasswordResetTests : MassTransitTestBase
{
    private MailhogApiClient _client;

    [OneTimeSetUp]
    public void OneTimeSetupPasswordResetTest()
    {
        _client = new MailhogApiClient(new MailhogSettings());
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _client.Dispose();
    }

    [SetUp]
    public async Task SetupPasswordResetTests()
    {
        await _client.DeleteAllMessagesAsync();
    }

    [Test]
    public async Task When_Password_Reset_Is_Requested_Email_Is_Sent()
    {
        var publisher = Resolve<IPublishEndpoint>();
        var recipient = "therecipient@thedomain.com";
        var expectedEmail = new MailhogMessage { Recipients = new List<string>() { recipient }, FromAddress = "no-reply@localhost.localdomain", Subject = "Password Reset" };

        await publisher.Publish<ResetPassword>(new { EmailAddress = recipient });

        var result = await EventuallyReturnAsync(AMessageFromMailhog(recipient, expectedEmail), timeoutInSeconds: 10, waitBetweenRetriesInMillis: 100);

        result.Should().NotBeNull();
        result!.Body.Should().ContainAll("password", "account");
    }

    private Func<Task<MailhogMessage?>> AMessageFromMailhog(string recipient, MailhogMessage expectedEmail) =>
        async () =>
        {
            var emails = await _client.FindMessagesByRecipient(recipient);
            emails.Should().HaveCountGreaterThan(0);
            emails.Should().ContainEquivalentOf(expectedEmail, options => options.Excluding(m => m.Body));

            return emails.FirstOrDefault();
        };
}
