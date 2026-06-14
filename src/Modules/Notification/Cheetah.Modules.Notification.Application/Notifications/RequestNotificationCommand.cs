using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Cheetah.Modules.Notification.Domain.Entities;
using Cheetah.Modules.Notification.DomainEvents;

namespace Cheetah.Modules.Notification.Application.Notifications;

/// <summary>
/// Ручной триггер уведомления (стенд продюсера для проверки конвейера). Генерирует
/// NotificationId и публикует NotificationRequested — дальнейшее делает оркестратор.
/// В реальном потоке это событие публикуют сами модули-продюсеры после своих изменений.
/// </summary>
public sealed record RequestNotificationCommand(
    Guid RecipientUserId,
    string TemplateKey,
    string Category,
    IReadOnlyDictionary<string, string> Data,
    string? ForceChannel = null) : ICommand<Guid>;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<RequestNotificationCommand, Guid>))]
public sealed class RequestNotificationCommandHandler : ICommandHandler<RequestNotificationCommand, Guid>
{
    private readonly IEventBus _eventBus;
    private readonly IRepository<NotificationMessage, Guid> _unitOfWork;

    public RequestNotificationCommandHandler(
        IEventBus eventBus,
        IRepository<NotificationMessage, Guid> unitOfWork)
    {
        _eventBus = eventBus;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Guid> HandleAsync(RequestNotificationCommand command, CancellationToken ct = default)
    {
        var notificationId = Guid.NewGuid();
        await _eventBus.PublishAsync(
            new NotificationRequested(
                notificationId,
                command.RecipientUserId,
                command.TemplateKey,
                command.Category,
                command.Data,
                command.ForceChannel),
            ct);

        // У стенда нет собственных бизнес-данных, но при включённом Outbox PublishAsync лишь кладёт
        // событие в OutboxMessages — фиксируем его SaveChanges'ом через тот же DbContext (репозиторий
        // и OutboxStore делят один scoped NotificationsDbContext). Реальный продюсер сохранял бы это
        // вместе со своими изменениями в одной транзакции.
        await _unitOfWork.SaveChangesAsync(ct);
        return notificationId;
    }
}
