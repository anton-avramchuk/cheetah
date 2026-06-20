using Cheetah.Core.CQRS;
using Cheetah.Modules.FeatureManagement.Application.Abstractions;
using Cheetah.Modules.FeatureManagement.Contracts;
using Cheetah.Modules.FeatureManagement.Domain.Entities;
using Cheetah.Modules.FeatureManagement.Domain.Specifications;

namespace Cheetah.Modules.FeatureManagement.Application.Queries;

// ── Флаг по ключу ──────────────────────────────────────────────────────────────────────────

/// <summary>Получить флаг по ключу (null, если не найден).</summary>
public sealed record GetFeatureFlagByKeyQuery<TDto>(string Key) : IQuery<TDto?>
    where TDto : FeatureFlagDtoBase;

public class GetFeatureFlagByKeyQueryHandler<TFlag, TDto> : IQueryHandler<GetFeatureFlagByKeyQuery<TDto>, TDto?>
    where TFlag : FeatureFlagBase
    where TDto : FeatureFlagDtoBase
{
    private readonly Domain.Repositories.IFeatureFlagRepository<TFlag> _repository;
    private readonly IFeatureFlagProjector<TFlag, TDto> _projector;

    public GetFeatureFlagByKeyQueryHandler(
        Domain.Repositories.IFeatureFlagRepository<TFlag> repository, IFeatureFlagProjector<TFlag, TDto> projector)
    {
        _repository = repository;
        _projector = projector;
    }

    public async ValueTask<TDto?> HandleAsync(GetFeatureFlagByKeyQuery<TDto> query, CancellationToken ct = default)
    {
        var flag = await _repository.GetByKeyAsync(query.Key, includeChildren: true, ct);
        return flag is null ? null : _projector.ToDto(flag);
    }
}

// ── Список флагов ──────────────────────────────────────────────────────────────────────────

/// <summary>Список флагов с опциональным фильтром по сервису и активности.</summary>
public sealed record ListFeatureFlagsQuery<TDto>(string? OwnerService, bool? OnlyActive)
    : IQuery<IReadOnlyList<TDto>>
    where TDto : FeatureFlagDtoBase;

public class ListFeatureFlagsQueryHandler<TFlag, TDto> : IQueryHandler<ListFeatureFlagsQuery<TDto>, IReadOnlyList<TDto>>
    where TFlag : FeatureFlagBase
    where TDto : FeatureFlagDtoBase
{
    private readonly Domain.Repositories.IFeatureFlagRepository<TFlag> _repository;
    private readonly IFeatureFlagProjector<TFlag, TDto> _projector;

    public ListFeatureFlagsQueryHandler(
        Domain.Repositories.IFeatureFlagRepository<TFlag> repository, IFeatureFlagProjector<TFlag, TDto> projector)
    {
        _repository = repository;
        _projector = projector;
    }

    public async ValueTask<IReadOnlyList<TDto>> HandleAsync(ListFeatureFlagsQuery<TDto> query, CancellationToken ct = default)
    {
        Cheetah.Core.Specification.ISpecification<TFlag>? spec = query.OwnerService is { } svc
            ? new FlagsByOwnerServiceSpecification<TFlag>(svc)
            : query.OnlyActive == true ? new ActiveFlagsSpecification<TFlag>() : null;

        var flags = await _repository.ListAsync(spec, includeChildren: true, ct);
        var filtered = query.OnlyActive == true ? flags.Where(f => f.IsActive) : flags;
        return filtered.Select(_projector.ToDto).ToArray();
    }
}
