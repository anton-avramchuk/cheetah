using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Cheetah.Modules.Teams.Contracts;
using Cheetah.Modules.Teams.Domain.Entities;

namespace Cheetah.Modules.Teams.Application.Members;

// ── Участник по Id ─────────────────────────────────────────────────────────────────────────────

/// <summary>Получить участника по идентификатору (null, если не найден).</summary>
public sealed record GetTeamMemberByIdQuery(Guid Id) : IQuery<TeamMemberDto?>;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetTeamMemberByIdQuery, TeamMemberDto?>))]
public sealed class GetTeamMemberByIdQueryHandler : IQueryHandler<GetTeamMemberByIdQuery, TeamMemberDto?>
{
    private readonly IGridRepository<TeamMember, Guid> _repository;

    public GetTeamMemberByIdQueryHandler(IGridRepository<TeamMember, Guid> repository)
        => _repository = repository;

    public async ValueTask<TeamMemberDto?> HandleAsync(GetTeamMemberByIdQuery query, CancellationToken ct = default)
        => await _repository.GetByIdAsync<TeamMemberDto>(query.Id, ct);
}

// ── Грид участников ────────────────────────────────────────────────────────────────────────────

/// <summary>Грид участников (пагинация/сортировка/фильтрация).</summary>
public sealed record GetTeamMembersGridQuery(int Page, int PageSize, List<SortDescriptor> Sort, FilterDescriptor? Filter)
    : IQuery<GridResult<TeamMemberDto>>;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetTeamMembersGridQuery, GridResult<TeamMemberDto>>))]
public sealed class GetTeamMembersGridQueryHandler : IQueryHandler<GetTeamMembersGridQuery, GridResult<TeamMemberDto>>
{
    private readonly IGridRepository<TeamMember, Guid> _repository;

    public GetTeamMembersGridQueryHandler(IGridRepository<TeamMember, Guid> repository)
        => _repository = repository;

    public async ValueTask<GridResult<TeamMemberDto>> HandleAsync(
        GetTeamMembersGridQuery query, CancellationToken ct = default)
    {
        var request = new GridRequest
        {
            Page = query.Page,
            PageSize = query.PageSize,
            Sort = query.Sort,
            Filter = query.Filter
        };
        return await _repository.GetGridAsync<TeamMemberDto>(request, ct);
    }
}
