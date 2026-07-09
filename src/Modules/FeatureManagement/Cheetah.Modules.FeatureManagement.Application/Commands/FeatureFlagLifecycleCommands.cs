using Cheetah.Core.CQRS;
using Cheetah.Core.Domain.Exceptions;
using Cheetah.Core.Events;
using Cheetah.Modules.FeatureManagement.Contracts;
using Cheetah.Modules.FeatureManagement.Domain.Entities;
using Cheetah.Modules.FeatureManagement.Domain.Repositories;

namespace Cheetah.Modules.FeatureManagement.Application.Commands;

// ── Команды (по ключу флага) ───────────────────────────────────────────────────────────────

/// <summary>Включить флаг (kill-switch on).</summary>
public sealed record EnableFeatureFlagCommand(string Key) : ICommand;

/// <summary>Выключить флаг (kill-switch off).</summary>
public sealed record DisableFeatureFlagCommand(string Key) : ICommand;

/// <summary>Полностью заменить набор правил таргетинга.</summary>
public sealed record SetTargetingCommand(string Key, IReadOnlyList<TargetingRuleDto> Rules) : ICommand;

/// <summary>Установить/заменить override для тенанта.</summary>
public sealed record SetTenantOverrideCommand(string Key, Guid TenantId, bool Enabled, IReadOnlyList<TargetingRuleDto> Rules) : ICommand;

/// <summary>Сменить/снять родителя флага (каскад). <c>ParentKey == null</c> — сделать флаг верхнего уровня.</summary>
public sealed record SetParentFeatureFlagCommand(string Key, string? ParentKey) : ICommand;

// ── Базовый хендлер: загрузка по ключу + публикация событий ─────────────────────────────────

public abstract class FeatureFlagCommandHandlerBase<TFlag>
    where TFlag : FeatureFlagBase
{
    protected readonly IFeatureFlagRepository<TFlag> Repository;
    private readonly IEventBus _eventBus;

    protected FeatureFlagCommandHandlerBase(IFeatureFlagRepository<TFlag> repository, IEventBus eventBus)
    {
        Repository = repository;
        _eventBus = eventBus;
    }

    protected async ValueTask<TFlag> LoadAsync(string key, bool includeChildren, CancellationToken ct)
        => await Repository.GetByKeyAsync(key, includeChildren, ct)
           ?? throw new EntityNotFoundException(nameof(FeatureFlagBase), key);

    protected async ValueTask SaveAndPublishAsync(TFlag flag, CancellationToken ct)
    {
        // Публикуем ДО SaveChanges: OutboxEventBus кладёт события в OutboxMessages того же
        // DbContext, и один SaveChangesAsync коммитит флаг + outbox-строки атомарно.
        foreach (var e in flag.DomainEvents)
            await _eventBus.PublishAsync(e, ct);
        flag.ClearDomainEvents();

        await Repository.SaveChangesAsync(ct);
    }
}

public sealed class EnableFeatureFlagCommandHandler<TFlag>(IFeatureFlagRepository<TFlag> repository, IEventBus eventBus)
    : FeatureFlagCommandHandlerBase<TFlag>(repository, eventBus), ICommandHandler<EnableFeatureFlagCommand>
    where TFlag : FeatureFlagBase
{
    public async ValueTask HandleAsync(EnableFeatureFlagCommand command, CancellationToken ct = default)
    {
        var flag = await LoadAsync(command.Key, includeChildren: false, ct);
        flag.Enable();
        await SaveAndPublishAsync(flag, ct);
    }
}

public sealed class DisableFeatureFlagCommandHandler<TFlag>(IFeatureFlagRepository<TFlag> repository, IEventBus eventBus)
    : FeatureFlagCommandHandlerBase<TFlag>(repository, eventBus), ICommandHandler<DisableFeatureFlagCommand>
    where TFlag : FeatureFlagBase
{
    public async ValueTask HandleAsync(DisableFeatureFlagCommand command, CancellationToken ct = default)
    {
        var flag = await LoadAsync(command.Key, includeChildren: false, ct);
        flag.Disable();
        await SaveAndPublishAsync(flag, ct);
    }
}

