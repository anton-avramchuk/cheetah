using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Cheetah.Modules.Teams.Contracts;
using Cheetah.Modules.Teams.Domain.Entities;

namespace Cheetah.Modules.Teams.Application.Roles;

// ── Роль по Id ─────────────────────────────────────────────────────────────────────────────────

/// <summary>Получить роль по идентификатору (null, если не найдена).</summary>
public sealed record GetTeamRoleByIdQuery(Guid Id) : IQuery<TeamRoleDto?>;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetTeamRoleByIdQuery, TeamRoleDto?>))]
public sealed class GetTeamRoleByIdQueryHandler : IQueryHandler<GetTeamRoleByIdQuery, TeamRoleDto?>
{
    private readonly IGridRepository<TeamRole, Guid> _repository;

    public GetTeamRoleByIdQueryHandler(IGridRepository<TeamRole, Guid> repository)
        => _repository = repository;

    public async ValueTask<TeamRoleDto?> HandleAsync(GetTeamRoleByIdQuery query, CancellationToken ct = default)
        => await _repository.GetByIdAsync<TeamRoleDto>(query.Id, ct);
}

// ── Грид ролей ─────────────────────────────────────────────────────────────────────────────────

/// <summary>Грид ролей (пагинация/сортировка/фильтрация).</summary>
public sealed record GetTeamRolesGridQuery(int Page, int PageSize, List<SortDescriptor> Sort, FilterDescriptor? Filter)
    : IQuery<GridResult<TeamRoleDto>>;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetTeamRolesGridQuery, GridResult<TeamRoleDto>>))]
public sealed class GetTeamRolesGridQueryHandler : IQueryHandler<GetTeamRolesGridQuery, GridResult<TeamRoleDto>>
{
    private readonly IGridRepository<TeamRole, Guid> _repository;

    public GetTeamRolesGridQueryHandler(IGridRepository<TeamRole, Guid> repository)
        => _repository = repository;

    public async ValueTask<GridResult<TeamRoleDto>> HandleAsync(
        GetTeamRolesGridQuery query, CancellationToken ct = default)
    {
        var request = new GridRequest
        {
            Page = query.Page,
            PageSize = query.PageSize,
            Sort = query.Sort,
            Filter = query.Filter
        };
        return await _repository.GetGridAsync<TeamRoleDto>(request, ct);
    }
}
