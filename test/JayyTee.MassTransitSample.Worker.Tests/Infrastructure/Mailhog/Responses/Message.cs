using JayyTee.MassTransitSample.Shared.Email;

namespace JayyTee.MassTransitSample.Worker.Tests.Infrastructure.Mailhog.Responses;

public class Message
{
    public string? Id { get; set; }
    public EmailAddress From { get; set; } = null!;
    public IEnumerable<EmailAddress> To { get; set; } = null!;
    public EmailContent Content { get; set; } = null!;

    public class EmailAddress
    {
        public string? Mailbox { get; set; }
        public string? Domain { get; set; }
    }

    public class EmailContent
    {
        public string? Body { get; set; }
        public EmailHeaders Headers { get; set; } = null!;
    }

    public class EmailHeaders
    {
        public string[]? Subject { get; set; }
    }
}
