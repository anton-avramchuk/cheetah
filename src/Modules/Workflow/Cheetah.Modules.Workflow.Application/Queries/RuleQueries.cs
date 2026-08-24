using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Specification;
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
    IAutomationRuleRepository<TRule> repository,
    IAutomationRuleProjector<TRule, TDto> projector)
    : IQueryHandler<ListRulesQuery<TDto>, IReadOnlyList<TDto>>
    where TRule : AutomationRuleBase
    where TDto : AutomationRuleDtoBase
{
    public async ValueTask<IReadOnlyList<TDto>> HandleAsync(ListRulesQuery<TDto> query, CancellationToken ct = default)
    {
        ISpecification<TRule>? spec = query.OwnerService is { Length: > 0 } owner
            ? new RulesByOwnerServiceSpecification<TRule>(owner)
            : null;

        if (query.OnlyActive is true)
        {
            var active = new ActiveRulesSpecification<TRule>();
            spec = spec is null ? active : spec.And(active);
        }

        // Отбор и срез — в БД: список правил не материализуется целиком.
        var rules = await repository.ListPageAsync(
            spec, includeChildren: true,
            WorkflowPaging.NormalizeSkip(query.Page, query.Size),
            WorkflowPaging.NormalizeTake(query.Size), ct);

        return rules.Select(projector.ToDto).ToArray();
    }
}

public sealed class ListRunsQueryHandler(IAutomationRunReader reader)
    : IQueryHandler<ListRunsQuery, IReadOnlyList<AutomationRunDto>>
{
    public async ValueTask<IReadOnlyList<AutomationRunDto>> HandleAsync(ListRunsQuery query, CancellationToken ct = default)
    {
        ISpecification<AutomationRun>? spec = query.RuleId is { } ruleId
            ? new RunsByRuleSpecification(ruleId)
            : null;

        if (query.Status is { } status)
        {
            var byStatus = new RunsByStatusSpecification(status);
            spec = spec is null ? byStatus : spec.And(byStatus);
        }

        // Журнал прогонов растёт неограниченно — читаем строго страницей из БД.
        var runs = await reader.ListPageAsync(
            spec,
            WorkflowPaging.NormalizeSkip(query.Page, query.Size),
            WorkflowPaging.NormalizeTake(query.Size), ct);

        return runs.Select(RunMapper.ToDto).ToArray();
    }
}

/// <summary>Границы страницы: клиент не должен уметь запросить журнал целиком.</summary>
internal static class WorkflowPaging
{
    public static int NormalizeTake(int size) => Math.Clamp(
        size <= 0 ? WorkflowConstants.DefaultPageSize : size, 1, WorkflowConstants.MaxPageSize);

    public static int NormalizeSkip(int page, int size) => Math.Max(0, page) * NormalizeTake(size);
}

internal static class RunMapper
{
    public static AutomationRunDto ToDto(AutomationRun run) => new(
        run.Id, run.RuleId, run.EventId, run.EventName, run.Status,
        run.Steps.Select(s => new AutomationRunStepDto(s.Order, s.ActionType, s.Status, s.Error)).ToArray(),
        run.CreatedAt ?? default);
}
