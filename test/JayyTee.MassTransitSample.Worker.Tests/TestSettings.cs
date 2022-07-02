namespace JayyTee.MassTransitSample.Worker.Tests;

public class TestSettings
{
    public const string SectionName = "TestSettings";

    public bool RunInProcess { get; set; }
    public Dictionary<string, string> EnvironmentVariables { get; set; } = new();
}
