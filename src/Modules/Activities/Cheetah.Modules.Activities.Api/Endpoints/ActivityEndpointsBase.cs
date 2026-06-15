using Cheetah.Core.CQRS;
using Cheetah.Modules.Activities.Application.Activities;
using Cheetah.Modules.Activities.Application.Exceptions;
using Cheetah.Modules.Activities.Contracts;
using Cheetah.Modules.Activities.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Cheetah.Modules.Activities.Api.Endpoints;

/// <summary>
/// Абстрактная база CRUD/lifecycle-эндпоинтов активности. Generic над конкретными типами Contracts,
/// которые поставляет наследник. Маршруты строятся на закрытых generic-командах/запросах — конкретный
/// тип сущности здесь не нужен (диспетчер резолвит handler по типу команды). Любой маршрут можно
/// переопределить (методы виртуальные).
/// </summary>
public abstract class ActivityEndpointsBase<TCreateRequest, TUpdateRequest, TDto>
    where TCreateRequest : CreateActivityRequestBase
    where TUpdateRequest : UpdateActivityRequestBase
    where TDto : ActivityDtoBase
{
    protected virtual string RoutePrefix => ActivitiesConstants.DefaultRoutePrefix;

    public void Map(IEndpointRouteBuilder routes)
    {
        var prefix = RoutePrefix.TrimEnd('/');

        routes.MapPost(prefix, CreateAsync).WithName("CreateActivity").WithTags("Activities");
        routes.MapGet(prefix, ListAsync).WithName("ListActivities").WithTags("Activities");
        routes.MapGet($"{prefix}/{{id:guid}}", GetByIdAsync).WithName("GetActivityById").WithTags("Activities");
        routes.MapPut($"{prefix}/{{id:guid}}", UpdateAsync).WithName("UpdateActivity").WithTags("Activities");
        routes.MapPost($"{prefix}/{{id:guid}}/start", StartAsync).WithName("StartActivity").WithTags("Activities");
        routes.MapPost($"{prefix}/{{id:guid}}/complete", CompleteAsync).WithName("CompleteActivity").WithTags("Activities");
        routes.MapPost($"{prefix}/{{id:guid}}/cancel", CancelAsync).WithName("CancelActivity").WithTags("Activities");
        routes.MapPost($"{prefix}/{{id:guid}}/reassign", ReassignAsync).WithName("ReassignActivity").WithTags("Activities");
    }

    protected virtual async Task<IResult> CreateAsync(
        [FromBody] TCreateRequest request,
        [FromServices] IDispatcher dispatcher,
        CancellationToken ct)
    {
        try
        {
            var id = await dispatcher.SendAsync<CreateActivityCommand<TCreateRequest>, Guid>(
                new CreateActivityCommand<TCreateRequest>(request), ct);
            return Results.Created($"/{RoutePrefix.TrimEnd('/')}/{id}", id);
        }
        catch (ActivityValidationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    protected virtual async Task<IResult> ListAsync(
        [FromQuery] Guid? assigneeId,
        [FromQuery] ActivityStatus? status,
        [FromQuery] string? entityType,
        [FromQuery] Guid? entityId,
        [FromQuery] DateTimeOffset? dueBefore,
        [FromServices] IDispatcher dispatcher,
        CancellationToken ct)
    {
        var items = await dispatcher.QueryAsync<ListActivitiesQuery<TDto>, IReadOnlyList<TDto>>(
            new ListActivitiesQuery<TDto>(assigneeId, status, entityType, entityId, dueBefore), ct);
        return Results.Ok(items);
    }

    protected virtual async Task<IResult> GetByIdAsync(
        [FromRoute] Guid id,
        [FromServices] IDispatcher dispatcher,
        CancellationToken ct)
    {
        var dto = await dispatcher.QueryAsync<GetActivityByIdQuery<TDto>, TDto?>(
            new GetActivityByIdQuery<TDto>(id), ct);
        return dto is null ? Results.NotFound() : Results.Ok(dto);
    }

    protected virtual async Task<IResult> UpdateAsync(
        [FromRoute] Guid id,
        [FromBody] TUpdateRequest request,
        [FromServices] IDispatcher dispatcher,
        CancellationToken ct)
    {
        try
        {
            await dispatcher.SendAsync(new UpdateActivityCommand<TUpdateRequest>(id, request), ct);
            return Results.NoContent();
        }
        catch (ActivityValidationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    protected virtual async Task<IResult> StartAsync(
        [FromRoute] Guid id,
        [FromServices] IDispatcher dispatcher,
        CancellationToken ct)
        => await SendLifecycleAsync(dispatcher, new StartActivityCommand(id), ct);

    protected virtual async Task<IResult> CompleteAsync(
        [FromRoute] Guid id,
        [FromBody] CompleteActivityBody body,
        [FromServices] IDispatcher dispatcher,
        CancellationToken ct)
        => await SendLifecycleAsync(dispatcher, new CompleteActivityCommand(id, body.CompletedBy, body.Result), ct);

    protected virtual async Task<IResult> CancelAsync(
        [FromRoute] Guid id,
        [FromServices] IDispatcher dispatcher,
        CancellationToken ct)
        => await SendLifecycleAsync(dispatcher, new CancelActivityCommand(id), ct);

    protected virtual async Task<IResult> ReassignAsync(
        [FromRoute] Guid id,
        [FromBody] ReassignActivityBody body,
        [FromServices] IDispatcher dispatcher,
        CancellationToken ct)
        => await SendLifecycleAsync(dispatcher, new ReassignActivityCommand(id, body.NewAssigneeId), ct);

    private static async Task<IResult> SendLifecycleAsync<TCommand>(
        IDispatcher dispatcher, TCommand command, CancellationToken ct)
        where TCommand : ICommand
    {
        try
        {
            await dispatcher.SendAsync(command, ct);
            return Results.NoContent();
        }
        catch (ActivityValidationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
}

/// <summary>Тело запроса завершения активности.</summary>
public sealed record CompleteActivityBody(Guid CompletedBy, string? Result = null);

/// <summary>Тело запроса смены исполнителя.</summary>
public sealed record ReassignActivityBody(Guid NewAssigneeId);
