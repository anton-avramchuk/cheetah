using Cheetah.Core.CQRS;
using Cheetah.Core.Domain.Exceptions;
using Cheetah.Expressions;
using Cheetah.Modules.Workflow.Contracts;
using Cheetah.Modules.Workflow.Domain.Entities;
using Cheetah.Modules.Workflow.Domain.Repositories;
using Cheetah.Workflow;

namespace Cheetah.Modules.Workflow.Application.Commands;

/// <summary>Сухой прогон правила: проверяет условие и возвращает план действий, НИЧЕГО не исполняя.</summary>
public sealed record TestRuleCommand(Guid RuleId, WorkflowEventEnvelope SampleEvent) : ICommand<TestRunResult>;

public sealed class TestRuleCommandHandler<TRule>(
    IAutomationRuleRepository<TRule> repository,
    IExpressionEvaluator expressions,
    IParameterRenderer renderer)
    : ICommandHandler<TestRuleCommand, TestRunResult>
    where TRule : AutomationRuleBase
{
    public async ValueTask<TestRunResult> HandleAsync(TestRuleCommand command, CancellationToken ct = default)
    {
        var rule = await repository.GetByIdAsync(command.RuleId, includeChildren: true, ct)
            ?? throw new EntityNotFoundException(nameof(AutomationRuleBase), command.RuleId);

        var matched = string.IsNullOrEmpty(rule.ConditionExpression)
            || await expressions.EvaluateBooleanAsync(rule.ConditionExpression, command.SampleEvent.Payload, ct);

        var planned = matched
            ? rule.Actions
                .Select(a => new PlannedActionDto(a.Order, a.ActionType,
                    renderer.Render(a.Parameters, command.SampleEvent.Payload)))
                .ToArray()
            : [];

        return new TestRunResult(matched, planned);
    }
}
