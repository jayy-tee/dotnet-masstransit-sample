namespace JayyTee.MassTransitSample.Worker.Tests.Infrastructure.Mailhog;

public class MailhogSettings
{
    public const string SectionName = "Mailhog";

    public string ApiBaseUri { get; set; } = "http://localhost:8025";
}
