namespace JayyTee.MassTransitSample.Application.Features.PasswordReset;

public record ResetPassword
{
    public string EmailAddress { get; set; } = null!;
}
