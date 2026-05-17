namespace Cheetah.Notifications.Email;

public class SmtpOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 25;
    public bool EnableSsl { get; set; } = false;

    /// <summary>Имя пользователя для SMTP-AUTH. Пусто = анонимная отправка.</summary>
    public string? Username { get; set; }
    public string? Password { get; set; }

    /// <summary>Адрес отправителя по умолчанию (используется если EmailMessage.From == null).</summary>
    public string DefaultFromAddress { get; set; } = "no-reply@localhost";
    public string? DefaultFromDisplayName { get; set; }

    /// <summary>Таймаут SMTP-операции.</summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
}
