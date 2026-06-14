using Cheetah.Modules.Notification.Shared;

namespace Cheetah.Modules.Notification.Domain.Abstractions;

/// <summary>Результат рендеринга шаблона под конкретный канал.</summary>
/// <param name="Subject">Тема (для Email; для Sms/Push может быть пустой).</param>
/// <param name="Body">Готовое тело сообщения.</param>
public sealed record RenderedMessage(string Subject, string Body);

/// <summary>
/// Рендерит шаблон уведомления под канал, подставляя merge-поля. Реализация — в Infrastructure
/// (Scriban). Шаблоны привязаны к паре (templateKey, channel): одно намерение «order.shipped»
/// рендерится по-разному для HTML-письма и короткого SMS.
/// </summary>
public interface ITemplateRenderer
{
    /// <summary>
    /// Рендерит шаблон. Бросает, если шаблон под (templateKey, channel) не найден —
    /// вызывающая сторона трактует это как подавление доставки по каналу.
    /// </summary>
    RenderedMessage Render(string templateKey, NotificationChannel channel, IReadOnlyDictionary<string, string> data);

    /// <summary>Есть ли шаблон под указанную пару (используется роутером/оркестратором).</summary>
    bool HasTemplate(string templateKey, NotificationChannel channel);
}
