using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Cheetah.Modules.Identity.DomainEvents;
using Cheetah.Modules.Teams.Domain.Abstractions;
using Cheetah.Modules.Teams.Domain.Entities;

namespace Cheetah.Modules.Teams.Application.EventHandlers.Identity;

/// <summary>
/// Смена username в Identity → обновление реплики-участника. Если реплики ещё нет (пропущено событие
/// создания / гонка с синком) — создаётся заново, чтобы не терять пользователя. Идемпотентно: при
/// совпадении хэша строка в БД не трогается.
/// </summary>
[Export(LifetimeType.Scoped)]
public sealed class UserNameChangedMemberHandler : IEventHandler<UserNameChangedEvent>
{
    private readonly IRepository<TeamMember, Guid> _members;

    public UserNameChangedMemberHandler(IRepository<TeamMember, Guid> members) => _members = members;

    public async ValueTask HandleAsync(UserNameChangedEvent @event, CancellationToken ct = default)
    {
        var entry = new UserDirectoryEntry(@event.UserId, @event.NewUserName);
        var member = await _members.GetByIdAsync(@event.UserId, ct);

        if (member is null)
        {
            _members.Add(TeamMember.CreateFromDirectory(entry));
        }
        else if (member.Apply(entry))
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
