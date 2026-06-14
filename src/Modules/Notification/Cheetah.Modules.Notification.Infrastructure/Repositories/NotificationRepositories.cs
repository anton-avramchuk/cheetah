using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.EntityFramework.Repositories;
using Cheetah.Modules.Notification.Domain.Entities;
using Cheetah.Modules.Notification.Infrastructure.Persistence;

namespace Cheetah.Modules.Notification.Infrastructure.Repositories;

[Export(LifetimeType.Scoped, typeof(IRepository<NotificationMessage, Guid>))]
public class NotificationMessageRepository : EfRepository<NotificationsDbContext, NotificationMessage, Guid>
{
    public NotificationMessageRepository(NotificationsDbContext context) : base(context) { }
}

[Export(LifetimeType.Scoped, typeof(IRepository<NotificationDispatch, Guid>))]
public class NotificationDispatchRepository : EfRepository<NotificationsDbContext, NotificationDispatch, Guid>
{
    public NotificationDispatchRepository(NotificationsDbContext context) : base(context) { }
}

[Export(LifetimeType.Scoped, typeof(IRepository<RecipientContact, Guid>))]
public class RecipientContactRepository : EfRepository<NotificationsDbContext, RecipientContact, Guid>
{
    public RecipientContactRepository(NotificationsDbContext context) : base(context) { }
}
