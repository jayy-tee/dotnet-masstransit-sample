namespace JayyTee.MassTransitSample.Shared.Email;

public class EmailSettings
{
    public const string SectionName = "Email";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 25;
}
