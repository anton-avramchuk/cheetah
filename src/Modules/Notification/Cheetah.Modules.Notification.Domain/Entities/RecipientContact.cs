using Cheetah.Core.Domain;

namespace Cheetah.Modules.Notification.Domain.Entities;

/// <summary>
/// Локальная реплика контактных данных пользователя (источник истины — Identity).
/// Notification держит её, чтобы резолвить адресата без синхронного похода в Identity
/// на горячем пути и чтобы PII не летал в событии NotificationRequested.
///
/// <para><see cref="Entity{TId}.Id"/> = UserId. Наполняется доменными событиями Identity
/// (UserCreatedEvent несёт Email). Смена email требует отдельного события Identity —
/// см. follow-up в README.</para>
/// </summary>
public class RecipientContact : Entity<Guid>
{
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? PushToken { get; private set; }

    private RecipientContact() { } // EF

    public static RecipientContact Create(Guid userId, string? email, string? phone = null, string? pushToken = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty", nameof(userId));

        return new RecipientContact
        {
            Id = userId,
            Email = NullIfBlank(email),
            Phone = NullIfBlank(phone),
            PushToken = NullIfBlank(pushToken)
        };
    }

    /// <summary>Обновляет email; возвращает true, если значение изменилось.</summary>
    public bool SetEmail(string? email)
    {
        var value = NullIfBlank(email);
        if (Email == value)
            return false;
        Email = value;
        return true;
    }

    public bool SetPhone(string? phone)
    {
        var value = NullIfBlank(phone);
        if (Phone == value)
            return false;
        Phone = value;
        return true;
    }

    public bool SetPushToken(string? token)
    {
        var value = NullIfBlank(token);
        if (PushToken == value)
            return false;
        PushToken = value;
        return true;
    }

    private static string? NullIfBlank(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
