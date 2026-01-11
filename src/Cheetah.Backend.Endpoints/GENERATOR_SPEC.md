# Source Generator Specification

## Overview

Source generator must analyze endpoint classes and generate registration code in module's `OnApplicationInitialization` method.

## Input: Endpoint Class

```csharp
public sealed class GetTenantByIdEndpoint
    : QueryOrNotFoundEndpoint<GetTenantByIdRequest, GetTenantByIdQuery, Tenant, TenantViewModel>
{
    public override string Route => "/api/tenants/{id:guid}";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithName("GetTenantById").WithTags("Tenants");
    }
}
```

## Output: Generated Code

Generator must create partial class extending the module:

```csharp
// Auto-generated file: CrmTenantsApiModule.Endpoints.g.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Cheetah.Core.CQRS;
using Cheetah.Mapping.Core;

namespace Cheetah.Tenants.Api;

partial class CrmTenantsApiModule
{
    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var routeBuilder = context.GetRouteBuilder();
        var serviceProvider = context.ServiceProvider;

        RegisterEndpoints(routeBuilder, serviceProvider);
    }

    private void RegisterEndpoints(IEndpointRouteBuilder routeBuilder, IServiceProvider serviceProvider)
    {
        var dispatcher = serviceProvider.GetRequiredService<IDispatcher>();
        var mapper = serviceProvider.GetRequiredService<IObjectMapper>();

        // GetTenantByIdEndpoint
        {
            var endpoint = new GetTenantByIdEndpoint();
            var builder = routeBuilder.MapGet(endpoint.Route, async (
                [AsParameters] GetTenantByIdRequest request,
                CancellationToken cancellationToken) =>
            {
                var query = mapper.Map<GetTenantByIdQuery>(request);
                var result = await dispatcher.QueryAsync<GetTenantByIdQuery, Tenant?>(query, cancellationToken);

                if (result == null)
                    return Results.NotFound();

                var response = mapper.Map<TenantViewModel>(result);
                return Results.Ok(response);
            });

            ApplyMetadata(builder, endpoint);
        }
    }

    private static void ApplyMetadata(RouteHandlerBuilder builder, IEndpointDefinition endpoint)
    {
        if (!string.IsNullOrEmpty(endpoint.Name))
            builder.WithName(endpoint.Name);

        if (!string.IsNullOrEmpty(endpoint.Description))
            builder.WithDescription(endpoint.Description);

        if (!string.IsNullOrEmpty(endpoint.Summary))
            builder.WithSummary(endpoint.Summary);

        if (endpoint.Tags.Length > 0)
            builder.WithTags(endpoint.Tags);

        if (endpoint.AllowAnonymous)
            builder.AllowAnonymous();

        foreach (var policy in endpoint.AuthorizationPolicies)
            builder.RequireAuthorization(policy);

        if (endpoint.IsDeprecated)
            builder.WithMetadata(new ObsoleteAttribute("This endpoint is deprecated"));

        builder.WithOpenApi();
    }
}
```

## Generator Rules

### 1. Find Endpoint Classes

Scan for all classes that inherit from:
- `QueryEndpoint<,,,>`
- `QueryOrNotFoundEndpoint<,,,>`
- `QueryCollectionEndpoint<,,,>`
- `CommandEndpoint<,>`
- `CommandWithResultEndpoint<,,,>`
- `CreateCommandEndpoint<,>`
- `UpdateCommandEndpoint<,>`
- `UpdateCommandWithResultEndpoint<,,,>`
- `PatchCommandEndpoint<,>`
- `DeleteCommandEndpoint<,>`

### 2. Extract Generic Type Parameters

For each endpoint, extract:
- `TRequest` - Request DTO type
- `TQuery` or `TCommand` - CQRS type
- `TQueryResult` or `TCommandResult` (if applicable) - Result type
- `TResponse` - Response DTO type

### 3. Determine HTTP Method

Based on base class:
- `QueryEndpoint` variants → `MapGet`
- `CommandEndpoint` / `CreateCommandEndpoint` → `MapPost`
- `UpdateCommandEndpoint` variants → `MapPut`
- `PatchCommandEndpoint` → `MapPatch`
- `DeleteCommandEndpoint` → `MapDelete`

### 4. Generate Handler Lambda

#### Pattern: QueryEndpoint

```csharp
routeBuilder.MapGet(endpoint.Route, async (
    [AsParameters] TRequest request,
    CancellationToken cancellationToken) =>
{
    var query = mapper.Map<TQuery>(request);
    var result = await dispatcher.QueryAsync<TQuery, TQueryResult>(query, cancellationToken);
    var response = mapper.Map<TResponse>(result);
    return Results.Ok(response);
});
```

#### Pattern: QueryOrNotFoundEndpoint

