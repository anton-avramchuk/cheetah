using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Expressions;
using Cheetah.Modules.CustomFields.Application.Mapping;
using Cheetah.Modules.CustomFields.Contracts;
using Cheetah.Modules.CustomFields.Domain.Abstractions;
using Cheetah.Modules.CustomFields.Domain.Entities;
using Cheetah.Modules.CustomFields.Domain.Specifications;

namespace Cheetah.Modules.CustomFields.Application.Definitions;

// ── Список определений типа (админка) ────────────────────────────────────────────────────

public sealed record ListDefinitionsQuery(string EntityType, bool OnlyActive)
    : IQuery<IReadOnlyList<CustomFieldDefinitionDto>>;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<ListDefinitionsQuery, IReadOnlyList<CustomFieldDefinitionDto>>))]
public sealed class ListDefinitionsQueryHandler
    : IQueryHandler<ListDefinitionsQuery, IReadOnlyList<CustomFieldDefinitionDto>>
{
    private readonly IRepository<CustomFieldDefinition, Guid> _definitions;
    private readonly CustomFieldDefinitionMapper _mapper;

    public ListDefinitionsQueryHandler(
        IRepository<CustomFieldDefinition, Guid> definitions, CustomFieldDefinitionMapper mapper)
    {
        _definitions = definitions;
        _mapper = mapper;
    }

    public async ValueTask<IReadOnlyList<CustomFieldDefinitionDto>> HandleAsync(
        ListDefinitionsQuery query, CancellationToken ct = default)
    {
        var defs = await _definitions.GetAllAsync(
            new DefinitionsByEntityTypeSpecification(null, query.EntityType, query.OnlyActive), ct);
        return defs.OrderBy(d => d.Order).Select(_mapper.ToDto).ToArray();
    }
}

// ── Видимые поля с учётом значений + контекста (JsonLogic) ────────────────────────────────

public sealed record GetVisibleFieldsQuery(
    string EntityType, string EntityId, IReadOnlyDictionary<string, object?>? Context)
    : IQuery<IReadOnlyList<CustomFieldDefinitionDto>>;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetVisibleFieldsQuery, IReadOnlyList<CustomFieldDefinitionDto>>))]
public sealed class GetVisibleFieldsQueryHandler
    : IQueryHandler<GetVisibleFieldsQuery, IReadOnlyList<CustomFieldDefinitionDto>>
{
    private readonly ICustomFieldDefinitionReader _definitions;
    private readonly IRepository<CustomFieldValueSet, Guid> _valueSets;
    private readonly CustomFieldDefinitionMapper _mapper;
    private readonly IExpressionEvaluator _evaluator;

    public GetVisibleFieldsQueryHandler(
        ICustomFieldDefinitionReader definitions,
        IRepository<CustomFieldValueSet, Guid> valueSets,
        CustomFieldDefinitionMapper mapper,
        IExpressionEvaluator evaluator)
    {
        _definitions = definitions;
        _valueSets = valueSets;
        _mapper = mapper;
        _evaluator = evaluator;
    }

    public async ValueTask<IReadOnlyList<CustomFieldDefinitionDto>> HandleAsync(
        GetVisibleFieldsQuery query, CancellationToken ct = default)
    {
        var defs = await _definitions.GetActiveAsync(null, query.EntityType, ct);

        var set = await _valueSets.GetBySpecAsync(
            new ValueSetByEntitySpecification(null, query.EntityType, query.EntityId), ct);
        var values = CustomFieldJson.Deserialize(set?.ValuesJson);

        // Источник переменных JsonLogic: значения сущности + переданный контекст.
        var data = new Dictionary<string, object?>(values);
        if (query.Context is not null)
            foreach (var (k, v) in query.Context)
                data[k] = v;

        var visible = new List<CustomFieldDefinitionDto>(defs.Count);
        foreach (var d in defs)
        {
            if (!string.IsNullOrWhiteSpace(d.VisibilityRule)
                && !await _evaluator.EvaluateBooleanAsync(d.VisibilityRule!, data, ct))
                continue;
            visible.Add(_mapper.ToDto(d));
        }
        return visible;
    }
}

// ── Список зарегистрированных типов (реестр) ─────────────────────────────────────────────

public sealed record ListEntityTypesQuery : IQuery<IReadOnlyList<CustomFieldEntityTypeDto>>;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<ListEntityTypesQuery, IReadOnlyList<CustomFieldEntityTypeDto>>))]
public sealed class ListEntityTypesQueryHandler
    : IQueryHandler<ListEntityTypesQuery, IReadOnlyList<CustomFieldEntityTypeDto>>
{
    private readonly IRepository<CustomFieldEntityType, Guid> _types;
    public ListEntityTypesQueryHandler(IRepository<CustomFieldEntityType, Guid> types) => _types = types;

    public async ValueTask<IReadOnlyList<CustomFieldEntityTypeDto>> HandleAsync(
        ListEntityTypesQuery query, CancellationToken ct = default)
    {
        var types = await _types.GetAllAsync(null, ct);
        return types
            .OrderBy(t => t.Key)
            .Select(t => new CustomFieldEntityTypeDto(t.Key, t.DisplayName, t.OwnerService, t.IdType, t.IsActive))
            .ToArray();
    }
}
