using Cheetah.Core.CQRS;
using Cheetah.Modules.Workflow.Application.Commands;
using Cheetah.Modules.Workflow.Application.Queries;
using Cheetah.Modules.Workflow.Contracts;
using Cheetah.Modules.Workflow.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Modules.Workflow.Application;

public static class WorkflowApplicationExtensions
{
    /// <summary>
    /// Регистрирует движок и закрытые generic-handler'ы Workflow под конкретные типы наследника. Открытые
    /// generic нельзя зарегистрировать через <c>[Export]</c>, поэтому собираем их здесь (как
    /// <c>AddFeatureManagementApplication</c>).
    /// </summary>
    public static IServiceCollection AddWorkflowApplication<TRule, TCreateRequest, TDto, TFactory, TProjector>(
        this IServiceCollection services)
        where TRule : AutomationRuleBase
        where TCreateRequest : CreateAutomationRuleRequestBase
        where TDto : AutomationRuleDtoBase
        where TFactory : class, IAutomationRuleFactory<TRule, TCreateRequest>
        where TProjector : class, IAutomationRuleProjector<TRule, TDto>
    {
        services.AddScoped<IAutomationRuleFactory<TRule, TCreateRequest>, TFactory>();
        services.AddScoped<IAutomationRuleProjector<TRule, TDto>, TProjector>();

        services.AddScoped<IWorkflowEngine, WorkflowEngine<TRule>>();

        // Команды
        services.AddScoped<ICommandHandler<CreateAutomationRuleCommand<TCreateRequest>, Guid>,
            CreateAutomationRuleCommandHandler<TRule, TCreateRequest>>();
        services.AddScoped<ICommandHandler<EnableRuleCommand>, EnableRuleCommandHandler<TRule>>();
        services.AddScoped<ICommandHandler<DisableRuleCommand>, DisableRuleCommandHandler<TRule>>();
        services.AddScoped<ICommandHandler<DeleteRuleCommand>, DeleteRuleCommandHandler<TRule>>();
        services.AddScoped<ICommandHandler<TestRuleCommand, TestRunResult>, TestRuleCommandHandler<TRule>>();

        // Запросы
        services.AddScoped<IQueryHandler<GetRuleByIdQuery<TDto>, TDto?>, GetRuleByIdQueryHandler<TRule, TDto>>();
        services.AddScoped<IQueryHandler<ListRulesQuery<TDto>, IReadOnlyList<TDto>>, ListRulesQueryHandler<TRule, TDto>>();
        services.AddScoped<IQueryHandler<ListRunsQuery, IReadOnlyList<AutomationRunDto>>, ListRunsQueryHandler>();

        return services;
    }
}