```csharp
routeBuilder.MapGet(endpoint.Route, async (
    [AsParameters] TRequest request,
    CancellationToken cancellationToken) =>
{
    var query = mapper.Map<TQuery>(request);
    var result = await dispatcher.QueryAsync<TQuery, TQueryResult?>(query, cancellationToken);

    if (result == null)
        return Results.NotFound();

    var response = mapper.Map<TResponse>(result);
    return Results.Ok(response);
});
```

#### Pattern: QueryCollectionEndpoint

```csharp
routeBuilder.MapGet(endpoint.Route, async (
    [AsParameters] TRequest request,
    CancellationToken cancellationToken) =>
{
    var query = mapper.Map<TQuery>(request);
    var results = await dispatcher.QueryAsync<TQuery, IReadOnlyList<TQueryResult>>(query, cancellationToken);
    var responses = mapper.Map<IReadOnlyList<TResponse>>(results);
    return Results.Ok(responses);
});
```

#### Pattern: CommandEndpoint (void)

```csharp
routeBuilder.MapPost(endpoint.Route, async (
    [FromBody] TRequest request,
    CancellationToken cancellationToken) =>
{
    var command = mapper.Map<TCommand>(request);
    await dispatcher.SendAsync(command, cancellationToken);
    return Results.NoContent();
});
```

#### Pattern: CommandWithResultEndpoint

```csharp
routeBuilder.MapPost(endpoint.Route, async (
    [FromBody] TRequest request,
    CancellationToken cancellationToken) =>
{
    var command = mapper.Map<TCommand>(request);
    var result = await dispatcher.SendAsync<TCommand, TCommandResult>(command, cancellationToken);
    var response = mapper.Map<TResponse>(result);
    return Results.Ok(response);
});
```

#### Pattern: CreateCommandEndpoint

```csharp
routeBuilder.MapPost(endpoint.Route, async (
    [FromBody] TRequest request,
    CancellationToken cancellationToken) =>
{
    var command = mapper.Map<TCommand>(request);
    var id = await dispatcher.SendAsync<TCommand, Guid>(command, cancellationToken);
    return Results.CreatedAtRoute(endpoint.GetByIdRouteName, new { id }, new GuidResponse(id));
});
```

#### Pattern: UpdateCommandEndpoint (void)

```csharp
routeBuilder.MapPut(endpoint.Route, async (
    [FromBody] TRequest request,
    CancellationToken cancellationToken) =>
{
    var command = mapper.Map<TCommand>(request);
    await dispatcher.SendAsync(command, cancellationToken);
    return Results.NoContent();
});
```

#### Pattern: DeleteCommandEndpoint

```csharp
routeBuilder.MapDelete(endpoint.Route, async (
    [AsParameters] TRequest request,
    CancellationToken cancellationToken) =>
{
    var command = mapper.Map<TCommand>(request);
    await dispatcher.SendAsync(command, cancellationToken);
    return Results.NoContent();
});
```

### 5. Request Parameter Binding

**Rule:**
- GET/DELETE endpoints → Use `[AsParameters]` (query string + route parameters)
- POST/PUT/PATCH endpoints → Use `[FromBody]` (request body)

### 6. Apply Metadata

For each registered endpoint, call `ApplyMetadata()` helper that checks:
- `endpoint.Name` → `.WithName()`
- `endpoint.Description` → `.WithDescription()`
- `endpoint.Summary` → `.WithSummary()`
- `endpoint.Tags` → `.WithTags()`
- `endpoint.AllowAnonymous` → `.AllowAnonymous()`
- `endpoint.AuthorizationPolicies` → `.RequireAuthorization(policy)` for each
- `endpoint.IsDeprecated` → `.WithMetadata(new ObsoleteAttribute())`
- Always call `.WithOpenApi()`

### 7. Add Produces Metadata

Based on endpoint type:

| Endpoint Type | Produces |
|---------------|----------|
| QueryEndpoint | 200 OK with TResponse |
| QueryOrNotFoundEndpoint | 200 OK with TResponse, 404 NotFound |
| QueryCollectionEndpoint | 200 OK with IReadOnlyList<TResponse> |
| CommandEndpoint | 204 NoContent, 400 BadRequest |
| CommandWithResultEndpoint | 200 OK with TResponse, 400 BadRequest |
| CreateCommandEndpoint | 201 Created with GuidResponse, 400 BadRequest |
| UpdateCommandEndpoint | 204 NoContent, 404 NotFound, 400 BadRequest |
| UpdateCommandWithResultEndpoint | 200 OK with TResponse, 404 NotFound, 400 BadRequest |
| PatchCommandEndpoint | 204 NoContent, 404 NotFound, 400 BadRequest |
| DeleteCommandEndpoint | 204 NoContent, 404 NotFound |

Example:
```csharp
builder.Produces<TResponse>(StatusCodes.Status200OK);
builder.Produces(StatusCodes.Status404NotFound);
```

## File Organization

Generated file should be named: `{ModuleName}.Endpoints.g.cs`

Example: `CrmTenantsApiModule.Endpoints.g.cs`

