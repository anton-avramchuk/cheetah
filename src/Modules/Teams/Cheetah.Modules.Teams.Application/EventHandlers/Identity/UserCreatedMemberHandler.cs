using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Cheetah.Modules.Identity.DomainEvents;
using Cheetah.Modules.Teams.Domain.Abstractions;
using Cheetah.Modules.Teams.Domain.Entities;

namespace Cheetah.Modules.Teams.Application.EventHandlers.Identity;

/// <summary>
/// Создание пользователя в Identity → апсёрт реплики-участника. Дополняет фоновый bulk-синк
/// (<see cref="ITeamMemberDirectorySynchronizer"/>): реплика обновляется сразу по событию, а не ждёт
/// следующего тика. Идемпотентно — повтор события или гонка с синком не плодят дублей (апсёрт по Id +
/// сравнение по хэшу через <see cref="TeamMember.Apply"/>).
/// </summary>
[Export(LifetimeType.Scoped)]
public sealed class UserCreatedMemberHandler : IEventHandler<UserCreatedEvent>
{
    private readonly IRepository<TeamMember, Guid> _members;

    public UserCreatedMemberHandler(IRepository<TeamMember, Guid> members) => _members = members;

    public async ValueTask HandleAsync(UserCreatedEvent @event, CancellationToken ct = default)
    {
        var entry = new UserDirectoryEntry(@event.UserId, @event.UserName);
        var member = await _members.GetByIdAsync(@event.UserId, ct);

        if (member is null)
        {
            _members.Add(TeamMember.CreateFromDirectory(entry));
        }
        else if (member.Apply(entry)) // хэш не совпал — реплика обновлена
        {
            _members.Update(member);
        }
        else
        {
            return;
        }

        await _members.SaveChangesAsync(ct);
    }
}
