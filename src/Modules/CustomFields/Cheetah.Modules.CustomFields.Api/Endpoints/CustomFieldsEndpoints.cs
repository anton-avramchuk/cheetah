using Cheetah.Core.CQRS;
using Cheetah.Modules.CustomFields.Application.Definitions;
using Cheetah.Modules.CustomFields.Application.Registry;
using Cheetah.Modules.CustomFields.Application.Values;
using Cheetah.Modules.CustomFields.Contracts;
using Cheetah.Modules.CustomFields.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Cheetah.Modules.CustomFields.Api.Endpoints;

/// <summary>
/// Эндпоинты CustomFields: реестр расширяемых типов, админка определений, значения и видимые поля.
/// Минимал-API, маршруты на CQRS-командах/запросах через <see cref="IDispatcher"/>.
/// </summary>
public sealed class CustomFieldsEndpoints
{
    private const string Tag = "CustomFields";
    private static string P => CustomFieldsConstants.DefaultRoutePrefix;

    public void Map(IEndpointRouteBuilder routes)
    {
        // Реестр (для сервисов).
        routes.MapPost($"{P}/registry/sync", SyncRegistryAsync).WithName("SyncCustomFieldRegistry").WithTags(Tag);
        routes.MapGet($"{P}/registry", ListRegistryAsync).WithName("ListCustomFieldRegistry").WithTags(Tag);

        // Определения (админка).
        routes.MapGet($"{P}/definitions", ListDefinitionsAsync).WithName("ListCustomFieldDefinitions").WithTags(Tag);
        routes.MapPost($"{P}/definitions", CreateDefinitionAsync).WithName("CreateCustomFieldDefinition").WithTags(Tag);
        routes.MapPut($"{P}/definitions/{{id:guid}}", UpdateDefinitionAsync).WithName("UpdateCustomFieldDefinition").WithTags(Tag);
        routes.MapDelete($"{P}/definitions/{{id:guid}}", DeactivateDefinitionAsync).WithName("DeactivateCustomFieldDefinition").WithTags(Tag);

        // Значения.
        routes.MapGet($"{P}/values", GetValuesAsync).WithName("GetCustomFieldValues").WithTags(Tag);
        routes.MapPut($"{P}/values", SetValuesAsync).WithName("SetCustomFieldValues").WithTags(Tag);
        routes.MapPost($"{P}/values/batch-get", BatchGetValuesAsync).WithName("BatchGetCustomFieldValues").WithTags(Tag);

        // Видимые поля (форма с учётом JsonLogic-видимости).
        routes.MapPost($"{P}/fields/visible", GetVisibleFieldsAsync).WithName("GetVisibleCustomFields").WithTags(Tag);
    }

    private static async Task<IResult> SyncRegistryAsync(
        [FromBody] SyncRegistryBody body, [FromServices] IDispatcher dispatcher, CancellationToken ct)
    {
        await dispatcher.SendAsync(new SyncEntityTypeRegistryCommand(body.Descriptors), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> ListRegistryAsync([FromServices] IDispatcher dispatcher, CancellationToken ct)
    {
        var items = await dispatcher.QueryAsync<ListEntityTypesQuery, IReadOnlyList<CustomFieldEntityTypeDto>>(
            new ListEntityTypesQuery(), ct);
        return Results.Ok(items);
    }

    private static async Task<IResult> ListDefinitionsAsync(
        [FromQuery] string entityType, [FromQuery] bool? onlyActive,
        [FromServices] IDispatcher dispatcher, CancellationToken ct)
    {
        var items = await dispatcher.QueryAsync<ListDefinitionsQuery, IReadOnlyList<CustomFieldDefinitionDto>>(
            new ListDefinitionsQuery(entityType, onlyActive ?? true), ct);
        return Results.Ok(items);
    }

    private static async Task<IResult> CreateDefinitionAsync(
        [FromBody] CreateCustomFieldDefinitionRequest request, [FromServices] IDispatcher dispatcher, CancellationToken ct)
    {
        var id = await dispatcher.SendAsync<CreateCustomFieldDefinitionCommand, Guid>(
            new CreateCustomFieldDefinitionCommand(request), ct);
        return Results.Created($"{P}/definitions/{id}", id);
    }

    private static async Task<IResult> UpdateDefinitionAsync(
        [FromRoute] Guid id, [FromBody] UpdateCustomFieldDefinitionRequest request,
        [FromServices] IDispatcher dispatcher, CancellationToken ct)
    {
        await dispatcher.SendAsync(new UpdateCustomFieldDefinitionCommand(request with { Id = id }), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> DeactivateDefinitionAsync(
        [FromRoute] Guid id, [FromServices] IDispatcher dispatcher, CancellationToken ct)
    {
        await dispatcher.SendAsync(new DeactivateCustomFieldDefinitionCommand(id), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> GetValuesAsync(
        [FromQuery] string entityType, [FromQuery] string entityId,
        [FromServices] IDispatcher dispatcher, CancellationToken ct)
    {
        var dto = await dispatcher.QueryAsync<GetValuesQuery, CustomFieldValuesDto>(
            new GetValuesQuery(entityType, entityId), ct);
        return Results.Ok(dto);
    }

    private static async Task<IResult> SetValuesAsync(
        [FromBody] SetCustomFieldValuesRequest request, [FromServices] IDispatcher dispatcher, CancellationToken ct)
    {
        await dispatcher.SendAsync(new SetCustomFieldValuesCommand(request), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> BatchGetValuesAsync(
        [FromBody] BatchGetValuesRequest request, [FromServices] IDispatcher dispatcher, CancellationToken ct)
    {
        var result = await dispatcher
            .QueryAsync<BatchGetValuesQuery, IReadOnlyDictionary<string, IReadOnlyDictionary<string, object?>>>(
                new BatchGetValuesQuery(request.EntityType, request.EntityIds), ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetVisibleFieldsAsync(
        [FromBody] GetVisibleFieldsRequest request, [FromServices] IDispatcher dispatcher, CancellationToken ct)
    {
        var items = await dispatcher.QueryAsync<GetVisibleFieldsQuery, IReadOnlyList<CustomFieldDefinitionDto>>(
            new GetVisibleFieldsQuery(request.EntityType, request.EntityId, request.Context), ct);
        return Results.Ok(items);
    }
}

/// <summary>Тело registry/sync.</summary>
public sealed record SyncRegistryBody(IReadOnlyList<CustomFieldEntityTypeDescriptor> Descriptors);
