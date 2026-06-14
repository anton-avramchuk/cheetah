using Cheetah.AspNetCore;
using Cheetah.AspNetCore.Extensions;
using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Tags.Application.Assignments;
using Cheetah.Modules.Tags.Application.Exceptions;
using Cheetah.Modules.Tags.Application.Registry;
using Cheetah.Modules.Tags.Application.Tags;
using Cheetah.Modules.Tags.Contracts.Assignments;
using Cheetah.Modules.Tags.Contracts.Registry;
using Cheetah.Modules.Tags.Contracts.Tags;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Cheetah.Modules.Tags.Api;

[DependsOn(typeof(CoreModule),
    typeof(CrmCQRSCoreModule),
    typeof(CrmAspNetCoreModule),
    typeof(Application.CheetahTagsApplicationModule),
    typeof(Contracts.CheetahTagsContractsModule))]
public partial class CheetahTagsApiModule : CrmModule
{
    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var routes = context.GetRouteBuilder();

        // --- Registry: сервисы регистрируют применимые типы при старте ---
        routes.MapPost("api/tags/registry/sync", SyncRegistryAsync)
            .WithName("TagsRegistrySync").WithTags("Tags");
        routes.MapGet("api/tags/registry", ListTaggableTypesAsync)
            .WithName("TagsRegistryList").WithTags("Tags");

        // --- Tags: словарь тэгов ---
        routes.MapPost("api/tags", CreateTagAsync)
            .WithName("CreateTag").WithTags("Tags");
        routes.MapGet("api/tags", ListTagsAsync)
            .WithName("ListTags").WithTags("Tags");

        // --- Assignments: привязки тэг ↔ сущность ---
        routes.MapPost("api/tags/assignments", AssignTagsAsync)
            .WithName("AssignTags").WithTags("Tags");
        routes.MapDelete("api/tags/assignments", UnassignTagsAsync)
            .WithName("UnassignTags").WithTags("Tags");
        routes.MapGet("api/tags/assignments", GetEntityTagsAsync)
            .WithName("GetEntityTags").WithTags("Tags");
    }

    private static async Task<IResult> SyncRegistryAsync(
        [FromBody] RegistrySyncRequest request,
        [FromServices] IDispatcher dispatcher,
        CancellationToken ct)
    {
        await dispatcher.SendAsync(new SyncTaggableTypesCommand(request.OwnerService, request.Items), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> ListTaggableTypesAsync(
        [FromQuery] string? ownerService,
        [FromServices] IDispatcher dispatcher,
        CancellationToken ct)
    {
        var items = await dispatcher.QueryAsync<ListTaggableTypesQuery, IReadOnlyList<TaggableEntityTypeDto>>(
            new ListTaggableTypesQuery(ownerService), ct);
        return Results.Ok(items);
    }

    private static async Task<IResult> CreateTagAsync(
        [FromBody] CreateTagRequest request,
        [FromServices] IDispatcher dispatcher,
        CancellationToken ct)
    {
        try
        {
            var id = await dispatcher.SendAsync<CreateTagCommand, Guid>(
                new CreateTagCommand(request.Name, request.Color, request.Description, request.Group), ct);
            return Results.Created($"/api/tags/{id}", id);
        }
        catch (TagsValidationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ListTagsAsync(
        [FromQuery] string? group,
        [FromServices] IDispatcher dispatcher,
        CancellationToken ct)
    {
        var items = await dispatcher.QueryAsync<ListTagsQuery, IReadOnlyList<TagDto>>(
            new ListTagsQuery(group), ct);
        return Results.Ok(items);
    }

    private static async Task<IResult> AssignTagsAsync(
        [FromBody] AssignTagsRequest request,
        [FromServices] IDispatcher dispatcher,
        CancellationToken ct)
    {
        try
        {
            await dispatcher.SendAsync(new AssignTagsCommand(
                request.EntityType, request.EntityId, request.TagIds), ct);
            return Results.NoContent();
        }
        catch (TagsValidationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> UnassignTagsAsync(
        [FromBody] UnassignTagsRequest request,
        [FromServices] IDispatcher dispatcher,
        CancellationToken ct)
    {
        await dispatcher.SendAsync(new UnassignTagsCommand(
            request.EntityType, request.EntityId, request.TagIds), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> GetEntityTagsAsync(
        [FromQuery] string entityType,
        [FromQuery] Guid entityId,
        [FromServices] IDispatcher dispatcher,
        CancellationToken ct)
    {
        var items = await dispatcher.QueryAsync<GetEntityTagsQuery, IReadOnlyList<TagDto>>(
            new GetEntityTagsQuery(entityType, entityId), ct);
        return Results.Ok(items);
    }
}
