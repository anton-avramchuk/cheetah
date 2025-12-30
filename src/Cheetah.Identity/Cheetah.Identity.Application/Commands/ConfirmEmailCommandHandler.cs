using Cheetah.Core.CQRS;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;
using Cheetah.Identity.DataAccess;

namespace Cheetah.Identity.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<ConfirmEmailCommand>))]
public class ConfirmEmailCommandHandler : ICommandHandler<ConfirmEmailCommand>
{
    private readonly IIdentityDbContext _dbContext;
    private readonly IEventBus _eventBus;

    public ConfirmEmailCommandHandler(IIdentityDbContext dbContext, IEventBus eventBus)
    {
        _dbContext = dbContext;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(ConfirmEmailCommand command, CancellationToken ct)
    {
        var user = await _dbContext.Users.FindAsync([command.UserId], ct);
        if (user == null)
            throw new InvalidOperationException($"User {command.UserId} not found");

        // Confirm email
        user.ConfirmEmail();

        await _dbContext.SaveChangesAsync(ct);

        // Publish domain events
        foreach (var domainEvent in user.DomainEvents)
        {
            await _eventBus.PublishAsync(domainEvent, ct);
        }
        user.ClearDomainEvents();
    }
}
