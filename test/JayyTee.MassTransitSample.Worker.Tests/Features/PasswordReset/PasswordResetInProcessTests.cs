using System.Net.Mail;
using FluentAssertions;
using JayyTee.MassTransitSample.Application.Features.PasswordReset;
using JayyTee.MassTransitSample.Application.Infrastructure;
using JayyTee.MassTransitSample.Worker.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace JayyTee.MassTransitSample.Worker.Tests.Features.PasswordReset;

[TestFixture]
[FixtureLifeCycle(LifeCycle.SingleInstance)]
public class PasswordResetInProcessTests : PasswordResetBase
{
    protected override bool RequiresInProcess => true;

    private readonly EmailSenderStuntDouble _emailSender = new();

    protected override void ConfigureInProcessHostServices(HostBuilderContext hostContext, IServiceCollection services)
    {
        base.ConfigureInProcessHostServices(hostContext, services);

        services.RemoveAll<IEmailSender>();
        services.AddSingleton<IEmailSender>(_emailSender);
    }

    [SetUp]
    public void Setup()
    {
        _emailSender.ResetList();
    }

    [Test]
    public async Task When_Password_Reset_Is_Requested_Email_Is_Sent_InProcessExample()
    {
        var publisher = Resolve<IPublishEndpoint>();
        var recipient = "therecipient@thedomain.com";
        var expectedOutput = new MailMessage(
            from: "no-reply@localhost.localdomain",
            to: recipient,
            subject: "Password Reset",
            body: string.Empty);

        await publisher.Publish<ResetPassword>(new { EmailAddress = recipient });

        var result = await RetryHelper.EventuallyReturnAsync(async () =>
        {
            _emailSender.SentMessages.Should().HaveCountGreaterThan(0);

            return _emailSender.SentMessages.FirstOrDefault();
        }, timeoutInSeconds: 10, waitBetweenRetriesInMillis: 100);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedOutput,
            options => options
                    .Excluding(m => m.Body)
                    .Excluding(m => m.From));
        result!.Body.Should().ContainAll("password", "account");
        result.From!.Address.Should().Be(expectedOutput.From!.Address);
    }

    internal class EmailSenderStuntDouble : IEmailSender
    {
        private readonly List<MailMessage> _sentMessages = new();
        public IReadOnlyCollection<MailMessage> SentMessages => _sentMessages;

        public void ResetList() => _sentMessages.Clear();

        public void SendMessage(MailMessage message) => _sentMessages.Add(message);
    }
}
