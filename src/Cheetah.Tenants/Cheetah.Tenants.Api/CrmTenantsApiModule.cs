using Cheetah.AspNetCore;
using Cheetah.AspNetCore.Extensions;
using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.Modularity;
using Cheetah.Mapping.Core;
using Cheetah.Mapping.Mapster;
using Cheetah.Tenants.Application;
using Cheetah.Tenants.Application.Commands;
using Cheetah.Tenants.Application.Queries;
using Cheetah.Tenants.Domain.Entities;
using Cheetah.Tenants.Shared.Requests;
using Cheetah.Tenants.Shared.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Cheetah.Tenants.Api;

[DependsOn(typeof(CrmTenantsApplicationModule))]
[DependsOn(typeof(CrmAspNetCoreModule))]
[DependsOn(typeof(CrmMapsterModule))]
public partial class CrmTenantsApiModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var routeBuilder = context.GetRouteBuilder();
        var mapper = context.ServiceProvider.GetRequiredService<IObjectMapper>();

        // GET /api/tenants - Get all tenants
        routeBuilder.MapGet("/api/tenants", async (
            [FromServices] IDispatcher dispatcher,
            CancellationToken cancellationToken) =>
        {
            var tenants = await dispatcher.QueryAsync<GetAllTenantsQuery, IReadOnlyList<Tenant>>(
                new GetAllTenantsQuery(),
                cancellationToken);

            var viewModels = mapper.Map<IReadOnlyList<TenantViewModel>>(tenants);
            return Results.Ok(viewModels);
        })
        .WithName("GetAllTenants")
        .Produces<IReadOnlyList<TenantViewModel>>(StatusCodes.Status200OK);

        // GET /api/tenants/{id} - Get tenant by ID
        routeBuilder.MapGet("/api/tenants/{id:guid}", async (
            [FromRoute] Guid id,
            [FromServices] IDispatcher dispatcher,
            CancellationToken cancellationToken) =>
        {
            var tenant = await dispatcher.QueryAsync<GetTenantByIdQuery, Tenant?>(
                new GetTenantByIdQuery(id),
                cancellationToken);

            if (tenant == null)
                return Results.NotFound();

            var viewModel = mapper.Map<TenantViewModel>(tenant);
            return Results.Ok(viewModel);
        })
        .WithName("GetTenantById")
        .Produces<TenantViewModel>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // POST /api/tenants - Create tenant
        routeBuilder.MapPost("/api/tenants", async (
            [FromBody] CreateTenantRequest request,
            [FromServices] IDispatcher dispatcher,
            CancellationToken cancellationToken) =>
        {
            var command = mapper.Map<CreateTenantCommand>(request);
            var tenantId = await dispatcher.SendAsync<CreateTenantCommand, Guid>(command, cancellationToken);

            return Results.CreatedAtRoute("GetTenantById", new { id = tenantId }, tenantId);
        })
        .WithName("CreateTenant")
        .Produces<Guid>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        // POST /api/tenants/{id}/activate - Activate tenant
        routeBuilder.MapPost("/api/tenants/{id:guid}/activate", async (
            [FromRoute] Guid id,
            [FromServices] IDispatcher dispatcher,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var command = new ActivateTenantCommand(id);
                await dispatcher.SendAsync(command, cancellationToken);
                return Results.NoContent();
            }
            catch (InvalidOperationException)
            {
                return Results.NotFound();
            }
        })
        .WithName("ActivateTenant")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        // POST /api/tenants/{id}/deactivate - Deactivate tenant
        routeBuilder.MapPost("/api/tenants/{id:guid}/deactivate", async (
            [FromRoute] Guid id,
            [FromServices] IDispatcher dispatcher,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var command = new DeactivateTenantCommand(id);
                await dispatcher.SendAsync(command, cancellationToken);
                return Results.NoContent();
            }
            catch (InvalidOperationException)
            {
                return Results.NotFound();
            }
        })
        .WithName("DeactivateTenant")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}
