namespace Cheetah.Notifications.Email;

/// <summary>
/// Сообщение email-канала. From обычно задаётся в SmtpOptions, но может быть переопределён.
/// </summary>
public sealed record EmailMessage
{
    public required IReadOnlyList<string> To { get; init; }
    public required string Subject { get; init; }
    public required string Body { get; init; }

    public bool IsHtml { get; init; } = false;
    public IReadOnlyList<string> Cc { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Bcc { get; init; } = Array.Empty<string>();
    public string? From { get; init; }
    public IReadOnlyList<Attachment> Attachments { get; init; } = Array.Empty<Attachment>();
}
