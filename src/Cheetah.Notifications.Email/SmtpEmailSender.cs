using System.IO;
using System.Net;
using System.Net.Mail;
using Cheetah.Core.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cheetah.Notifications.Email;

/// <summary>
/// SMTP-реализация INotificationSender&lt;EmailMessage&gt; через System.Net.Mail.SmtpClient (BCL).
/// Для продакшна с современным TLS/OAuth2 разумно swap'нуть на MailKit-реализацию отдельным пакетом.
/// </summary>
[Export(LifetimeType.Scoped, typeof(INotificationSender<EmailMessage>))]
public sealed class SmtpEmailSender : INotificationSender<EmailMessage>
{
    private readonly SmtpOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<SmtpOptions> options, ILogger<SmtpEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async ValueTask SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        if (message.To.Count == 0)
            throw new ArgumentException("EmailMessage.To is empty", nameof(message));

        using var smtp = BuildClient();
        using var mail = BuildMail(message);

        _logger.LogDebug("Sending email to {Recipients}, subject '{Subject}'",
            string.Join(",", message.To), message.Subject);

        // SmtpClient.SendMailAsync не принимает CT в одной перегрузке для BCL — оборачиваем.
        using var registration = cancellationToken.Register(smtp.SendAsyncCancel);
        await smtp.SendMailAsync(mail).ConfigureAwait(false);
    }

    private SmtpClient BuildClient()
    {
        var client = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl = _options.EnableSsl,
            Timeout = (int)_options.Timeout.TotalMilliseconds
        };
        if (!string.IsNullOrEmpty(_options.Username))
        {
            client.Credentials = new NetworkCredential(_options.Username, _options.Password ?? string.Empty);
        }
        return client;
    }

    private MailMessage BuildMail(EmailMessage message)
    {
        var fromAddress = message.From ?? _options.DefaultFromAddress;
        var from = string.IsNullOrEmpty(_options.DefaultFromDisplayName)
            ? new MailAddress(fromAddress)
            : new MailAddress(fromAddress, _options.DefaultFromDisplayName);

        var mail = new MailMessage
        {
            From = from,
            Subject = message.Subject,
            Body = message.Body,
            IsBodyHtml = message.IsHtml
        };

        foreach (var to in message.To) mail.To.Add(to);
        foreach (var cc in message.Cc) mail.CC.Add(cc);
        foreach (var bcc in message.Bcc) mail.Bcc.Add(bcc);

        foreach (var att in message.Attachments)
        {
            // ReadOnlyMemory -> MemoryStream без копирования (когда возможно).
            var stream = new MemoryStream(att.Content.ToArray(), writable: false);
            var attachment = new System.Net.Mail.Attachment(stream, att.FileName, att.ContentType);
            mail.Attachments.Add(attachment);
        }

        return mail;
    }
}
