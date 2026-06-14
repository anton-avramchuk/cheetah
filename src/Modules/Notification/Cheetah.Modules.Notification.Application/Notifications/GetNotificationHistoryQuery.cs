using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Modules.Notification.Contracts;
using Cheetah.Modules.Notification.Domain.Entities;
using Cheetah.Modules.Notification.Domain.Specifications;

namespace Cheetah.Modules.Notification.Application.Notifications;

/// <summary>История уведомлений получателя с попытками доставки по каналам.</summary>
public sealed record GetNotificationHistoryQuery(Guid RecipientUserId)
    : IQuery<IReadOnlyList<NotificationHistoryDto>>;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetNotificationHistoryQuery, IReadOnlyList<NotificationHistoryDto>>))]
public sealed class GetNotificationHistoryQueryHandler
    : IQueryHandler<GetNotificationHistoryQuery, IReadOnlyList<NotificationHistoryDto>>
{
    private readonly IRepository<NotificationMessage, Guid> _notifications;
    private readonly IRepository<NotificationDispatch, Guid> _dispatches;

    public GetNotificationHistoryQueryHandler(
        IRepository<NotificationMessage, Guid> notifications,
        IRepository<NotificationDispatch, Guid> dispatches)
    {
        _notifications = notifications;
        _dispatches = dispatches;
    }

    public async ValueTask<IReadOnlyList<NotificationHistoryDto>> HandleAsync(
        GetNotificationHistoryQuery query, CancellationToken ct = default)
    {
        var messages = await _notifications.GetAllAsync(
            new NotificationsByRecipientSpecification(query.RecipientUserId), ct);
        if (messages.Count == 0)
            return Array.Empty<NotificationHistoryDto>();

        // Все диспатчи одним запросом + группировка в памяти (без N+1).
        var messageIds = messages.Select(m => m.Id).ToArray();
        var dispatchesByNotification = (await _dispatches.GetAllAsync(
                new DispatchesByNotificationsSpecification(messageIds), ct))
            .GroupBy(d => d.NotificationId)
            .ToDictionary(g => g.Key, g => g.ToArray());

        return messages
            .OrderByDescending(m => m.CreatedAt)
            .Select(message => new NotificationHistoryDto(
                message.Id,
                message.RecipientUserId,
                message.TemplateKey,
                message.Category,
                message.CreatedAt,
                (dispatchesByNotification.TryGetValue(message.Id, out var d) ? d : [])
                    .Select(x => new NotificationDispatchDto(x.Id, x.Channel, x.Status, x.Error, x.UpdatedAt))
                    .ToArray()))
            .ToArray();
    }
}
