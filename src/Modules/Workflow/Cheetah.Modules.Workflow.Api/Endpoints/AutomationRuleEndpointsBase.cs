using Cheetah.Core.CQRS;
using Cheetah.Core.Domain.Exceptions;
using Cheetah.Modules.Workflow.Application.Commands;
using Cheetah.Modules.Workflow.Application.Queries;
using Cheetah.Modules.Workflow.Contracts;
using Cheetah.Modules.Workflow.Shared;
using Cheetah.Workflow;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Cheetah.Modules.Workflow.Api.Endpoints;

/// <summary>
/// Абстрактная база эндпоинтов Workflow: админка правил, dry-run теста и журнал срабатываний. Generic над
/// конкретными Contracts наследника; маршруты — на закрытых generic-командах/запросах. Все методы
/// виртуальные (можно переопределить).
/// </summary>
public abstract class AutomationRuleEndpointsBase<TCreateRequest, TDto>
    where TCreateRequest : CreateAutomationRuleRequestBase
    where TDto : AutomationRuleDtoBase
{
    protected virtual string RoutePrefix => "/api/automation";
    private const string Tag = "Workflow";

    public void Map(IEndpointRouteBuilder routes)
    {
        var p = RoutePrefix.TrimEnd('/');

        routes.MapPost($"{p}/rules", CreateAsync).WithName("CreateAutomationRule").WithTags(Tag);
        routes.MapGet($"{p}/rules", ListAsync).WithName("ListAutomationRules").WithTags(Tag);
        routes.MapGet($"{p}/rules/{{id:guid}}", GetByIdAsync).WithName("GetAutomationRule").WithTags(Tag);
        routes.MapPost($"{p}/rules/{{id:guid}}/enable", EnableAsync).WithName("EnableAutomationRule").WithTags(Tag);
        routes.MapPost($"{p}/rules/{{id:guid}}/disable", DisableAsync).WithName("DisableAutomationRule").WithTags(Tag);
        routes.MapDelete($"{p}/rules/{{id:guid}}", DeleteAsync).WithName("DeleteAutomationRule").WithTags(Tag);
        routes.MapPost($"{p}/rules/{{id:guid}}/test", TestAsync).WithName("TestAutomationRule").WithTags(Tag);
        routes.MapGet($"{p}/runs", ListRunsAsync).WithName("ListAutomationRuns").WithTags(Tag);
    }

    protected virtual async Task<IResult> CreateAsync(
        [FromBody] TCreateRequest request, [FromServices] IDispatcher dispatcher, CancellationToken ct)
    {
        var id = await dispatcher.SendAsync<CreateAutomationRuleCommand<TCreateRequest>, Guid>(
            new CreateAutomationRuleCommand<TCreateRequest>(request), ct);
        return Results.Created($"{RoutePrefix.TrimEnd('/')}/rules/{id}", id);
    }

    protected virtual async Task<IResult> ListAsync(
        [FromQuery] string? ownerService, [FromQuery] bool? onlyActive,
        [FromQuery] int page, [FromQuery] int size,
        [FromServices] IDispatcher dispatcher, CancellationToken ct)
    {
        var items = await dispatcher.QueryAsync<ListRulesQuery<TDto>, IReadOnlyList<TDto>>(
            new ListRulesQuery<TDto>(ownerService, onlyActive, page, size <= 0 ? 50 : size), ct);
        return Results.Ok(items);
    }

    protected virtual async Task<IResult> GetByIdAsync(
        [FromRoute] Guid id, [FromServices] IDispatcher dispatcher, CancellationToken ct)
    {
        var dto = await dispatcher.QueryAsync<GetRuleByIdQuery<TDto>, TDto?>(new GetRuleByIdQuery<TDto>(id), ct);
        return dto is null ? Results.NotFound() : Results.Ok(dto);
    }

    protected virtual Task<IResult> EnableAsync(
        [FromRoute] Guid id, [FromServices] IDispatcher dispatcher, CancellationToken ct)
        => SendAsync(dispatcher, new EnableRuleCommand(id), ct);

    protected virtual Task<IResult> DisableAsync(
        [FromRoute] Guid id, [FromServices] IDispatcher dispatcher, CancellationToken ct)
        => SendAsync(dispatcher, new DisableRuleCommand(id), ct);

    protected virtual Task<IResult> DeleteAsync(
        [FromRoute] Guid id, [FromServices] IDispatcher dispatcher, CancellationToken ct)
        => SendAsync(dispatcher, new DeleteRuleCommand(id), ct);

    protected virtual async Task<IResult> TestAsync(
        [FromRoute] Guid id, [FromBody] WorkflowEventEnvelope sampleEvent,
        [FromServices] IDispatcher dispatcher, CancellationToken ct)
    {
        try
        {
            var result = await dispatcher.SendAsync<TestRuleCommand, TestRunResult>(
                new TestRuleCommand(id, sampleEvent), ct);
            return Results.Ok(result);
        }
        catch (EntityNotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
    }

    protected virtual async Task<IResult> ListRunsAsync(
        [FromQuery] Guid? ruleId, [FromQuery] RunStatus? status,
        [FromQuery] int page, [FromQuery] int size,
        [FromServices] IDispatcher dispatcher, CancellationToken ct)
    {
        var items = await dispatcher.QueryAsync<ListRunsQuery, IReadOnlyList<AutomationRunDto>>(
            new ListRunsQuery(ruleId, status, page, size <= 0 ? 50 : size), ct);
        return Results.Ok(items);
    }

    private static async Task<IResult> SendAsync<TCommand>(IDispatcher dispatcher, TCommand command, CancellationToken ct)
        where TCommand : ICommand
    {
        try
        {
            await dispatcher.SendAsync(command, ct);
            return Results.NoContent();
        }
        catch (EntityNotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
    }
}
