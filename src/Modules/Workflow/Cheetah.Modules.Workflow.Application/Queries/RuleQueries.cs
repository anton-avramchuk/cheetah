using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Modules.Workflow.Contracts;
using Cheetah.Modules.Workflow.Domain.Entities;
using Cheetah.Modules.Workflow.Domain.Repositories;
using Cheetah.Modules.Workflow.Domain.Specifications;
using Cheetah.Modules.Workflow.Shared;

namespace Cheetah.Modules.Workflow.Application.Queries;

// ── Запросы ─────────────────────────────────────────────────────────────────────────────────

public sealed record GetRuleByIdQuery<TDto>(Guid RuleId) : IQuery<TDto?> where TDto : AutomationRuleDtoBase;

public sealed record ListRulesQuery<TDto>(string? OwnerService, bool? OnlyActive, int Page, int Size)
    : IQuery<IReadOnlyList<TDto>> where TDto : AutomationRuleDtoBase;

public sealed record ListRunsQuery(Guid? RuleId, RunStatus? Status, int Page, int Size)
    : IQuery<IReadOnlyList<AutomationRunDto>>;

// ── Хендлеры ────────────────────────────────────────────────────────────────────────────────

public sealed class GetRuleByIdQueryHandler<TRule, TDto>(
    IAutomationRuleRepository<TRule> repository,
    IAutomationRuleProjector<TRule, TDto> projector)
    : IQueryHandler<GetRuleByIdQuery<TDto>, TDto?>
    where TRule : AutomationRuleBase
    where TDto : AutomationRuleDtoBase
{
    public async ValueTask<TDto?> HandleAsync(GetRuleByIdQuery<TDto> query, CancellationToken ct = default)
    {
        var rule = await repository.GetByIdAsync(query.RuleId, includeChildren: true, ct);
        return rule is null ? null : projector.ToDto(rule);
    }
}

public sealed class ListRulesQueryHandler<TRule, TDto>(
    IReadOnlyRepository<TRule, Guid> repository,
    IAutomationRuleProjector<TRule, TDto> projector)
    : IQueryHandler<ListRulesQuery<TDto>, IReadOnlyList<TDto>>
    where TRule : AutomationRuleBase
    where TDto : AutomationRuleDtoBase
{
    public async ValueTask<IReadOnlyList<TDto>> HandleAsync(ListRulesQuery<TDto> query, CancellationToken ct = default)
    {
        var spec = query.OwnerService is { Length: > 0 } owner
            ? new RulesByOwnerServiceSpecification<TRule>(owner)
            : null;
        var rules = await repository.GetAllAsync(spec, ct);

        IEnumerable<TRule> filtered = rules;
        if (query.OnlyActive is true)
            filtered = filtered.Where(r => r.IsActive);

        return filtered
            .Skip(Math.Max(0, query.Page) * Math.Max(1, query.Size))
            .Take(Math.Max(1, query.Size))
            .Select(projector.ToDto)
            .ToArray();
    }
}

public sealed class ListRunsQueryHandler(IReadOnlyRepository<AutomationRun, Guid> repository)
    : IQueryHandler<ListRunsQuery, IReadOnlyList<AutomationRunDto>>
{
    public async ValueTask<IReadOnlyList<AutomationRunDto>> HandleAsync(ListRunsQuery query, CancellationToken ct = default)
    {
        var runs = await repository.GetAllAsync(null, ct);

        IEnumerable<AutomationRun> filtered = runs;
        if (query.RuleId is { } ruleId)
            filtered = filtered.Where(r => r.RuleId == ruleId);
        if (query.Status is { } status)
            filtered = filtered.Where(r => r.Status == status);

        return filtered
            .OrderByDescending(r => r.CreatedAt)
            .Skip(Math.Max(0, query.Page) * Math.Max(1, query.Size))
            .Take(Math.Max(1, query.Size))
            .Select(RunMapper.ToDto)
            .ToArray();
    }
}

internal static class RunMapper
{
    public static AutomationRunDto ToDto(AutomationRun run) => new(
        run.Id, run.RuleId, run.EventId, run.EventName, run.Status,
        run.Steps.Select(s => new AutomationRunStepDto(s.Order, s.ActionType, s.Status, s.Error)).ToArray(),
        run.CreatedAt ?? default);
}
