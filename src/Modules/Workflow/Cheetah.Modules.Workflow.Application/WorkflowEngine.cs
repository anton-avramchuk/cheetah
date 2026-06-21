using System.Text.Json;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.Expressions;
using Cheetah.Modules.Workflow.Domain.Entities;
using Cheetah.Modules.Workflow.Domain.Specifications;
using Cheetah.Modules.Workflow.Shared;
using Cheetah.Workflow;
using Microsoft.Extensions.Logging;

namespace Cheetah.Modules.Workflow.Application;

/// <summary>Движок правил: приём конверта → подбор правил → условие → план → исполнение → журнал.</summary>
public interface IWorkflowEngine
{
    ValueTask HandleTriggerAsync(WorkflowEventEnvelope envelope, CancellationToken ct);
}

/// <summary>
/// Ядро модуля. Чистая оркестрация над портами (<see cref="IRuleMatcher{TRule}"/>,
/// <see cref="IExpressionEvaluator"/>, <see cref="IWorkflowActionExecutor"/>, <see cref="IParameterRenderer"/>,
/// репозиторий журнала) — юнит-тестируется без БД/шины. Generic-движок закрывается конкретным
/// <typeparamref name="TRule"/> наследника через <c>AddWorkflowApplication</c>.
/// </summary>
public sealed class WorkflowEngine<TRule> : IWorkflowEngine where TRule : AutomationRuleBase
{
    private readonly IRuleMatcher<TRule> _matcher;
    private readonly IExpressionEvaluator _expressions;
    private readonly IWorkflowActionExecutor _executor;
    private readonly IParameterRenderer _renderer;
    private readonly IRepository<AutomationRun, Guid> _runs;
    private readonly IEventBus _eventBus;
    private readonly ILogger<WorkflowEngine<TRule>> _logger;

    public WorkflowEngine(
        IRuleMatcher<TRule> matcher,
        IExpressionEvaluator expressions,
        IWorkflowActionExecutor executor,
        IParameterRenderer renderer,
        IRepository<AutomationRun, Guid> runs,
        IEventBus eventBus,
        ILogger<WorkflowEngine<TRule>> logger)
    {
        _matcher = matcher;
        _expressions = expressions;
        _executor = executor;
        _renderer = renderer;
        _runs = runs;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async ValueTask HandleTriggerAsync(WorkflowEventEnvelope envelope, CancellationToken ct)
    {
        var rules = await _matcher.MatchAsync(envelope.EventName, envelope.TenantId, ct);
        if (rules.Count == 0)
            return;

        foreach (var rule in rules)
        {
            // 1. Условие (JsonLogic над payload). Пустое условие → истина.
            if (!string.IsNullOrEmpty(rule.ConditionExpression) &&
                !await _expressions.EvaluateBooleanAsync(rule.ConditionExpression, envelope.Payload, ct))
                continue;

            // 2. Дедуп: правило уже отрабатывало это событие? (бэкстоп — уникальный индекс (RuleId, EventId)).
            if (await _runs.ExistsAsync(new RunByRuleAndEventSpecification(rule.Id, envelope.SourceEventId), ct))
            {
                _logger.LogDebug("Rule {RuleId} already ran for event {EventId} — skipping (dedup).",
                    rule.Id, envelope.SourceEventId);
                continue;
            }

            var run = AutomationRun.Start(rule.Id, envelope, JsonSerializer.Serialize(envelope.Payload));
            _runs.Add(run);
            await _runs.SaveChangesAsync(ct);

            // 3. Действия по Order.
            foreach (var action in rule.Actions)
            {
                try
                {
                    var prms = _renderer.Render(action.Parameters, envelope.Payload);
                    await _executor.ExecuteAsync(action.ActionType,
                        new WorkflowActionContext(rule.Id, run.Id, envelope, prms), ct);
                    run.RecordSuccess(action.ActionType);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Action {Action} of rule {RuleId} failed.", action.ActionType, rule.Id);
                    run.RecordFailure(action.ActionType, ex.Message);
                    if (action.FailureMode == ActionFailureMode.StopRule)
                        break;
                    // ContinueNext / Compensate (Compensate-откат — follow-up через Cheetah.Saga)
                }
            }

            // 4. Завершение журнала + публикация события результата.
            run.Complete();
            await _runs.SaveChangesAsync(ct);
            foreach (var e in run.DomainEvents)
                await _eventBus.PublishAsync(e, ct);
            run.ClearDomainEvents();
        }
    }
}
