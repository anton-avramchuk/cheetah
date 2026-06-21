using Cheetah.Core.CQRS;
using Cheetah.Core.Domain.Exceptions;
using Cheetah.Core.Events;
using Cheetah.Modules.Workflow.Contracts;
using Cheetah.Modules.Workflow.Domain.Entities;
using Cheetah.Modules.Workflow.Domain.Repositories;

namespace Cheetah.Modules.Workflow.Application.Commands;

// ── Команды ─────────────────────────────────────────────────────────────────────────────────

public sealed record CreateAutomationRuleCommand<TCreateRequest>(TCreateRequest Request) : ICommand<Guid>
    where TCreateRequest : CreateAutomationRuleRequestBase;

public sealed record EnableRuleCommand(Guid RuleId) : ICommand;

public sealed record DisableRuleCommand(Guid RuleId) : ICommand;

public sealed record DeleteRuleCommand(Guid RuleId) : ICommand;

// ── Хендлеры ────────────────────────────────────────────────────────────────────────────────

public sealed class CreateAutomationRuleCommandHandler<TRule, TCreateRequest>(
    IAutomationRuleFactory<TRule, TCreateRequest> factory,
    IAutomationRuleRepository<TRule> repository,
    IEventBus eventBus)
    : ICommandHandler<CreateAutomationRuleCommand<TCreateRequest>, Guid>
    where TRule : AutomationRuleBase
    where TCreateRequest : CreateAutomationRuleRequestBase
{
    public async ValueTask<Guid> HandleAsync(CreateAutomationRuleCommand<TCreateRequest> command, CancellationToken ct = default)
    {
        var rule = factory.Create(command.Request);
        repository.Add(rule);
        await repository.SaveChangesAsync(ct);
        foreach (var e in rule.DomainEvents)
            await eventBus.PublishAsync(e, ct);
        rule.ClearDomainEvents();
        return rule.Id;
    }
}

public abstract class RuleLifecycleHandlerBase<TRule>(IAutomationRuleRepository<TRule> repository, IEventBus eventBus)
    where TRule : AutomationRuleBase
{
    protected readonly IAutomationRuleRepository<TRule> Repository = repository;

    protected async ValueTask<TRule> LoadAsync(Guid id, CancellationToken ct)
        => await Repository.GetByIdAsync(id, includeChildren: true, ct)
           ?? throw new EntityNotFoundException(nameof(AutomationRuleBase), id);

    protected async ValueTask SaveAndPublishAsync(TRule rule, CancellationToken ct)
    {
        await Repository.SaveChangesAsync(ct);
        foreach (var e in rule.DomainEvents)
            await eventBus.PublishAsync(e, ct);
        rule.ClearDomainEvents();
    }
}

public sealed class EnableRuleCommandHandler<TRule>(IAutomationRuleRepository<TRule> repository, IEventBus eventBus)
    : RuleLifecycleHandlerBase<TRule>(repository, eventBus), ICommandHandler<EnableRuleCommand>
    where TRule : AutomationRuleBase
{
    public async ValueTask HandleAsync(EnableRuleCommand command, CancellationToken ct = default)
    {
        var rule = await LoadAsync(command.RuleId, ct);
        rule.Enable();
        await SaveAndPublishAsync(rule, ct);
    }
}

public sealed class DisableRuleCommandHandler<TRule>(IAutomationRuleRepository<TRule> repository, IEventBus eventBus)
    : RuleLifecycleHandlerBase<TRule>(repository, eventBus), ICommandHandler<DisableRuleCommand>
    where TRule : AutomationRuleBase
{
    public async ValueTask HandleAsync(DisableRuleCommand command, CancellationToken ct = default)
    {
        var rule = await LoadAsync(command.RuleId, ct);
        rule.Disable();
        await SaveAndPublishAsync(rule, ct);
    }
}

public sealed class DeleteRuleCommandHandler<TRule>(IAutomationRuleRepository<TRule> repository, IEventBus eventBus)
    : RuleLifecycleHandlerBase<TRule>(repository, eventBus), ICommandHandler<DeleteRuleCommand>
    where TRule : AutomationRuleBase
{
    public async ValueTask HandleAsync(DeleteRuleCommand command, CancellationToken ct = default)
    {
        var rule = await LoadAsync(command.RuleId, ct);
        Repository.Delete(rule);
        await Repository.SaveChangesAsync(ct);
    }
}
