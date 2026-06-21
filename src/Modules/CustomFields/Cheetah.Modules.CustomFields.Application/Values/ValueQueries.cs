using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Modules.CustomFields.Application.Mapping;
using Cheetah.Modules.CustomFields.Contracts;
using Cheetah.Modules.CustomFields.Domain.Entities;
using Cheetah.Modules.CustomFields.Domain.Specifications;

namespace Cheetah.Modules.CustomFields.Application.Values;

// ── Значения сущности ────────────────────────────────────────────────────────────────────

public sealed record GetValuesQuery(string EntityType, string EntityId) : IQuery<CustomFieldValuesDto>;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetValuesQuery, CustomFieldValuesDto>))]
public sealed class GetValuesQueryHandler : IQueryHandler<GetValuesQuery, CustomFieldValuesDto>
{
    private readonly IRepository<CustomFieldValueSet, Guid> _valueSets;
    public GetValuesQueryHandler(IRepository<CustomFieldValueSet, Guid> valueSets) => _valueSets = valueSets;

    public async ValueTask<CustomFieldValuesDto> HandleAsync(GetValuesQuery query, CancellationToken ct = default)
    {
        var set = await _valueSets.GetBySpecAsync(
            new ValueSetByEntitySpecification(null, query.EntityType, query.EntityId), ct);
        var values = CustomFieldJson.Deserialize(set?.ValuesJson);
        return new CustomFieldValuesDto(query.EntityType, query.EntityId, values);
    }
}

// ── Батч-чтение (анти-N+1) ───────────────────────────────────────────────────────────────

public sealed record BatchGetValuesQuery(string EntityType, IReadOnlyList<string> EntityIds)
    : IQuery<IReadOnlyDictionary<string, IReadOnlyDictionary<string, object?>>>;

[Export(LifetimeType.Scoped,
    typeof(IQueryHandler<BatchGetValuesQuery, IReadOnlyDictionary<string, IReadOnlyDictionary<string, object?>>>))]
public sealed class BatchGetValuesQueryHandler
    : IQueryHandler<BatchGetValuesQuery, IReadOnlyDictionary<string, IReadOnlyDictionary<string, object?>>>
{
    private readonly IRepository<CustomFieldValueSet, Guid> _valueSets;
    public BatchGetValuesQueryHandler(IRepository<CustomFieldValueSet, Guid> valueSets) => _valueSets = valueSets;

    public async ValueTask<IReadOnlyDictionary<string, IReadOnlyDictionary<string, object?>>> HandleAsync(
        BatchGetValuesQuery query, CancellationToken ct = default)
    {
        var ids = query.EntityIds.Distinct().ToArray();
        var result = new Dictionary<string, IReadOnlyDictionary<string, object?>>();
        if (ids.Length == 0) return result;

        var sets = await _valueSets.GetAllAsync(
            new ValueSetsByEntityIdsSpecification(null, query.EntityType, ids), ct);

        foreach (var set in sets)
            result[set.EntityId] = CustomFieldJson.Deserialize(set.ValuesJson);
        return result;
    }
}
