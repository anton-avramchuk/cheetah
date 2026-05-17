namespace Cheetah.Notifications.Sms;

public class SmsOptions
{
    /// <summary>
    /// HTTP endpoint провайдера. Получит POST с телом {"to":"...","body":"..."}.
    /// Конкретные интеграции (Twilio, SMSC.ru, Aero и т.п.) — отдельным модулем
    /// либо как pre-deployed адаптер на стороне инфраструктуры.
    /// </summary>
    public string ProviderUrl { get; set; } = string.Empty;

    /// <summary>Bearer-токен для авторизации (Authorization: Bearer ...). Опционально.</summary>
    public string? ApiKey { get; set; }

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);
}
