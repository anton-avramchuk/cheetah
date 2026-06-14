using Cheetah.Core.Events;

namespace Cheetah.Modules.Email.DomainEvents;

/// <summary>
/// Запрос на отправку уже отрендеренного письма. Publish: Notification. Consume: Email.
/// Контент уже готов — канал ничего не знает про шаблоны.
///
/// <para><see cref="DispatchId"/> — ключ идемпотентности канала и одновременно
/// idempotency-key для внешнего шлюза (SES/SMTP), чтобы повтор не отправил письмо дважды.</para>
///
/// <para>ВНИМАНИЕ: несёт PII (<see cref="ToAddress"/>). Это единственная точка, где контакт
/// попадает на шину «Notification → канал»; см. follow-up по шифрованию полей/ретеншну.</para>
/// </summary>
public record EmailRequested(
    Guid DispatchId,
    Guid NotificationId,
    string ToAddress,
    string Subject,
    string HtmlBody,
    string? PlainBody,
    string Category) : EventBase;
