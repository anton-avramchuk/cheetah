using Cheetah.AspNetCore;
using Cheetah.AspNetCore.Extensions;
using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.Modularity;
using Cheetah.Permissions;
using Cheetah.Permissions.Catalog.Application;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Cheetah.Permissions.Catalog.Api;

[DependsOn(typeof(CoreModule),
    typeof(CrmCQRSCoreModule),
    typeof(CrmAspNetCoreModule),
    typeof(CrmPermissionsModule),
    typeof(CrmPermissionsCatalogModule))]
public partial class CrmPermissionsCatalogApiModule : CrmModule
{
    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var routeBuilder = context.GetRouteBuilder();

        routeBuilder.MapPost("api/permissions/catalog/sync", SyncAsync)
            .RequirePermission(CatalogPermissions.Sync)
            .WithName("PermissionsCatalogSync")
            .WithTags("Permissions");

        routeBuilder.MapGet("api/permissions/catalog", ListAsync)
            .RequirePermission(CatalogPermissions.Read)
            .WithName("PermissionsCatalogList")
            .WithTags("Permissions");
    }

    private static async Task<IResult> SyncAsync(
        [FromBody] RegistrySyncRequest request,
        [FromServices] IDispatcher dispatcher,
        CancellationToken ct)
    {
        await dispatcher.SendAsync(new SyncRegistryCommand(request.Module, request.Items), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> ListAsync(
        [FromQuery] string? module,
        [FromServices] IDispatcher dispatcher,
        CancellationToken ct)
    {
        var items = await dispatcher.QueryAsync<ListPermissionsQuery, IReadOnlyList<PermissionDefinitionDto>>(
            new ListPermissionsQuery(module), ct);
        return Results.Ok(items);
    }
}
