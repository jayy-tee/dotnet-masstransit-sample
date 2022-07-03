using FluentAssertions;
using JayyTee.MassTransitSample.Application.Features.PasswordReset;
using JayyTee.MassTransitSample.Worker.Tests.Infrastructure;
using JayyTee.MassTransitSample.Worker.Tests.Infrastructure.Mailhog;

namespace JayyTee.MassTransitSample.Worker.Tests.Features.PasswordReset;

[TestFixture]
[FixtureLifeCycle(LifeCycle.SingleInstance)]
public class PasswordResetTests : PasswordResetBase
{
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
}
