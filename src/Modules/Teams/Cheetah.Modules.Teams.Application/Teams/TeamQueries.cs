using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Modules.Teams.Application.Abstractions;
using Cheetah.Modules.Teams.Contracts;
using Cheetah.Modules.Teams.Domain.Entities;
using Cheetah.Modules.Teams.Domain.Specifications;

namespace Cheetah.Modules.Teams.Application.Teams;

// ── Команда по Id ──────────────────────────────────────────────────────────────────────────────

/// <summary>Получить команду со составом по идентификатору (null, если не найдена).</summary>
public sealed record GetTeamByIdQuery<TDto>(Guid Id) : IQuery<TDto?>
    where TDto : TeamDtoBase;

public class GetTeamByIdQueryHandler<TTeam, TDto> : IQueryHandler<GetTeamByIdQuery<TDto>, TDto?>
    where TTeam : TeamBase
    where TDto : TeamDtoBase
{
    private readonly IRepository<TTeam, Guid> _repository;
    private readonly ITeamProjector<TTeam, TDto> _projector;

    public GetTeamByIdQueryHandler(IRepository<TTeam, Guid> repository, ITeamProjector<TTeam, TDto> projector)
    {
        _repository = repository;
        _projector = projector;
    }

    public async ValueTask<TDto?> HandleAsync(GetTeamByIdQuery<TDto> query, CancellationToken ct = default)
    {
        var team = await _repository.GetBySpecAsync(new TeamByIdSpecification<TTeam>(query.Id), ct);
        return team is null ? null : _projector.ToDto(team);
    }
}

// ── Список команд ──────────────────────────────────────────────────────────────────────────────

/// <summary>Список команд с комбинированным фильтром (поиск/только активные).</summary>
public sealed record ListTeamsQuery<TDto>(string? Search, bool OnlyActive) : IQuery<IReadOnlyList<TDto>>
    where TDto : TeamDtoBase;

public class ListTeamsQueryHandler<TTeam, TDto> : IQueryHandler<ListTeamsQuery<TDto>, IReadOnlyList<TDto>>
    where TTeam : TeamBase
    where TDto : TeamDtoBase
{
    private readonly IRepository<TTeam, Guid> _repository;
    private readonly ITeamProjector<TTeam, TDto> _projector;

    public ListTeamsQueryHandler(IRepository<TTeam, Guid> repository, ITeamProjector<TTeam, TDto> projector)
    {
        _repository = repository;
        _projector = projector;
    }

    public async ValueTask<IReadOnlyList<TDto>> HandleAsync(ListTeamsQuery<TDto> query, CancellationToken ct = default)
    {
        var spec = new TeamsFilterSpecification<TTeam>(query.Search, query.OnlyActive);
        var items = await _repository.GetAllAsync(spec, ct);
        return items.Select(_projector.ToDto).ToArray();
    }
}
