using System.Linq.Expressions;
using Cheetah.Core.Specification;
using Cheetah.Modules.Notification.Domain.Entities;

namespace Cheetah.Modules.Notification.Domain.Specifications;

/// <summary>Попытки доставки конкретного уведомления.</summary>
public sealed class DispatchesByNotificationSpecification : Specification<NotificationDispatch>
{
    private readonly Guid _notificationId;
    public DispatchesByNotificationSpecification(Guid notificationId) => _notificationId = notificationId;
    public override Expression<Func<NotificationDispatch, bool>> ToExpression()
        => d => d.NotificationId == _notificationId;
}

/// <summary>Попытки доставки по набору уведомлений (для истории — одним запросом, без N+1).</summary>
public sealed class DispatchesByNotificationsSpecification : Specification<NotificationDispatch>
{
    private readonly IReadOnlyCollection<Guid> _notificationIds;
    public DispatchesByNotificationsSpecification(IReadOnlyCollection<Guid> notificationIds)
        => _notificationIds = notificationIds;
    public override Expression<Func<NotificationDispatch, bool>> ToExpression()
        => d => _notificationIds.Contains(d.NotificationId);
}

/// <summary>Уведомления конкретного получателя (для истории).</summary>
public sealed class NotificationsByRecipientSpecification : Specification<NotificationMessage>
{
    private readonly Guid _recipientUserId;
    public NotificationsByRecipientSpecification(Guid recipientUserId) => _recipientUserId = recipientUserId;
    public override Expression<Func<NotificationMessage, bool>> ToExpression()
        => n => n.RecipientUserId == _recipientUserId;
}
