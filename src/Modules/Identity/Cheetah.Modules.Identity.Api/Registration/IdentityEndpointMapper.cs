using Cheetah.AspNetCore.Contracts.Extensions;
using Cheetah.Backend.Endpoints.Responses;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Mapping.Core;
using Cheetah.Modules.Identity.Application.Commands;
using Cheetah.Modules.Identity.Application.Models;
using Cheetah.Modules.Identity.Application.Queries;
using Cheetah.Modules.Identity.Contracts.Requests;
using Cheetah.Modules.Identity.Contracts.Response;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Cheetah.Modules.Identity.Api.Registration;

/// <summary>
/// Ручной обобщённый регистратор HTTP-эндпоинтов Identity. Заменяет source-generator
/// (тот пропускает абстрактные базовые эндпоинты): хост указывает конкретные типы один
/// раз в <c>AddCrmIdentity(...)</c>, а здесь они разворачиваются в Map* вызовы.
/// </summary>
internal static class IdentityEndpointMapper
{
    private const string UsersRoute = "api/users";
    private const string RolesRoute = "api/roles";

    public static void MapUserEndpoints<
        TCreateRequest, TCreateCommand,
        TUpdateRequest, TUpdateCommand,
        TUserModel, TUserDetailModel,
        TUserGridVm, TUserDetailVm>(IEndpointRouteBuilder routes)
        where TCreateRequest : CreateUserRequest
        where TCreateCommand : CreateUserCommand
        where TUpdateRequest : UpdateUserRequest
        where TUpdateCommand : UpdateUserCommand
        where TUserModel : UserModel
        where TUserDetailModel : UserDetailModel
        where TUserGridVm : class, ICrmResponse
        where TUserDetailVm : class, ICrmResponse
    {
        // Create -> 201
        routes.MapPost(UsersRoute, async (
                [FromBody] TCreateRequest bodyRequest,
                HttpContext httpContext,
                [FromServices] IDispatcher dispatcher,
                [FromServices] IObjectMapper mapper,
                CancellationToken ct) =>
            {
                var request = httpContext.MergeRouteValuesInto(bodyRequest);
                var command = mapper.Map<TCreateCommand>(request);
                var id = await dispatcher.SendAsync<TCreateCommand, Guid>(command, ct);
                var routeValues = new RouteValueDictionary(httpContext.Request.RouteValues) { ["id"] = id };
                return Results.CreatedAtRoute("GetUserById", routeValues, new GuidResponse(id));
            })
            .WithTags("Users")
            .Produces<GuidResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        // Update -> 204
        routes.MapPut($"{UsersRoute}/{{id:guid}}", async (
                [FromBody] TUpdateRequest bodyRequest,
                HttpContext httpContext,
                [FromServices] IDispatcher dispatcher,
                [FromServices] IObjectMapper mapper,
                CancellationToken ct) =>
            {
                var request = httpContext.MergeRouteValuesInto(bodyRequest);
                var command = mapper.Map<TUpdateCommand>(request);
                await dispatcher.SendAsync(command, ct);
                return Results.NoContent();
            })
            .WithTags("Users")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

        // Delete -> 204
        routes.MapDelete($"{UsersRoute}/{{id:guid}}", async (
                [AsParameters] DeleteUserRequest request,
                [FromServices] IDispatcher dispatcher,
                [FromServices] IObjectMapper mapper,
                CancellationToken ct) =>
            {
                var command = mapper.Map<DeleteUserCommand>(request);
                await dispatcher.SendAsync(command, ct);
                return Results.NoContent();
            })
            .WithTags("Users")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        // Get by id -> 200 / 404
        routes.MapGet($"{UsersRoute}/{{id:guid}}", async (
                [AsParameters] GetUserByIdRequest request,
                [FromServices] IDispatcher dispatcher,
                [FromServices] IObjectMapper mapper,
                CancellationToken ct) =>
            {
                var query = mapper.Map<GetUserByIdQuery<TUserDetailModel>>(request);
                var result = await dispatcher.QueryAsync<GetUserByIdQuery<TUserDetailModel>, TUserDetailModel?>(query, ct);
                if (result is null)
                    return Results.NotFound();
                var response = mapper.Map<TUserDetailVm>(result);
                return Results.Ok(response);
            })
            .WithName("GetUserById")
            .WithTags("Users")
            .Produces<TUserDetailVm>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        // Grid -> 200
        routes.MapGet(UsersRoute, async (
                HttpContext httpContext,
                [FromServices] IDispatcher dispatcher,
                [FromServices] IObjectMapper mapper,
                CancellationToken ct) =>
            {
                var request = httpContext.BindGridRequest<GetUsersGridRequest>();
                var query = mapper.Map<GetUsersGridQuery<TUserModel>>(request);
                var result = await dispatcher.QueryAsync<GetUsersGridQuery<TUserModel>, GridResult<TUserModel>>(query, ct);
                var items = mapper.Map<IReadOnlyList<TUserGridVm>>(result.Data);
                var response = new GridResult<TUserGridVm>(items, result.Total);
                return Results.Ok(response);
            })
            .WithTags("Users")
            .Produces<GridResult<TUserGridVm>>(StatusCodes.Status200OK);
    }

