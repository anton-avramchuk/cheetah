using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Cheetah.Modules.Email.DomainEvents;
using Cheetah.Modules.Notification.Domain.Entities;

namespace Cheetah.Modules.Notification.Application.EventHandlers;

/// <summary>Обратный статус Email → перевод Dispatch в Delivered. Идемпотентно (терминальный — пропуск).</summary>
[Export(LifetimeType.Scoped)]
public sealed class EmailDeliveredHandler : IEventHandler<EmailDelivered>
{
    private readonly IRepository<NotificationDispatch, Guid> _dispatches;

    public EmailDeliveredHandler(IRepository<NotificationDispatch, Guid> dispatches) => _dispatches = dispatches;

    public async ValueTask HandleAsync(EmailDelivered @event, CancellationToken ct = default)
    {
        var dispatch = await _dispatches.GetByIdAsync(@event.DispatchId, ct);
        if (dispatch is null || dispatch.IsTerminal)
            return;

        dispatch.MarkDelivered(@event.ProviderMessageId);
        _dispatches.Update(dispatch);
        await _dispatches.SaveChangesAsync(ct);
    }
}
