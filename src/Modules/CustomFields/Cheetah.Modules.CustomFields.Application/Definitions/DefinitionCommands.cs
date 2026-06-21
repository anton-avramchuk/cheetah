using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Cheetah.Core.Events;
using Cheetah.Modules.CustomFields.Application.Mapping;
using Cheetah.Modules.CustomFields.Contracts;
using Cheetah.Modules.CustomFields.Domain.Entities;

namespace Cheetah.Modules.CustomFields.Application.Definitions;

// MVP: определения создаются глобальными (TenantId == null). Резолв тенант-скоупа из контекста — follow-up.

// ── Создание ─────────────────────────────────────────────────────────────────────────────

public sealed record CreateCustomFieldDefinitionCommand(CreateCustomFieldDefinitionRequest Request)
    : ICommand<Guid>;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateCustomFieldDefinitionCommand, Guid>))]
public sealed class CreateCustomFieldDefinitionCommandHandler
    : ICommandHandler<CreateCustomFieldDefinitionCommand, Guid>
{
    private readonly IRepository<CustomFieldDefinition, Guid> _definitions;
    private readonly CustomFieldDefinitionMapper _mapper;
    private readonly IEventBus _eventBus;

    public CreateCustomFieldDefinitionCommandHandler(
        IRepository<CustomFieldDefinition, Guid> definitions,
        CustomFieldDefinitionMapper mapper,
        IEventBus eventBus)
    {
        _definitions = definitions;
        _mapper = mapper;
        _eventBus = eventBus;
    }

    public async ValueTask<Guid> HandleAsync(CreateCustomFieldDefinitionCommand command, CancellationToken ct = default)
    {
        var r = command.Request;
        var definition = CustomFieldDefinition.Create(
            tenantId: null, r.EntityType, r.Key, r.Label, r.DataType, r.Required,
            r.Options, _mapper.SerializeRules(r.ValidationRules), r.VisibilityRule, r.Order);

        _definitions.Add(definition);
        await _definitions.SaveChangesAsync(ct);

        foreach (var e in definition.DomainEvents)
            await _eventBus.PublishAsync(e, ct);
        definition.ClearDomainEvents();

        return definition.Id;
    }
}

// ── Изменение метаданных ─────────────────────────────────────────────────────────────────

public sealed record UpdateCustomFieldDefinitionCommand(UpdateCustomFieldDefinitionRequest Request) : ICommand;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<UpdateCustomFieldDefinitionCommand>))]
public sealed class UpdateCustomFieldDefinitionCommandHandler
    : ICommandHandler<UpdateCustomFieldDefinitionCommand>
{
    private readonly IRepository<CustomFieldDefinition, Guid> _definitions;
    private readonly CustomFieldDefinitionMapper _mapper;
    private readonly IEventBus _eventBus;

    public UpdateCustomFieldDefinitionCommandHandler(
        IRepository<CustomFieldDefinition, Guid> definitions,
        CustomFieldDefinitionMapper mapper,
        IEventBus eventBus)
    {
        _definitions = definitions;
        _mapper = mapper;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(UpdateCustomFieldDefinitionCommand command, CancellationToken ct = default)
    {
        var r = command.Request;
        var definition = await _definitions.GetByIdAsync(r.Id, ct)
            ?? throw EntityNotFoundException.For<CustomFieldDefinition>(r.Id);

        definition.UpdateMetadata(r.Label, r.Required, r.Options,
            _mapper.SerializeRules(r.ValidationRules), r.VisibilityRule, r.Order);
        _definitions.Update(definition);
        await _definitions.SaveChangesAsync(ct);

        foreach (var e in definition.DomainEvents)
            await _eventBus.PublishAsync(e, ct);
        definition.ClearDomainEvents();
    }
}

// ── Деактивация (soft-delete) ────────────────────────────────────────────────────────────

public sealed record DeactivateCustomFieldDefinitionCommand(Guid Id) : ICommand;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<DeactivateCustomFieldDefinitionCommand>))]
public sealed class DeactivateCustomFieldDefinitionCommandHandler
    : ICommandHandler<DeactivateCustomFieldDefinitionCommand>
{
    private readonly IRepository<CustomFieldDefinition, Guid> _definitions;
    private readonly IEventBus _eventBus;

    public DeactivateCustomFieldDefinitionCommandHandler(
        IRepository<CustomFieldDefinition, Guid> definitions, IEventBus eventBus)
    {
        _definitions = definitions;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(DeactivateCustomFieldDefinitionCommand command, CancellationToken ct = default)
    {
        var definition = await _definitions.GetByIdAsync(command.Id, ct)
            ?? throw EntityNotFoundException.For<CustomFieldDefinition>(command.Id);

        definition.Deactivate();
        _definitions.Update(definition);
        await _definitions.SaveChangesAsync(ct);

        foreach (var e in definition.DomainEvents)
            await _eventBus.PublishAsync(e, ct);
        definition.ClearDomainEvents();
    }
}
