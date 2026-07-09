using Cheetah.Core.CQRS;
using Cheetah.Core.Events;
using Cheetah.Modules.FeatureManagement.Application.Abstractions;
using Cheetah.Modules.FeatureManagement.Contracts;
using Cheetah.Modules.FeatureManagement.Domain.Entities;
using Cheetah.Modules.FeatureManagement.Domain.Repositories;

namespace Cheetah.Modules.FeatureManagement.Application.Commands;

/// <summary>
/// Идемпотентный upsert дескрипторов флагов в каталог (registry/sync). Новый ключ создаётся (выкл по
/// умолчанию), существующий обновляет только метаданные — НЕ трогает <c>Enabled</c>/таргетинг.
/// </summary>
public sealed record SyncFeatureRegistryCommand(IReadOnlyList<FeatureDefinitionDescriptor> Descriptors) : ICommand;

public class SyncFeatureRegistryCommandHandler<TFlag, TCreateRequest>
    : ICommandHandler<SyncFeatureRegistryCommand>
    where TFlag : FeatureFlagBase
    where TCreateRequest : CreateFeatureFlagRequestBase
{
    private readonly IFeatureFlagRepository<TFlag> _repository;
    private readonly IFeatureFlagFactory<TFlag, TCreateRequest> _factory;
    private readonly IEventBus _eventBus;

    public SyncFeatureRegistryCommandHandler(
        IFeatureFlagRepository<TFlag> repository,
        IFeatureFlagFactory<TFlag, TCreateRequest> factory,
        IEventBus eventBus)
    {
        _repository = repository;
        _factory = factory;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(SyncFeatureRegistryCommand command, CancellationToken ct = default)
    {
        var created = new List<TFlag>();

        foreach (var descriptor in command.Descriptors)
        {
            var existing = await _repository.GetByKeyAsync(descriptor.Key, includeChildren: false, ct);
            if (existing is not null)
            {
                existing.RegisterMetadata(descriptor.Name, descriptor.Description, descriptor.ValueType);
            }
            else
            {
                var flag = _factory.CreateFromDescriptor(descriptor);
                _repository.Add(flag);
                created.Add(flag);
            }
        }

        // Публикация ДО SaveChanges: события создания ложатся в outbox той же транзакцией.
        foreach (var flag in created)
        {
            foreach (var e in flag.DomainEvents)
                await _eventBus.PublishAsync(e, ct);
            flag.ClearDomainEvents();
        }

        await _repository.SaveChangesAsync(ct);
    }
}
