using System.Net;
using System.Net.Mail;

namespace EgyMediChain.Api.Common;

// Brevo SMTP relay (smtp-relay.brevo.com:587) rather than the REST API - the key configured in
// "Brevo:SmtpKey" is an SMTP key (starts with "xsmtpsib-"), which only authenticates against the
// SMTP relay, not the REST API (that needs a separate "xkeysib-" key). Uses the built-in
// System.Net.Mail.SmtpClient, so no extra NuGet package is needed.
public class BrevoEmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<BrevoEmailService> _logger;

    public BrevoEmailService(IConfiguration config, ILogger<BrevoEmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendAsync(string toEmail, string subject, string htmlBody)
    {
        var section = _config.GetSection("Brevo");
        var smtpKey = section["SmtpKey"];
        var loginEmail = section["SmtpLoginEmail"];
        var senderEmail = section["SenderEmail"] ?? loginEmail;
        var senderName = section["SenderName"] ?? "EgyMediChain";

        if (string.IsNullOrWhiteSpace(smtpKey) || string.IsNullOrWhiteSpace(loginEmail))
        {
            // Not configured yet - degrade to a console log instead of throwing, so callers
            // (login/reset/edit flows) keep working even before Brevo is set up.
            _logger.LogWarning("[Brevo not configured] Would send to {ToEmail}: {Subject}", toEmail, subject);
            return;
        }

        try
        {
            using var client = new SmtpClient("smtp-relay.brevo.com", 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(loginEmail, smtpKey)
            };

            using var message = new MailMessage
            {
                From = new MailAddress(senderEmail!, senderName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            message.To.Add(toEmail);

            await client.SendMailAsync(message);
        }
        catch (Exception ex)
        {
            // Never let an email provider outage break the caller (login/reset/edit flows must
            // keep working even if Brevo is down) - log and move on, per §5.4's retry/alerting note.
            _logger.LogError(ex, "Brevo SMTP send failed sending to {ToEmail}", toEmail);
        }
    }
}
