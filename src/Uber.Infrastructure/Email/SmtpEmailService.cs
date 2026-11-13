using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Uber.Application.Abstractions.Email;

namespace Uber.Infrastructure.Email;

public sealed class SmtpEmailService(IConfiguration configuration) : IEmailService
{
    public async Task SendAsync(
        string recipient,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        var host = GetRequiredSetting("Smtp:Host");
        var userName = GetRequiredSetting("Smtp:UserName");
        var password = GetRequiredSetting("Smtp:Password");
        var sender = configuration["Smtp:Sender"] ?? userName;
        var port = int.TryParse(configuration["Smtp:Port"], out var configuredPort)
            ? configuredPort
            : 587;
        var enableSsl = !bool.TryParse(configuration["Smtp:EnableSsl"], out var configuredSsl)
            || configuredSsl;

        using var client = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(userName, password),
            EnableSsl = enableSsl
        };

        using var message = new MailMessage(sender, recipient, subject, htmlBody)
        {
            IsBodyHtml = true
        };

        await client.SendMailAsync(message, cancellationToken);
    }

    private string GetRequiredSetting(string key) =>
        configuration[key]
        ?? throw new InvalidOperationException($"Configuration value '{key}' is required.");
}
