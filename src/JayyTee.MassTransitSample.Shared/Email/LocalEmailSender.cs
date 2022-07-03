using System.Net.Mail;
using JayyTee.MassTransitSample.Application.Infrastructure;
using Microsoft.Extensions.Logging;

namespace JayyTee.MassTransitSample.Shared.Email;

public class LocalEmailSender : IEmailSender, IDisposable
{
    private readonly ILogger<LocalEmailSender> _logger;
    private readonly SmtpClient _smtp;

    public LocalEmailSender(ILogger<LocalEmailSender> logger)
    {
        _logger = logger;
        _smtp = new SmtpClient();
        _smtp.Host = "localhost";
        _smtp.Port = 1025;
    }

    public void SendMessage(MailMessage message)
    {
        _logger.LogInformation("Sending email message");
        _smtp.Send(message);
    }

    public void Dispose() => _smtp.Dispose();
}
