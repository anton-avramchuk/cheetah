using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Modules.Teams.Domain.Abstractions;
using Cheetah.Modules.Teams.Domain.Entities;

namespace Cheetah.Modules.Teams.Application.Members;

/// <summary>
/// Полный синк реплики участников из Identity. Тянет всех пользователей через
/// <see cref="IIdentityUserDirectory"/> и идемпотентно апсёртит их в справочник участников. Чтобы не
/// нагружать БД, существующие записи обновляются только при изменении контентного хэша
/// (<see cref="TeamMember.Apply"/>); неизменившиеся строки не трогаются вовсе.
/// </summary>
[Export(LifetimeType.Scoped, typeof(ITeamMemberDirectorySynchronizer))]
public sealed class TeamMemberDirectorySynchronizer : ITeamMemberDirectorySynchronizer
{
    private readonly IIdentityUserDirectory _directory;
    private readonly IRepository<TeamMember, Guid> _repository;

    public TeamMemberDirectorySynchronizer(
        IIdentityUserDirectory directory, IRepository<TeamMember, Guid> repository)
    {
        _directory = directory;
        _repository = repository;
    }

    public async ValueTask<int> SyncAsync(CancellationToken ct = default)
    {
        var entries = await _directory.GetAllAsync(ct);
        var existing = (await _repository.GetAllAsync(null, ct)).ToDictionary(m => m.Id);

        var changed = 0;
        foreach (var entry in entries)
        {
            if (existing.TryGetValue(entry.Id, out var member))
            {
                if (member.Apply(entry)) // детект изменений по хэшу — БД не трогаем, если совпал
                {
                    _repository.Update(member);
                    changed++;
                }
            }
            else
            {
                _repository.Add(TeamMember.CreateFromDirectory(entry));
                changed++;
            }
        }

        if (changed > 0)
            await _repository.SaveChangesAsync(ct);

        return changed;
    }
}
