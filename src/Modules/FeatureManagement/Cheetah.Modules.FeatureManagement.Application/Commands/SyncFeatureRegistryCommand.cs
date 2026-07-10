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
/// <para>
/// Объявленный кодом <c>ParentKey</c> применяется и к существующему флагу — связь каскада должна
/// воспроизводиться на любой БД. Дескриптор без родителя связь не снимает: выставленную админом руками
/// иерархию рестарт сервиса не ломает.
/// </para>
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
        // Публикуем только события, порождённые ЭТОЙ синхронизацией: у существующего флага могут висеть
        // чужие неопубликованные события, переотправлять их — не наше дело.
        var raised = new List<(TFlag Flag, int PublishedFrom)>();

        foreach (var descriptor in command.Descriptors)
        {
            var existing = await _repository.GetByKeyAsync(descriptor.Key, includeChildren: false, ct);
            if (existing is not null)
            {
                var before = existing.DomainEvents.Count;
                existing.RegisterMetadata(descriptor.Name, descriptor.Description, descriptor.ValueType);

                // SetParent молчит, если родитель не изменился — рестарт сервиса не плодит событий.
                if (descriptor.ParentKey is not null)
                    existing.SetParent(descriptor.ParentKey);

                if (existing.DomainEvents.Count > before)
                    raised.Add((existing, before));
            }
            else
            {
                var flag = _factory.CreateFromDescriptor(descriptor);
                _repository.Add(flag);
                raised.Add((flag, 0));
            }
        }

        // Публикация ДО SaveChanges: события ложатся в outbox той же транзакцией.
        foreach (var (flag, publishedFrom) in raised)
        {
            foreach (var e in flag.DomainEvents.Skip(publishedFrom))
                await _eventBus.PublishAsync(e, ct);
            flag.ClearDomainEvents();
        }

        await _repository.SaveChangesAsync(ct);
    }
}