public sealed class SetTargetingCommandHandler<TFlag>(IFeatureFlagRepository<TFlag> repository, IEventBus eventBus)
    : FeatureFlagCommandHandlerBase<TFlag>(repository, eventBus), ICommandHandler<SetTargetingCommand>
    where TFlag : FeatureFlagBase
{
    public async ValueTask HandleAsync(SetTargetingCommand command, CancellationToken ct = default)
    {
        var flag = await LoadAsync(command.Key, includeChildren: true, ct);
        flag.SetTargeting(TargetingRuleMapper.ToEntities(flag.Id, command.Rules));
        await SaveAndPublishAsync(flag, ct);
    }
}

public sealed class SetTenantOverrideCommandHandler<TFlag>(IFeatureFlagRepository<TFlag> repository, IEventBus eventBus)
    : FeatureFlagCommandHandlerBase<TFlag>(repository, eventBus), ICommandHandler<SetTenantOverrideCommand>
    where TFlag : FeatureFlagBase
{
    public async ValueTask HandleAsync(SetTenantOverrideCommand command, CancellationToken ct = default)
    {
        var flag = await LoadAsync(command.Key, includeChildren: true, ct);
        var rulesJson = command.Rules.Count > 0 ? TargetingRuleMapper.SerializeRules(command.Rules) : null;
        flag.SetTenantOverride(TenantOverride.Create(flag.Id, command.TenantId, command.Enabled, rulesJson));
        await SaveAndPublishAsync(flag, ct);
    }
}

public sealed class SetParentFeatureFlagCommandHandler<TFlag>(IFeatureFlagRepository<TFlag> repository, IEventBus eventBus)
    : FeatureFlagCommandHandlerBase<TFlag>(repository, eventBus), ICommandHandler<SetParentFeatureFlagCommand>
    where TFlag : FeatureFlagBase
{
    /// <summary>Максимальная глубина цепочки родителей — защита от случайно оставшихся циклов в данных.</summary>
    private const int MaxDepth = 32;

    public async ValueTask HandleAsync(SetParentFeatureFlagCommand command, CancellationToken ct = default)
    {
        var flag = await LoadAsync(command.Key, includeChildren: false, ct);

        if (command.ParentKey is { } parentKey)
        {
            if (string.Equals(parentKey, command.Key, StringComparison.Ordinal))
                throw new InvalidOperationException("Флаг не может быть родителем самому себе.");

            // Строим карту Key -> ParentKey по всему графу флагов, чтобы обнаружить цикл ДО записи:
            // если, поднимаясь от предполагаемого родителя вверх по цепочке, встретим ключ самого
            // флага — новая связь замкнёт цикл.
            var all = await Repository.ListAsync(spec: null, includeChildren: false, ct);
            var parentByKey = all.ToDictionary(f => f.Key, f => f.ParentKey, StringComparer.Ordinal);

            if (!parentByKey.ContainsKey(parentKey))
                throw new EntityNotFoundException(nameof(FeatureFlagBase), parentKey);

            var current = parentKey;
            for (var depth = 0; current is not null; depth++)
            {
                if (depth >= MaxDepth)
                    throw new InvalidOperationException($"Цепочка родителей флага '{parentKey}' слишком длинная.");
                if (string.Equals(current, command.Key, StringComparison.Ordinal))
                    throw new InvalidOperationException(
                        $"Флаг '{parentKey}' не может стать родителем '{command.Key}' — это создаст цикл.");

                parentByKey.TryGetValue(current, out current);
            }
        }

        flag.SetParent(command.ParentKey);
        await SaveAndPublishAsync(flag, ct);
    }
}