    public static void MapRoleEndpoints<
        TCreateRequest, TCreateCommand,
        TUpdateRequest, TUpdateCommand,
        TRoleModel,
        TRoleGridVm, TRoleVm>(IEndpointRouteBuilder routes)
        where TCreateRequest : CreateRoleRequest
        where TCreateCommand : CreateRoleCommand
        where TUpdateRequest : UpdateRoleRequest
        where TUpdateCommand : UpdateRoleCommand
        where TRoleModel : RoleModel
        where TRoleGridVm : class, ICrmResponse
        where TRoleVm : class, ICrmResponse
    {
        // Create -> 201
        routes.MapPost(RolesRoute, async (
                [FromBody] TCreateRequest bodyRequest,
                HttpContext httpContext,
                [FromServices] IDispatcher dispatcher,
                [FromServices] IObjectMapper mapper,
                CancellationToken ct) =>
            {
                var request = httpContext.MergeRouteValuesInto(bodyRequest);
                var command = mapper.Map<TCreateCommand>(request);
                var id = await dispatcher.SendAsync<TCreateCommand, Guid>(command, ct);
                var routeValues = new RouteValueDictionary(httpContext.Request.RouteValues) { ["id"] = id };
                return Results.CreatedAtRoute("GetRoleById", routeValues, new GuidResponse(id));
            })
            .WithTags("Roles")
            .Produces<GuidResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        // Update -> 204
        routes.MapPut($"{RolesRoute}/{{id:guid}}", async (
                [FromBody] TUpdateRequest bodyRequest,
                HttpContext httpContext,
                [FromServices] IDispatcher dispatcher,
                [FromServices] IObjectMapper mapper,
                CancellationToken ct) =>
            {
                var request = httpContext.MergeRouteValuesInto(bodyRequest);
                var command = mapper.Map<TUpdateCommand>(request);
                await dispatcher.SendAsync(command, ct);
                return Results.NoContent();
            })
            .WithTags("Roles")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

        // Delete -> 204
        routes.MapDelete($"{RolesRoute}/{{id:guid}}", async (
                [AsParameters] DeleteRoleRequest request,
                [FromServices] IDispatcher dispatcher,
                [FromServices] IObjectMapper mapper,
                CancellationToken ct) =>
            {
                var command = mapper.Map<DeleteRoleCommand>(request);
                await dispatcher.SendAsync(command, ct);
                return Results.NoContent();
            })
            .WithTags("Roles")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        // Get by id -> 200 / 404
        routes.MapGet($"{RolesRoute}/{{id:guid}}", async (
                [AsParameters] GetRoleByIdRequest request,
                [FromServices] IDispatcher dispatcher,
                [FromServices] IObjectMapper mapper,
                CancellationToken ct) =>
            {
                var query = mapper.Map<GetRoleByIdQuery<TRoleModel>>(request);
                var result = await dispatcher.QueryAsync<GetRoleByIdQuery<TRoleModel>, TRoleModel?>(query, ct);
                if (result is null)
                    return Results.NotFound();
                var response = mapper.Map<TRoleVm>(result);
                return Results.Ok(response);
            })
            .WithName("GetRoleById")
            .WithTags("Roles")
            .Produces<TRoleVm>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        // Grid -> 200
        routes.MapGet(RolesRoute, async (
                HttpContext httpContext,
                [FromServices] IDispatcher dispatcher,
                [FromServices] IObjectMapper mapper,
                CancellationToken ct) =>
            {
                var request = httpContext.BindGridRequest<GetRolesGridRequest>();
                var query = mapper.Map<GetRolesGridQuery<TRoleModel>>(request);
                var result = await dispatcher.QueryAsync<GetRolesGridQuery<TRoleModel>, GridResult<TRoleModel>>(query, ct);
                var items = mapper.Map<IReadOnlyList<TRoleGridVm>>(result.Data);
                var response = new GridResult<TRoleGridVm>(items, result.Total);
                return Results.Ok(response);
            })
            .WithTags("Roles")
            .Produces<GridResult<TRoleGridVm>>(StatusCodes.Status200OK);
    }
}
