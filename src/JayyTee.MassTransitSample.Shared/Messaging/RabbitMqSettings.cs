namespace JayyTee.MassTransitSample.Shared.Messaging;

public class RabbitMqSettings
{
    public const string SectionName = "RabbitMq";

    public string HostUri { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}
