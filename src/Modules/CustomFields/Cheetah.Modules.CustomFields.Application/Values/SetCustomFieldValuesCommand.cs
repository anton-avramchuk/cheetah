using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Cheetah.Modules.CustomFields.Application.Mapping;
using Cheetah.Modules.CustomFields.Contracts;
using Cheetah.Modules.CustomFields.Domain.Abstractions;
using Cheetah.Modules.CustomFields.Domain.Entities;
using Cheetah.Modules.CustomFields.Domain.Specifications;
using Cheetah.Validation;

namespace Cheetah.Modules.CustomFields.Application.Values;

/// <summary>Upsert набора значений кастомных полей сущности с валидацией по активным определениям.</summary>
public sealed record SetCustomFieldValuesCommand(SetCustomFieldValuesRequest Request) : ICommand;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<SetCustomFieldValuesCommand>))]
public sealed class SetCustomFieldValuesCommandHandler : ICommandHandler<SetCustomFieldValuesCommand>
{
    private readonly ICustomFieldDefinitionReader _definitions;
    private readonly IRepository<CustomFieldValueSet, Guid> _valueSets;
    private readonly CustomFieldDefinitionMapper _mapper;
    private readonly IValidationEngine _validation;
    private readonly IServiceProvider _services;
    private readonly IEventBus _eventBus;

    public SetCustomFieldValuesCommandHandler(
        ICustomFieldDefinitionReader definitions,
        IRepository<CustomFieldValueSet, Guid> valueSets,
        CustomFieldDefinitionMapper mapper,
        IValidationEngine validation,
        IServiceProvider services,
        IEventBus eventBus)
    {
        _definitions = definitions;
        _valueSets = valueSets;
        _mapper = mapper;
        _validation = validation;
        _services = services;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(SetCustomFieldValuesCommand command, CancellationToken ct = default)
    {
        var req = command.Request;
        Guid? tenantId = null;   // MVP: глобальный скоуп; резолв тенанта — follow-up.

        var defs = await _definitions.GetActiveAsync(tenantId, req.EntityType, ct);
        var values = CustomFieldJson.Normalize(req.Values);

        // 1. Валидация: правила каждого поля (Required + декларированные) над нормализованными значениями.
        var fields = defs
            .Select(d => new FieldValidation(d.Key, values.GetValueOrDefault(d.Key), _mapper.BuildRules(d)))
            .ToList();

        var result = await _validation.ValidateAsync(fields, values, _services, ct);
        if (!result.IsValid)
            throw new CustomFieldsValidationException(result.Errors);

        // 2. Оставляем только значения зарегистрированных ключей.
        var allowed = defs.Select(d => d.Key).ToHashSet();
        var clean = values.Where(kv => allowed.Contains(kv.Key)).ToDictionary(kv => kv.Key, kv => kv.Value);
        var json = CustomFieldJson.Serialize(clean);

        // 3. Upsert набора значений.
        var set = await _valueSets.GetBySpecAsync(
            new ValueSetByEntitySpecification(tenantId, req.EntityType, req.EntityId), ct);
        if (set is null)
        {
            set = CustomFieldValueSet.Create(tenantId, req.EntityType, req.EntityId, json);
            _valueSets.Add(set);
        }
        else
        {
            set.Replace(json);
            _valueSets.Update(set);
        }

        await _valueSets.SaveChangesAsync(ct);

        foreach (var e in set.DomainEvents)
            await _eventBus.PublishAsync(e, ct);
        set.ClearDomainEvents();
    }
}
