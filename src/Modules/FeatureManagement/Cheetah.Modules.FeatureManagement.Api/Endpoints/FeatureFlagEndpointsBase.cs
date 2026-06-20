using Cheetah.Core.CQRS;
using Cheetah.Core.Domain.Exceptions;
using Cheetah.FeatureManagement;
using Cheetah.Modules.FeatureManagement.Application.Commands;
using Cheetah.Modules.FeatureManagement.Application.Queries;
using Cheetah.Modules.FeatureManagement.Contracts;
using Cheetah.Modules.FeatureManagement.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Cheetah.Modules.FeatureManagement.Api.Endpoints;

/// <summary>
/// Абстрактная база эндпоинтов FeatureManagement: реестр (registry/sync), админка флагов и батч-оценка.
/// Generic над конкретными Contracts наследника; маршруты — на закрытых generic-командах/запросах.
/// Любой метод можно переопределить (виртуальные).
/// </summary>
public abstract class FeatureFlagEndpointsBase<TCreateRequest, TDto>
    where TCreateRequest : CreateFeatureFlagRequestBase
    where TDto : FeatureFlagDtoBase
{
    protected virtual string RoutePrefix => FeatureManagementConstants.DefaultRoutePrefix;
    private const string Tag = "FeatureManagement";

    public void Map(IEndpointRouteBuilder routes)
    {
        var p = RoutePrefix.TrimEnd('/');

        // Реестр (для сервисов).
        routes.MapPost($"{p}/registry/sync", SyncRegistryAsync).WithName("SyncFeatureRegistry").WithTags(Tag);
        routes.MapGet($"{p}/registry", ListRegistryAsync).WithName("ListFeatureRegistry").WithTags(Tag);

        // Админка.
        routes.MapPost(p, CreateAsync).WithName("CreateFeatureFlag").WithTags(Tag);
        routes.MapGet(p, ListAsync).WithName("ListFeatureFlags").WithTags(Tag);
        routes.MapGet($"{p}/{{key}}", GetByKeyAsync).WithName("GetFeatureFlag").WithTags(Tag);
        routes.MapPost($"{p}/{{key}}/enable", EnableAsync).WithName("EnableFeatureFlag").WithTags(Tag);
        routes.MapPost($"{p}/{{key}}/disable", DisableAsync).WithName("DisableFeatureFlag").WithTags(Tag);
        routes.MapPut($"{p}/{{key}}/targeting", SetTargetingAsync).WithName("SetFeatureTargeting").WithTags(Tag);
        routes.MapPut($"{p}/{{key}}/tenants/{{tenantId:guid}}", SetTenantOverrideAsync).WithName("SetFeatureTenantOverride").WithTags(Tag);

        // Оценка (для потребителей без локальной реплики).
        routes.MapPost($"{p}/evaluate", EvaluateAsync).WithName("EvaluateFeatures").WithTags(Tag);

        // Снимок определений (pull для RemoteFeatureDefinitionProvider в микросервисе-потребителе).
        routes.MapGet($"{p}/definitions", PullDefinitionsAsync).WithName("PullFeatureDefinitions").WithTags(Tag);
    }

    protected virtual async Task<IResult> PullDefinitionsAsync(
        [FromQuery] Guid? tenantId, [FromServices] IFeatureDefinitionProvider provider, CancellationToken ct)
    {
        var defs = await provider.GetAllAsync(tenantId, ct);
        return Results.Ok(defs);
    }

    protected virtual async Task<IResult> SyncRegistryAsync(
        [FromBody] SyncRegistryBody body, [FromServices] IDispatcher dispatcher, CancellationToken ct)
    {
        await dispatcher.SendAsync(new SyncFeatureRegistryCommand(body.Descriptors), ct);
        return Results.NoContent();
    }

    protected virtual async Task<IResult> ListRegistryAsync(
        [FromQuery] string? ownerService, [FromServices] IDispatcher dispatcher, CancellationToken ct)
    {
        var items = await dispatcher.QueryAsync<ListFeatureFlagsQuery<TDto>, IReadOnlyList<TDto>>(
            new ListFeatureFlagsQuery<TDto>(ownerService, OnlyActive: null), ct);
        return Results.Ok(items);
    }

    protected virtual async Task<IResult> CreateAsync(
        [FromBody] TCreateRequest request, [FromServices] IDispatcher dispatcher, CancellationToken ct)
    {
        var id = await dispatcher.SendAsync<CreateFeatureFlagCommand<TCreateRequest>, Guid>(
            new CreateFeatureFlagCommand<TCreateRequest>(request), ct);
        return Results.Created($"/{RoutePrefix.TrimEnd('/')}/{request.Key}", id);
    }

    protected virtual async Task<IResult> ListAsync(
        [FromQuery] string? ownerService, [FromQuery] bool? onlyActive,
        [FromServices] IDispatcher dispatcher, CancellationToken ct)
    {
        var items = await dispatcher.QueryAsync<ListFeatureFlagsQuery<TDto>, IReadOnlyList<TDto>>(
            new ListFeatureFlagsQuery<TDto>(ownerService, onlyActive), ct);
        return Results.Ok(items);
    }

    protected virtual async Task<IResult> GetByKeyAsync(
        [FromRoute] string key, [FromServices] IDispatcher dispatcher, CancellationToken ct)
    {
        var dto = await dispatcher.QueryAsync<GetFeatureFlagByKeyQuery<TDto>, TDto?>(
            new GetFeatureFlagByKeyQuery<TDto>(key), ct);
        return dto is null ? Results.NotFound() : Results.Ok(dto);
    }

    protected virtual Task<IResult> EnableAsync(
        [FromRoute] string key, [FromServices] IDispatcher dispatcher, CancellationToken ct)
        => SendAsync(dispatcher, new EnableFeatureFlagCommand(key), ct);

    protected virtual Task<IResult> DisableAsync(
        [FromRoute] string key, [FromServices] IDispatcher dispatcher, CancellationToken ct)
        => SendAsync(dispatcher, new DisableFeatureFlagCommand(key), ct);

    protected virtual Task<IResult> SetTargetingAsync(
        [FromRoute] string key, [FromBody] SetTargetingBody body,
        [FromServices] IDispatcher dispatcher, CancellationToken ct)
        => SendAsync(dispatcher, new SetTargetingCommand(key, body.Rules), ct);

    protected virtual Task<IResult> SetTenantOverrideAsync(
        [FromRoute] string key, [FromRoute] Guid tenantId, [FromBody] SetTenantOverrideBody body,
        [FromServices] IDispatcher dispatcher, CancellationToken ct)
        => SendAsync(dispatcher, new SetTenantOverrideCommand(key, tenantId, body.Enabled, body.Rules), ct);

    protected virtual async Task<IResult> EvaluateAsync(
        [FromBody] EvaluateFeaturesRequest request, [FromServices] IDispatcher dispatcher, CancellationToken ct)
    {
        var result = await dispatcher.QueryAsync<EvaluateFeaturesQuery, IReadOnlyDictionary<string, FeatureEvaluationDto>>(
            new EvaluateFeaturesQuery(request.Keys, request.Context ?? FeatureContext.Empty), ct);
        return Results.Ok(result);
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

/// <summary>Тело registry/sync.</summary>
public sealed record SyncRegistryBody(IReadOnlyList<FeatureDefinitionDescriptor> Descriptors);

/// <summary>Тело смены таргетинга.</summary>
public sealed record SetTargetingBody(IReadOnlyList<TargetingRuleDto> Rules);

/// <summary>Тело override тенанта.</summary>
public sealed record SetTenantOverrideBody(bool Enabled, IReadOnlyList<TargetingRuleDto> Rules);

/// <summary>Тело батч-оценки.</summary>
public sealed record EvaluateFeaturesRequest(IReadOnlyList<string> Keys, FeatureContext? Context);
