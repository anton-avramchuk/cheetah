using Cheetah.AspNetCore.Blazor.Grid;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Cheetah.Modules.Teams.Blazor.ViewModels;
using Cheetah.Modules.Teams.Domain.Entities;

namespace Cheetah.Modules.Teams.Blazor.Services;

/// <summary>
/// CRUD ролей команды для грида: чтение/удаление — из базового сервиса поверх <see cref="IGridRepository{TEntity}"/>;
/// создание/обновление — через доменные фабрики <see cref="TeamRole"/>.
/// </summary>
[Export(LifetimeType.Scoped, typeof(ICrudService<TeamRoleGridViewModel, TeamRoleDetailsViewModel, TeamRoleCreateViewModel>))]
public sealed class TeamRoleCrudService(IGridRepository<TeamRole> repository)
    : BaseCrudService<TeamRole, TeamRoleGridViewModel, TeamRoleDetailsViewModel, TeamRoleCreateViewModel>(repository)
{
    public override async Task<Guid> CreateAsync(TeamRoleCreateViewModel model, CancellationToken ct = default)
    {
        var role = TeamRole.Create(model.Name);
        Repository.Add(role);
        await Repository.SaveChangesAsync(ct);
        return role.Id;
    }

    public override async Task UpdateAsync(Guid id, TeamRoleDetailsViewModel model, CancellationToken ct = default)
    {
        var role = await Repository.GetByIdAsync(id, ct);
        if (role is null) return;

        role.Rename(model.Name);
        await Repository.SaveChangesAsync(ct);
    }
}