## Module Detection

Generator should:
1. Find all classes inheriting from endpoint base classes
2. Group by containing assembly
3. Find the CrmModule class in same assembly (class inheriting from `CrmModule`)
4. Generate partial class extending that module

## Edge Cases

1. **No endpoints found** → Don't generate file
2. **Multiple modules in assembly** → Generate separate method for each, or throw error
3. **Endpoint without Configure override** → Skip metadata application for that endpoint
4. **CreateCommandEndpoint missing GetByIdRouteName** → Compilation error (abstract property)

## Example Full Generation

Input assembly with 3 endpoints:
- GetAllTenantsEndpoint
- GetTenantByIdEndpoint
- CreateTenantEndpoint

Output `CrmTenantsApiModule.Endpoints.g.cs`:

```csharp
// <auto-generated/>
#nullable enable

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Cheetah.Core.CQRS;
using Cheetah.Mapping.Core;
using Cheetah.Backend.Endpoints.Abstractions;

namespace Cheetah.Tenants.Api;

partial class CrmTenantsApiModule
{
    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var routeBuilder = context.GetRouteBuilder();
        var serviceProvider = context.ServiceProvider;

        RegisterGeneratedEndpoints(routeBuilder, serviceProvider);
    }

    private void RegisterGeneratedEndpoints(IEndpointRouteBuilder routeBuilder, IServiceProvider serviceProvider)
    {
        var dispatcher = serviceProvider.GetRequiredService<IDispatcher>();
        var mapper = serviceProvider.GetRequiredService<IObjectMapper>();

        // GetAllTenantsEndpoint - QueryCollectionEndpoint
        {
            var endpoint = new GetAllTenantsEndpoint();
            var builder = routeBuilder.MapGet(endpoint.Route, async (
                [AsParameters] GetAllTenantsRequest request,
                CancellationToken cancellationToken) =>
            {
                var query = mapper.Map<GetAllTenantsQuery>(request);
                var results = await dispatcher.QueryAsync<GetAllTenantsQuery, IReadOnlyList<Tenant>>(query, cancellationToken);
                var responses = mapper.Map<IReadOnlyList<TenantViewModel>>(results);
                return Results.Ok(responses);
            });

            ApplyMetadata(builder, endpoint);
            builder.Produces<IReadOnlyList<TenantViewModel>>(StatusCodes.Status200OK);
        }

        // GetTenantByIdEndpoint - QueryOrNotFoundEndpoint
        {
            var endpoint = new GetTenantByIdEndpoint();
            var builder = routeBuilder.MapGet(endpoint.Route, async (
                [AsParameters] GetTenantByIdRequest request,
                CancellationToken cancellationToken) =>
            {
                var query = mapper.Map<GetTenantByIdQuery>(request);
                var result = await dispatcher.QueryAsync<GetTenantByIdQuery, Tenant?>(query, cancellationToken);

                if (result == null)
                    return Results.NotFound();

                var response = mapper.Map<TenantViewModel>(result);
                return Results.Ok(response);
            });

            ApplyMetadata(builder, endpoint);
            builder.Produces<TenantViewModel>(StatusCodes.Status200OK);
            builder.Produces(StatusCodes.Status404NotFound);
        }

        // CreateTenantEndpoint - CreateCommandEndpoint
        {
            var endpoint = new CreateTenantEndpoint();
            var builder = routeBuilder.MapPost(endpoint.Route, async (
                [FromBody] CreateTenantRequest request,
                CancellationToken cancellationToken) =>
            {
                var command = mapper.Map<CreateTenantCommand>(request);
                var id = await dispatcher.SendAsync<CreateTenantCommand, Guid>(command, cancellationToken);
                return Results.CreatedAtRoute(endpoint.GetByIdRouteName, new { id }, new GuidResponse(id));
            });

            ApplyMetadata(builder, endpoint);
            builder.Produces<GuidResponse>(StatusCodes.Status201Created);
            builder.Produces(StatusCodes.Status400BadRequest);
        }
    }

    private static void ApplyMetadata(RouteHandlerBuilder builder, IEndpointDefinition endpoint)
    {
        if (!string.IsNullOrEmpty(endpoint.Name))
            builder.WithName(endpoint.Name);

        if (!string.IsNullOrEmpty(endpoint.Description))
            builder.WithDescription(endpoint.Description);

        if (!string.IsNullOrEmpty(endpoint.Summary))
            builder.WithSummary(endpoint.Summary);

        if (endpoint.Tags.Length > 0)
            builder.WithTags(endpoint.Tags);

        if (endpoint.AllowAnonymous)
            builder.AllowAnonymous();

        foreach (var policy in endpoint.AuthorizationPolicies)
            builder.RequireAuthorization(policy);

        if (endpoint.IsDeprecated)
            builder.WithMetadata(new ObsoleteAttribute("This endpoint is deprecated"));

        builder.WithOpenApi();
    }
}
```
