using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Cheetah.Modules.Email.DomainEvents;
using Cheetah.Modules.Notification.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Cheetah.Modules.Notification.Application.EventHandlers;

/// <summary>
/// Обратный статус Email → перевод Dispatch в Failed. Здесь же — точка фолбэка на следующий
/// канал плана. Полноценная цепочка эскалаций («доставить хотя бы одним каналом за N минут»)
/// — это Saga (Cheetah.Saga.Mongo), см. follow-up; в слайсе только фиксируем неудачу.
/// </summary>
[Export(LifetimeType.Scoped)]
public sealed class EmailFailedHandler : IEventHandler<EmailFailed>
{
    private readonly IRepository<NotificationDispatch, Guid> _dispatches;
    private readonly ILogger<EmailFailedHandler> _logger;

    public EmailFailedHandler(IRepository<NotificationDispatch, Guid> dispatches, ILogger<EmailFailedHandler> logger)
    {
        _dispatches = dispatches;
        _logger = logger;
    }

    public async ValueTask HandleAsync(EmailFailed @event, CancellationToken ct = default)
    {
        var dispatch = await _dispatches.GetByIdAsync(@event.DispatchId, ct);
        if (dispatch is null || dispatch.IsTerminal)
            return;

        dispatch.MarkFailed($"{@event.Reason}: {@event.Detail}");
        _dispatches.Update(dispatch);
        await _dispatches.SaveChangesAsync(ct);

        _logger.LogWarning(
            "Email dispatch {DispatchId} failed ({Reason}, permanent={Permanent}); fallback chain is a follow-up (Saga)",
            @event.DispatchId, @event.Reason, @event.IsPermanent);
    }
}
