using System.Net.Mail;
using JayyTee.MassTransitSample.Application.Infrastructure;
using MassTransit;

namespace JayyTee.MassTransitSample.Application.Features.PasswordReset;

public class ResetPasswordConsumerDefinition : ConsumerDefinition<ResetPasswordConsumer>
{
    public ResetPasswordConsumerDefinition()
    {
        EndpointName = "masstransit-worker";
    }
}

public class ResetPasswordConsumer : IConsumer<ResetPassword>
{
    private readonly IEmailSender _emailSender;

    public ResetPasswordConsumer(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    public Task Consume(ConsumeContext<ResetPassword> context)
    {
        var body = "A password reset has been requested for your account.";
        var message = new MailMessage(
            from: "The Internet <no-reply@localhost.localdomain>",
            to: context.Message.EmailAddress,
            subject: "Password Reset",
            body: body);

        _emailSender.SendMessage(message);

        return Task.CompletedTask;
    }
}
