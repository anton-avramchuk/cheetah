using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Cheetah.Modules.CustomFields.Contracts;
using Cheetah.Modules.CustomFields.Domain.Entities;
using Cheetah.Modules.CustomFields.Domain.Specifications;

namespace Cheetah.Modules.CustomFields.Application.Registry;

/// <summary>
/// Идемпотентный upsert каталога расширяемых типов (registry/sync при старте сервисов). Существующий
/// тип обновляет метаданные; предопределённые поля апсёртятся как глобальные определения (TenantId==null),
/// НЕ затирая уже добавленные администраторами поля тенантов.
/// </summary>
public sealed record SyncEntityTypeRegistryCommand(IReadOnlyList<CustomFieldEntityTypeDescriptor> Descriptors)
    : ICommand;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<SyncEntityTypeRegistryCommand>))]
public sealed class SyncEntityTypeRegistryCommandHandler : ICommandHandler<SyncEntityTypeRegistryCommand>
{
    private readonly IRepository<CustomFieldEntityType, Guid> _types;
    private readonly IRepository<CustomFieldDefinition, Guid> _definitions;
    private readonly IEventBus _eventBus;

    public SyncEntityTypeRegistryCommandHandler(
        IRepository<CustomFieldEntityType, Guid> types,
        IRepository<CustomFieldDefinition, Guid> definitions,
        IEventBus eventBus)
    {
        _types = types;
        _definitions = definitions;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(SyncEntityTypeRegistryCommand command, CancellationToken ct = default)
    {
        var newDefinitions = new List<CustomFieldDefinition>();

        foreach (var d in command.Descriptors)
        {
            // 1. Upsert типа по Key.
            var type = await _types.GetBySpecAsync(new EntityTypeByKeySpecification(d.Key), ct);
            if (type is null)
            {
                type = CustomFieldEntityType.Create(d.Key, d.DisplayName, d.OwnerService, d.IdType);
                _types.Add(type);
            }
            else
            {
                type.Update(d.DisplayName, d.OwnerService, d.IdType);
                _types.Update(type);
            }

            // 2. Предопределённые поля → глобальные определения (только если ещё нет — без затирания).
            if (d.PredefinedFields is null) continue;
            foreach (var f in d.PredefinedFields)
            {
                var exists = await _definitions.ExistsAsync(
                    new DefinitionByKeySpecification(null, d.Key, f.Key), ct);
                if (exists) continue;

                var def = CustomFieldDefinition.Create(
                    tenantId: null, d.Key, f.Key, f.Label, f.DataType, f.Required,
                    f.Options, validationRulesJson: null, visibilityRule: null, f.Order);
                _definitions.Add(def);
                newDefinitions.Add(def);
            }
        }

        await _types.SaveChangesAsync(ct);

        foreach (var def in newDefinitions)
        {
            foreach (var e in def.DomainEvents)
                await _eventBus.PublishAsync(e, ct);
            def.ClearDomainEvents();
        }
    }
}
