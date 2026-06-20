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
        await Repository.SaveChangesAsync(ct);
        foreach (var e in flag.DomainEvents)
            await _eventBus.PublishAsync(e, ct);
        flag.ClearDomainEvents();
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
