using System.Net.Mail;

namespace JayyTee.MassTransitSample.Application.Infrastructure;

public interface IEmailSender
{
    public void SendMessage(MailMessage message);
}
