using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Cheetah.Modules.Identity.DomainEvents;
using Cheetah.Modules.Teams.Domain.Entities;

namespace Cheetah.Modules.Teams.Application.EventHandlers.Identity;

/// <summary>Удаление пользователя в Identity → удаление реплики-участника.</summary>
[Export(LifetimeType.Scoped)]
public sealed class UserDeletedMemberHandler : IEventHandler<UserDeletedEvent>
{
    private readonly IRepository<TeamMember, Guid> _members;

    public UserDeletedMemberHandler(IRepository<TeamMember, Guid> members) => _members = members;

    public async ValueTask HandleAsync(UserDeletedEvent @event, CancellationToken ct = default)
    {
        var member = await _members.GetByIdAsync(@event.UserId, ct);
        if (member is null)
            return;

        _members.Delete(member);
        await _members.SaveChangesAsync(ct);
    }
}
