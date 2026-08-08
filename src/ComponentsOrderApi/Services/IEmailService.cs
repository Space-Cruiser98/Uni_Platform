namespace ComponentsOrderApi.Services;

public interface IEmailService
{
    Task SendEmailVerificationAsync(
        string email,
        string name,
        string verificationUrl,
        CancellationToken ct = default);

    Task SendPasswordResetAsync(
        string email,
        string name,
        string resetUrl,
        CancellationToken ct = default);
}
