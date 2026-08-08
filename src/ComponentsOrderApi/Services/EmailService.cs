using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ComponentsOrderApi.Services;

public class EmailService : IEmailService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        HttpClient httpClient,
        IConfiguration config,
        ILogger<EmailService> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
    }

    public Task SendEmailVerificationAsync(
        string email,
        string name,
        string verificationUrl,
        CancellationToken ct = default)
    {
        return SendAsync(
            email,
            name,
            "Verify your Components Order account",
            $"""
            <h2>Welcome to ENISO Components Order</h2>

            <p>Hello {WebUtility.HtmlEncode(name)},</p>

            <p>Thank you for registering.</p>

            <p>Please verify your email address by clicking the button below:</p>

            <p>
                <a href="{WebUtility.HtmlEncode(verificationUrl)}"
                   style="display:inline-block;padding:12px 20px;background:#2563eb;color:white;text-decoration:none;border-radius:6px;">
                    Verify my email
                </a>
            </p>

            <p>This link expires in 24 hours.</p>

            <p>If you did not create this account, you can ignore this email.</p>
            """,
            ct);
    }

    public Task SendPasswordResetAsync(
        string email,
        string name,
        string resetUrl,
        CancellationToken ct = default)
    {
        return SendAsync(
            email,
            name,
            "Reset your Components Order password",
            $"""
            <h2>Password reset</h2>

            <p>Hello {WebUtility.HtmlEncode(name)},</p>

            <p>We received a request to reset your password.</p>

            <p>
                <a href="{WebUtility.HtmlEncode(resetUrl)}"
                   style="display:inline-block;padding:12px 20px;background:#2563eb;color:white;text-decoration:none;border-radius:6px;">
                    Reset my password
                </a>
            </p>

            <p>This link expires in 30 minutes.</p>

            <p>If you did not request a password reset, you can ignore this email.</p>
            """,
            ct);
    }

    private async Task SendAsync(
        string email,
        string name,
        string subject,
        string htmlContent,
        CancellationToken ct)
    {
        var apiKey = _config["Brevo:ApiKey"];
        var senderEmail = _config["Brevo:SenderEmail"];
        var senderName = _config["Brevo:SenderName"]
                         ?? "ENISO Components Order";

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException(
                "Brevo:ApiKey is not configured.");

        if (string.IsNullOrWhiteSpace(senderEmail))
            throw new InvalidOperationException(
                "Brevo:SenderEmail is not configured.");

        var payload = new
        {
            sender = new
            {
                name = senderName,
                email = senderEmail
            },
            to = new[]
            {
                new
                {
                    email,
                    name
                }
            },
            subject,
            htmlContent
        };

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "https://api.brevo.com/v3/smtp/email");

        request.Headers.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        request.Headers.Add("api-key", apiKey);

        request.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        using var response =
            await _httpClient.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body =
                await response.Content.ReadAsStringAsync(ct);

            _logger.LogError(
                "Brevo email failed. Status: {StatusCode}, Response: {Response}",
                response.StatusCode,
                body);

            throw new InvalidOperationException(
                "Unable to send email.");
        }
    }
}