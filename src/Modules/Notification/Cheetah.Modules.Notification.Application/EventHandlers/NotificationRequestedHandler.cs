using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Cheetah.Modules.Email.DomainEvents;
using Cheetah.Modules.Notification.Domain.Abstractions;
using Cheetah.Modules.Notification.DomainEvents;
using Cheetah.Modules.Notification.Domain.Entities;
using Cheetah.Modules.Notification.Shared;
using Microsoft.Extensions.Logging;

namespace Cheetah.Modules.Notification.Application.EventHandlers;

/// <summary>
/// Вход конвейера: намерение уведомить → раскладка по каналам.
/// Резолвит контакт из локальной реплики, спрашивает роутер, создаёт агрегат + Dispatch
/// на ОСНОВНОЙ канал, рендерит шаблон и публикует канальное событие (для слайса — EmailRequested).
///
/// <para>Идемпотентность — доменная: повтор NotificationRequested с тем же NotificationId
/// отбрасывается по наличию агрегата. (Framework-Inbox по (EventId, consumer) — follow-up,
/// см. README: требует разводки IInboxStore по нескольким DbContext.)</para>
/// </summary>
[Export(LifetimeType.Scoped)]
public sealed class NotificationRequestedHandler : IEventHandler<NotificationRequested>
{
    private readonly IRepository<NotificationMessage, Guid> _notifications;
    private readonly IRepository<NotificationDispatch, Guid> _dispatches;
    private readonly IRepository<RecipientContact, Guid> _contacts;
    private readonly ITemplateRenderer _renderer;
    private readonly IChannelRouter _router;
    private readonly IEventBus _eventBus;
    private readonly ILogger<NotificationRequestedHandler> _logger;

    public NotificationRequestedHandler(
        IRepository<NotificationMessage, Guid> notifications,
        IRepository<NotificationDispatch, Guid> dispatches,
        IRepository<RecipientContact, Guid> contacts,
        ITemplateRenderer renderer,
        IChannelRouter router,
        IEventBus eventBus,
        ILogger<NotificationRequestedHandler> logger)
    {
        _notifications = notifications;
        _dispatches = dispatches;
        _contacts = contacts;
        _renderer = renderer;
        _router = router;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async ValueTask HandleAsync(NotificationRequested @event, CancellationToken ct = default)
    {
        // Идемпотентность: уже обрабатывали это уведомление
        if (await _notifications.GetByIdAsync(@event.NotificationId, ct) is not null)
        {
            _logger.LogDebug("Notification {NotificationId} already processed, skipping", @event.NotificationId);
            return;
        }

        var category = Enum.TryParse<NotificationCategory>(@event.Category, ignoreCase: true, out var parsedCategory)
            ? parsedCategory
            : NotificationCategory.System;

        NotificationChannel? forceChannel =
            Enum.TryParse<NotificationChannel>(@event.ForceChannel, ignoreCase: true, out var fc) ? fc : null;

        var contact = await _contacts.GetByIdAsync(@event.RecipientUserId, ct);
        var plan = _router.Resolve(category, contact, forceChannel);

        var message = NotificationMessage.Create(
            @event.NotificationId, @event.RecipientUserId, @event.TemplateKey, @event.Category, @event.CorrelationId);
        _notifications.Add(message);

        if (plan.Count == 0)
        {
            _logger.LogInformation(
                "Notification {NotificationId} suppressed: no deliverable channel for user {UserId}",
                @event.NotificationId, @event.RecipientUserId);
            await _notifications.SaveChangesAsync(ct);
            return;
        }

        // Слайс доставляет основной канал; фолбэк на plan[1..] — по EmailFailed (follow-up: Saga).
        var primary = plan[0];
        var dispatch = NotificationDispatch.Create(@event.NotificationId, primary);
        _dispatches.Add(dispatch);

        EmailRequested? emailToPublish = null;

        if (primary == NotificationChannel.Email)
        {
            // Штатный роутер не вернёт Email без email-контакта, но кастомный мог бы — защищаемся явно.
            if (string.IsNullOrWhiteSpace(contact?.Email))
            {
                dispatch.MarkSuppressed("No email contact for recipient");
            }
            else if (_renderer.HasTemplate(@event.TemplateKey, NotificationChannel.Email))
            {
                var rendered = _renderer.Render(@event.TemplateKey, NotificationChannel.Email, @event.Data);
                emailToPublish = new EmailRequested(
                    dispatch.Id, @event.NotificationId, contact.Email,
                    rendered.Subject, rendered.Body, PlainBody: null, @event.Category);
            }
            else
            {
                dispatch.MarkSuppressed($"No Email template for key '{@event.TemplateKey}'");
            }
        }
        else
        {
            // Sms/Push в этом слайсе не реализованы — канал есть в плане, но провайдера ещё нет.
            dispatch.MarkSuppressed($"Channel '{primary}' is not wired in this slice");
        }

        // Публикуем ДО SaveChanges: OutboxEventBus кладёт EmailRequested в OutboxMessages того же
        // DbContext, и один SaveChangesAsync коммитит агрегат + dispatch + outbox-строку атомарно.
        // OutboxProcessor затем релеит событие в транспорт (Redis/Kafka).
        if (emailToPublish is not null)
            await _eventBus.PublishAsync(emailToPublish, ct);

        await _notifications.SaveChangesAsync(ct);
    }
}
