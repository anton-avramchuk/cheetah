using Cheetah.AspNetCore;
using Cheetah.AspNetCore.Extensions;
using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.Modularity;
using Cheetah.Identity.Application;
using Cheetah.Identity.Application.Commands;
using Cheetah.Identity.Application.Queries;
using Cheetah.Identity.Contracts;
using Cheetah.Identity.Contracts.Requests;
using Cheetah.Identity.Contracts.ViewModels;
using Cheetah.Mapping.Core;
using Cheetah.Mapping.Mapster;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Identity.Api;

[DependsOn(typeof(CrmIdentityApplicationModule))]
[DependsOn(typeof(CrmIdentityContractsModule))]
[DependsOn(typeof(CrmAspNetCoreModule))]
[DependsOn(typeof(CrmMapsterModule))]
public partial class CrmIdentityApiModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var routeBuilder = context.GetRouteBuilder();
        var mapper = context.ServiceProvider.GetRequiredService<IObjectMapper>();

        // Authentication Endpoints

        // POST /api/identity/register
        routeBuilder.MapPost("/api/identity/register", async (
            [FromBody] RegisterUserRequest request,
            [FromServices] IDispatcher dispatcher,
            CancellationToken ct) =>
        {
            var command = mapper.Map<RegisterUserCommand>(request);
            var userId = await dispatcher.SendAsync<RegisterUserCommand, Guid>(command, ct);

            var query = new GetUserByIdQuery(userId);
            var userViewModel = await dispatcher.QueryAsync<GetUserByIdQuery, UserViewModel?>(query, ct);

            return Results.Created($"/api/identity/users/{userId}", userViewModel);
        })
        .WithName("RegisterUser")
        .AllowAnonymous();

        // POST /api/identity/users/{userId}/change-password
        routeBuilder.MapPost("/api/identity/users/{userId:guid}/change-password", async (
            [FromRoute] Guid userId,
            [FromBody] ChangePasswordRequest request,
            [FromServices] IDispatcher dispatcher,
            CancellationToken ct) =>
        {
            var command = new ChangePasswordCommand(
                userId,
                request.CurrentPassword,
                request.NewPassword
            );
            await dispatcher.SendAsync(command, ct);

            return Results.NoContent();
        })
        .WithName("ChangePassword");

        // POST /api/identity/confirm-email
        routeBuilder.MapPost("/api/identity/confirm-email/{userId:guid}", async (
            [FromRoute] Guid userId,
            [FromServices] IDispatcher dispatcher,
            CancellationToken ct) =>
        {
            var command = new ConfirmEmailCommand(userId);
            await dispatcher.SendAsync(command, ct);

            return Results.NoContent();
        })
        .WithName("ConfirmEmail");

        // User Endpoints

        // GET /api/identity/users/{id}
        routeBuilder.MapGet("/api/identity/users/{id:guid}", async (
            [FromRoute] Guid id,
            [FromServices] IDispatcher dispatcher,
            CancellationToken ct) =>
        {
            var query = new GetUserByIdQuery(id);
            var userViewModel = await dispatcher.QueryAsync<GetUserByIdQuery, UserViewModel?>(query, ct);

            return userViewModel is not null
                ? Results.Ok(userViewModel)
                : Results.NotFound();
        })
        .WithName("GetUserById");

        // GET /api/identity/users/email/{email}
        routeBuilder.MapGet("/api/identity/users/email/{email}", async (
            [FromRoute] string email,
            [FromServices] IDispatcher dispatcher,
            CancellationToken ct) =>
        {
            var query = new GetUserByEmailQuery(email);
            var userViewModel = await dispatcher.QueryAsync<GetUserByEmailQuery, UserViewModel?>(query, ct);

            return userViewModel is not null
                ? Results.Ok(userViewModel)
                : Results.NotFound();
        })
        .WithName("GetUserByEmail");

        // GET /api/identity/users/{userId}/permissions
        routeBuilder.MapGet("/api/identity/users/{userId:guid}/permissions", async (
            [FromRoute] Guid userId,
            [FromServices] IDispatcher dispatcher,
            CancellationToken ct) =>
        {
            var query = new GetUserPermissionsQuery(userId);
            var permissions = await dispatcher.QueryAsync<GetUserPermissionsQuery, List<string>>(query, ct);

            return Results.Ok(permissions);
        })
        .WithName("GetUserPermissions");

        // Role Endpoints

        // POST /api/identity/roles
        routeBuilder.MapPost("/api/identity/roles", async (
            [FromBody] CreateRoleRequest request,
            [FromServices] IDispatcher dispatcher,
            CancellationToken ct) =>
        {
            var command = mapper.Map<CreateRoleCommand>(request);
            var roleId = await dispatcher.SendAsync<CreateRoleCommand, Guid>(command, ct);

            return Results.Created($"/api/identity/roles/{roleId}", new { id = roleId });
        })
        .WithName("CreateRole");

        // GET /api/identity/roles
        routeBuilder.MapGet("/api/identity/roles", async (
            [FromServices] IDispatcher dispatcher,
            CancellationToken ct) =>
        {
            var query = new GetAllRolesQuery();
            var roles = await dispatcher.QueryAsync<GetAllRolesQuery, List<RoleViewModel>>(query, ct);

            return Results.Ok(roles);
        })
        .WithName("GetAllRoles");

        // Permission Endpoints

        // POST /api/identity/roles/{roleId}/permissions
        routeBuilder.MapPost("/api/identity/roles/{roleId:guid}/permissions", async (
            [FromRoute] Guid roleId,
            [FromBody] AddPermissionRequest request,
            [FromServices] IDispatcher dispatcher,
            CancellationToken ct) =>
        {
            var command = new AddPermissionToRoleCommand(roleId, request.Permission);
            await dispatcher.SendAsync(command, ct);

            return Results.NoContent();
        })
        .WithName("AddPermissionToRole");

        // POST /api/identity/users/{userId}/permissions
        routeBuilder.MapPost("/api/identity/users/{userId:guid}/permissions", async (
            [FromRoute] Guid userId,
            [FromBody] AddPermissionRequest request,
            [FromServices] IDispatcher dispatcher,
            CancellationToken ct) =>
        {
            var command = new AddPermissionToUserCommand(userId, request.Permission);
            await dispatcher.SendAsync(command, ct);

            return Results.NoContent();
        })
        .WithName("AddPermissionToUser");

        // POST /api/identity/users/{userId}/roles/{roleId}
        routeBuilder.MapPost("/api/identity/users/{userId:guid}/roles/{roleId:guid}", async (
            [FromRoute] Guid userId,
            [FromRoute] Guid roleId,
            [FromServices] IDispatcher dispatcher,
            CancellationToken ct) =>
        {
            var command = new AssignRoleToUserCommand(userId, roleId);
            await dispatcher.SendAsync(command, ct);

            return Results.NoContent();
        })
        .WithName("AssignRoleToUser");
    }
}
