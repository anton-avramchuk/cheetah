using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Cheetah.Modules.Email.Domain.Abstractions;
using Cheetah.Modules.Email.Domain.Entities;
using Cheetah.Modules.Email.DomainEvents;
using Microsoft.Extensions.Logging;

namespace Cheetah.Modules.Email.Application.EventHandlers;

/// <summary>
/// Обрабатывает запрос на отправку письма: идемпотентно (по DispatchId через маркер SentEmail)
/// шлёт через шлюз и публикует обратный статус.
///
/// <para>В слайсе «успех шлюза» = EmailDelivered. В реале EmailDelivered подтверждается вебхуком
/// провайдера, а сразу после accept выставляется промежуточный Sent — см. follow-up (вебхуки в Api).</para>
/// </summary>
[Export(LifetimeType.Scoped)]
public sealed class EmailRequestedHandler : IEventHandler<EmailRequested>
{
    private readonly IRepository<SentEmail, Guid> _sent;
    private readonly IEmailGateway _gateway;
    private readonly IEventBus _eventBus;
    private readonly ILogger<EmailRequestedHandler> _logger;

    public EmailRequestedHandler(
        IRepository<SentEmail, Guid> sent,
        IEmailGateway gateway,
        IEventBus eventBus,
        ILogger<EmailRequestedHandler> logger)
    {
        _sent = sent;
        _gateway = gateway;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async ValueTask HandleAsync(EmailRequested @event, CancellationToken ct = default)
    {
        // Провайдер-уровневая идемпотентность: уже отправляли это сообщение
        if (await _sent.GetByIdAsync(@event.DispatchId, ct) is not null)
        {
            _logger.LogDebug("Email dispatch {DispatchId} already handled, skipping", @event.DispatchId);
            return;
        }

        var result = await _gateway.SendAsync(
            @event.ToAddress, @event.Subject, @event.HtmlBody, @event.PlainBody,
            idempotencyKey: @event.DispatchId.ToString(), ct);

        var marker = SentEmail.Create(@event.DispatchId, @event.NotificationId, @event.ToAddress);
        if (result.Success)
            marker.MarkSucceeded(result.ProviderMessageId);
        else
            marker.MarkFailed($"{result.Reason}: {result.Detail}");
        _sent.Add(marker);

        // Публикуем ДО SaveChanges: статус-событие ложится в OutboxMessages того же DbContext,
        // и один SaveChangesAsync коммитит маркер SentEmail + outbox-строку атомарно.
        if (result.Success)
        {
            await _eventBus.PublishAsync(
                new EmailDelivered(@event.DispatchId, @event.NotificationId,
                    result.ProviderMessageId!, DateTimeOffset.UtcNow), ct);
        }
        else
        {
            await _eventBus.PublishAsync(
                new EmailFailed(@event.DispatchId, @event.NotificationId,
                    result.Reason ?? EmailFailureReason.GatewayError, result.IsPermanent, result.Detail), ct);
        }

        await _sent.SaveChangesAsync(ct);
    }
}
