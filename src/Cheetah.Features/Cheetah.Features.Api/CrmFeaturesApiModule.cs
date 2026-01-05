using Cheetah.AspNetCore;
using Cheetah.AspNetCore.Extensions;
using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.Modularity;
using Cheetah.Features.Application;
using Cheetah.Features.Application.Commands;
using Cheetah.Features.Application.Queries;
using Cheetah.Features.DataAccess;
using Cheetah.Features.Domain.Entities;
using Cheetah.Features.Shared.Requests;
using Cheetah.Features.Shared.ViewModels;
using Cheetah.Mapping.Core;
using Cheetah.Mapping.Mapster;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Features.Api;

[DependsOn(typeof(CrmFeaturesApplicationModule))]
[DependsOn(typeof(CrmFeaturesDataAccessModule))]
[DependsOn(typeof(CrmAspNetCoreModule))]
[DependsOn(typeof(CrmMapsterModule))]
public partial class CrmFeaturesApiModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var routeBuilder = context.GetRouteBuilder();
        var mapper = context.ServiceProvider.GetRequiredService<IObjectMapper>();

        // GET /api/features - Get all features
        routeBuilder.MapGet("/api/features", async (
            [FromServices] IDispatcher dispatcher,
            CancellationToken cancellationToken) =>
        {
            var features = await dispatcher.QueryAsync<GetAllFeaturesQuery, IReadOnlyList<Feature>>(
                new GetAllFeaturesQuery(),
                cancellationToken);

            var viewModels = mapper.Map<IReadOnlyList<FeatureViewModel>>(features);
            return Results.Ok(viewModels);
        })
        .WithName("GetAllFeatures")
        .Produces<IReadOnlyList<FeatureViewModel>>(StatusCodes.Status200OK);

        // POST /api/features - Create feature
        routeBuilder.MapPost("/api/features", async (
            [FromBody] CreateFeatureRequest request,
            [FromServices] IDispatcher dispatcher,
            CancellationToken cancellationToken) =>
        {
            var command = mapper.Map<CreateFeatureCommand>(request);
            var featureId = await dispatcher.SendAsync<CreateFeatureCommand, string>(command, cancellationToken);

            return Results.Created($"/api/features/{featureId}", featureId);
        })
        .WithName("CreateFeature")
        .Produces<string>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        // GET /api/tenants/{tenantId}/features - Get tenant features
        routeBuilder.MapGet("/api/tenants/{tenantId:guid}/features", async (
            [FromRoute] Guid tenantId,
            [FromServices] IDispatcher dispatcher,
            CancellationToken cancellationToken) =>
        {
            var tenantFeatures = await dispatcher.QueryAsync<GetTenantFeaturesQuery, IReadOnlyList<TenantFeature>>(
                new GetTenantFeaturesQuery(tenantId),
                cancellationToken);

            var viewModels = mapper.Map<IReadOnlyList<TenantFeatureViewModel>>(tenantFeatures);
            return Results.Ok(viewModels);
        })
        .WithName("GetTenantFeatures")
        .Produces<IReadOnlyList<TenantFeatureViewModel>>(StatusCodes.Status200OK);

        // POST /api/tenants/{tenantId}/features/{featureId}/enable - Enable feature for tenant
        routeBuilder.MapPost("/api/tenants/{tenantId:guid}/features/{featureId}/enable", async (
            [FromRoute] Guid tenantId,
            [FromRoute] string featureId,
            [FromServices] IDispatcher dispatcher,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var command = new EnableFeatureCommand(tenantId, featureId);
                await dispatcher.SendAsync(command, cancellationToken);
                return Results.NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        })
        .WithName("EnableFeature")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest);

        // POST /api/tenants/{tenantId}/features/{featureId}/disable - Disable feature for tenant
        routeBuilder.MapPost("/api/tenants/{tenantId:guid}/features/{featureId}/disable", async (
            [FromRoute] Guid tenantId,
            [FromRoute] string featureId,
            [FromServices] IDispatcher dispatcher,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var command = new DisableFeatureCommand(tenantId, featureId);
                await dispatcher.SendAsync(command, cancellationToken);
                return Results.NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        })
        .WithName("DisableFeature")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest);

        // GET /api/tenants/{tenantId}/features/{featureId}/check - Check if feature is enabled
        routeBuilder.MapGet("/api/tenants/{tenantId:guid}/features/{featureId}/check", async (
            [FromRoute] Guid tenantId,
            [FromRoute] string featureId,
            [FromServices] IDispatcher dispatcher,
            CancellationToken cancellationToken) =>
        {
            var isEnabled = await dispatcher.QueryAsync<CheckFeatureQuery, bool>(
                new CheckFeatureQuery(tenantId, featureId),
                cancellationToken);

            return Results.Ok(new { isEnabled });
        })
        .WithName("CheckFeature")
        .Produces<object>(StatusCodes.Status200OK);
    }
}
