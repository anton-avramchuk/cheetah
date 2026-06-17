using Cheetah.Core.CQRS;
using Cheetah.Modules.NotesTimeline.Application.Exceptions;
using Cheetah.Modules.NotesTimeline.Application.Notes;
using Cheetah.Modules.NotesTimeline.Contracts;
using Cheetah.Modules.NotesTimeline.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Cheetah.Modules.NotesTimeline.Api.Endpoints;

/// <summary>
/// Абстрактная база CRUD/lifecycle-эндпоинтов заметки. Generic над конкретными типами Contracts,
/// которые поставляет наследник. Маршруты строятся на закрытых generic-командах/запросах — конкретный
/// тип сущности здесь не нужен (диспетчер резолвит handler по типу команды). Любой маршрут можно
/// переопределить (методы виртуальные).
/// </summary>
public abstract class NoteEndpointsBase<TCreateRequest, TUpdateRequest, TDto>
    where TCreateRequest : CreateNoteRequestBase
    where TUpdateRequest : UpdateNoteRequestBase
    where TDto : NoteDtoBase
{
    protected virtual string RoutePrefix => NotesTimelineConstants.DefaultNotesRoutePrefix;

    public void Map(IEndpointRouteBuilder routes)
    {
        var prefix = RoutePrefix.TrimEnd('/');

        routes.MapPost(prefix, CreateAsync).WithName("CreateNote").WithTags("Notes");
        routes.MapGet(prefix, ListByEntityAsync).WithName("ListNotesByEntity").WithTags("Notes");
        routes.MapPut($"{prefix}/{{id:guid}}", UpdateAsync).WithName("UpdateNote").WithTags("Notes");
        routes.MapPost($"{prefix}/{{id:guid}}/pin", PinAsync).WithName("PinNote").WithTags("Notes");
        routes.MapPost($"{prefix}/{{id:guid}}/unpin", UnpinAsync).WithName("UnpinNote").WithTags("Notes");
        routes.MapDelete($"{prefix}/{{id:guid}}", RemoveAsync).WithName("RemoveNote").WithTags("Notes");
    }

    protected virtual async Task<IResult> CreateAsync(
        [FromBody] TCreateRequest request,
        [FromServices] IDispatcher dispatcher,
        CancellationToken ct)
    {
        try
        {
            var id = await dispatcher.SendAsync<CreateNoteCommand<TCreateRequest>, Guid>(
                new CreateNoteCommand<TCreateRequest>(request), ct);
            return Results.Created($"/{RoutePrefix.TrimEnd('/')}/{id}", id);
        }
        catch (NoteValidationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    protected virtual async Task<IResult> ListByEntityAsync(
        [FromQuery] string entityType,
        [FromQuery] Guid entityId,
        [FromServices] IDispatcher dispatcher,
        CancellationToken ct)
    {
        var items = await dispatcher.QueryAsync<GetNotesByEntityQuery<TDto>, IReadOnlyList<TDto>>(
            new GetNotesByEntityQuery<TDto>(entityType, entityId), ct);
        return Results.Ok(items);
    }

    protected virtual async Task<IResult> UpdateAsync(
        [FromRoute] Guid id,
        [FromBody] TUpdateRequest request,
        [FromServices] IDispatcher dispatcher,
        CancellationToken ct)
    {
        try
        {
            await dispatcher.SendAsync(new UpdateNoteCommand<TUpdateRequest>(id, request), ct);
            return Results.NoContent();
        }
        catch (NoteValidationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    protected virtual Task<IResult> PinAsync(
        [FromRoute] Guid id, [FromServices] IDispatcher dispatcher, CancellationToken ct)
        => SendLifecycleAsync(dispatcher, new PinNoteCommand(id), ct);

    protected virtual Task<IResult> UnpinAsync(
        [FromRoute] Guid id, [FromServices] IDispatcher dispatcher, CancellationToken ct)
        => SendLifecycleAsync(dispatcher, new UnpinNoteCommand(id), ct);

    protected virtual Task<IResult> RemoveAsync(
        [FromRoute] Guid id, [FromServices] IDispatcher dispatcher, CancellationToken ct)
        => SendLifecycleAsync(dispatcher, new RemoveNoteCommand(id), ct);

    private static async Task<IResult> SendLifecycleAsync<TCommand>(
        IDispatcher dispatcher, TCommand command, CancellationToken ct)
        where TCommand : ICommand
    {
        try
        {
            await dispatcher.SendAsync(command, ct);
            return Results.NoContent();
        }
        catch (NoteValidationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
}
