# Cheetah.Backend.Endpoints - Usage Examples

## Overview

Endpoints are **fully declarative** - no HandleAsync method needed. The system automatically:
1. Maps Request → Command/Query using IObjectMapper
2. Executes Command/Query via IDispatcher
3. Maps Result → Response using IObjectMapper
4. Returns appropriate HTTP result

## Example 1: Query Endpoint (GET)

### GET /api/tenants/{id} - Get tenant by ID

```csharp
using Cheetah.Backend.Endpoints.Http;
using Cheetah.Tenants.Contracts.Requests;
using Cheetah.Tenants.Contracts.ViewModels;
using Cheetah.Tenants.Application.Queries;
using Cheetah.Tenants.Domain.Entities;

namespace Cheetah.Tenants.Api.Endpoints;

public sealed class GetTenantByIdEndpoint
    : QueryOrNotFoundEndpoint<GetTenantByIdRequest, GetTenantByIdQuery, Tenant, TenantViewModel>
{
    public override string Route => "/api/tenants/{id:guid}";

    protected override void Configure(EndpointConfiguration config)
    {
        config
            .WithName("GetTenantById")
            .WithDescription("Retrieves a specific tenant by ID")
            .WithTags("Tenants");
    }
}
```

**Request DTO:**
```csharp
public sealed record GetTenantByIdRequest(
    [FromRoute] Guid Id
) : ICrmRequest;
```

**Automatic behavior:**
- Maps `GetTenantByIdRequest` → `GetTenantByIdQuery`
- Executes query via `IDispatcher`
- Maps `Tenant` → `TenantViewModel`
- Returns 200 OK with ViewModel, or 404 if null

---

## Example 2: Query Collection Endpoint (GET)

### GET /api/tenants - Get all tenants

```csharp
public sealed class GetAllTenantsEndpoint
    : QueryCollectionEndpoint<GetAllTenantsRequest, GetAllTenantsQuery, Tenant, TenantViewModel>
{
    public override string Route => "/api/tenants";

    protected override void Configure(EndpointConfiguration config)
    {
        config
            .WithName("GetAllTenants")
            .WithDescription("Retrieves all tenants")
            .WithTags("Tenants")
            .RequireAuthorization("Admin");
    }
}
```

**Request DTO:**
```csharp
public sealed record GetAllTenantsRequest : ICrmRequest;
```

**Automatic behavior:**
- Maps request → `GetAllTenantsQuery`
- Executes query returning `IReadOnlyList<Tenant>`
- Maps to `IReadOnlyList<TenantViewModel>`
- Returns 200 OK with collection

---

## Example 3: Create Command Endpoint (POST)

### POST /api/tenants - Create new tenant

```csharp
public sealed class CreateTenantEndpoint
    : CreateCommandEndpoint<CreateTenantRequest, CreateTenantCommand>
{
    public override string Route => "/api/tenants";

    protected override string GetByIdRouteName => "GetTenantById";

    protected override void Configure(EndpointConfiguration config)
    {
        config
            .WithName("CreateTenant")
            .WithDescription("Creates a new tenant")
            .WithTags("Tenants")
            .RequirePermissions("Tenants.Create");
    }
}
```

**Request DTO:**
```csharp
public sealed record CreateTenantRequest(
    string Name,
    string? Subdomain,
    string? Description
) : ICrmRequest;
```

**Automatic behavior:**
- Maps request → `CreateTenantCommand`
- Executes command returning `Guid`
- Returns 201 Created with:
  - Location header: `/api/tenants/{id}`
  - Body: `{ "id": "guid" }`

---

## Example 4: Command Without Result (POST)

### POST /api/tenants/{id}/activate - Activate tenant

```csharp
public sealed class ActivateTenantEndpoint
    : CommandEndpoint<ActivateTenantRequest, ActivateTenantCommand>
{
    public override string Route => "/api/tenants/{id:guid}/activate";

    protected override void Configure(EndpointConfiguration config)
    {
        config
            .WithName("ActivateTenant")
            .WithDescription("Activates a tenant")
            .WithTags("Tenants");
    }
}
```

**Request DTO:**
```csharp
public sealed record ActivateTenantRequest(
    [FromRoute] Guid Id
) : ICrmRequest;
```

**Automatic behavior:**
- Maps request → `ActivateTenantCommand`
- Executes void command
- Returns 204 NoContent

---

## Example 5: Update Command (PUT)

### PUT /api/tenants/{id} - Update tenant

```csharp
public sealed class UpdateTenantEndpoint
    : UpdateCommandEndpoint<UpdateTenantRequest, UpdateTenantCommand>
{
    public override string Route => "/api/tenants/{id:guid}";

    protected override void Configure(EndpointConfiguration config)
    {
        config
            .WithName("UpdateTenant")
            .WithDescription("Updates an existing tenant")
            .WithTags("Tenants");
    }
}
```

**Request DTO:**
```csharp
public sealed record UpdateTenantRequest(
    [FromRoute] Guid Id,
    [FromBody] UpdateTenantBody Body
) : ICrmRequest;

public sealed record UpdateTenantBody(
    string Name,
    string? Description
);
```

**Automatic behavior:**
- Maps request → `UpdateTenantCommand`
- Executes void command
- Returns 204 NoContent (or 404 if not found)

---

## Example 6: Delete Command (DELETE)

### DELETE /api/tenants/{id} - Delete tenant

```csharp
public sealed class DeleteTenantEndpoint
    : DeleteCommandEndpoint<DeleteTenantRequest, DeleteTenantCommand>
{
    public override string Route => "/api/tenants/{id:guid}";

    protected override void Configure(EndpointConfiguration config)
    {
        config
            .WithName("DeleteTenant")
            .WithDescription("Deletes a tenant")
            .WithTags("Tenants")
            .RequirePermissions("Tenants.Delete");
    }
}
```

**Request DTO:**
```csharp
public sealed record DeleteTenantRequest(
    [FromRoute] Guid Id
) : ICrmRequest;
```

**Automatic behavior:**
- Maps request → `DeleteTenantCommand`
- Executes void command
- Returns 204 NoContent (or 404 if not found)

---

## Example 7: Command With Result (POST)

### POST /api/tenants/validate - Validate tenant data

```csharp
public sealed class ValidateTenantEndpoint
    : CommandWithResultEndpoint<ValidateTenantRequest, ValidateTenantCommand, ValidationResult, ValidationResponse>
{
    public override string Route => "/api/tenants/validate";

    protected override void Configure(EndpointConfiguration config)
    {
        config
            .WithName("ValidateTenant")
            .WithDescription("Validates tenant data without creating")
            .WithTags("Tenants")
            .AllowAnonymousAccess();
    }
}
```

**Request DTO:**
```csharp
public sealed record ValidateTenantRequest(
    string Name,
    string? Subdomain
) : ICrmRequest;
```

**Automatic behavior:**
- Maps request → `ValidateTenantCommand`
- Executes command returning `ValidationResult`
- Maps to `ValidationResponse`
- Returns 200 OK with response

---

## Example 8: Query with Filtering (GET)

### GET /api/tenants/search - Search tenants

```csharp
public sealed class SearchTenantsEndpoint
    : QueryCollectionEndpoint<SearchTenantsRequest, SearchTenantsQuery, Tenant, TenantViewModel>
{
    public override string Route => "/api/tenants/search";

    protected override void Configure(EndpointConfiguration config)
    {
        config
            .WithName("SearchTenants")
            .WithDescription("Searches tenants by criteria")
            .WithTags("Tenants");
    }
}
```

**Request DTO:**
```csharp
public sealed record SearchTenantsRequest(
    [FromQuery] string? Name = null,
    [FromQuery] bool? IsActive = null,
    [FromQuery] int PageSize = 20,
    [FromQuery] int PageNumber = 1
) : ICrmRequest;
```

**Automatic behavior:**
- Maps request (with query parameters) → `SearchTenantsQuery`
- Executes query
- Returns filtered collection

---

## Mapping Configuration

For automatic mapping to work, configure Mapster profiles:

```csharp
// Request → Command mapping
public class TenantRequestMappingProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<GetTenantByIdRequest, GetTenantByIdQuery>()
            .Map(dest => dest.Id, src => src.Id);

        config.NewConfig<CreateTenantRequest, CreateTenantCommand>()
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Subdomain, src => src.Subdomain);
    }
}

// Domain → ViewModel mapping
public class TenantViewModelMappingProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Tenant, TenantViewModel>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.IsActive, src => src.IsActive);
    }
}
```

---

## Authorization Examples

```csharp
// Require authentication
protected override void Configure(EndpointConfiguration config)
{
    config.RequireAuthorization();
}

// Require specific policy
protected override void Configure(EndpointConfiguration config)
{
    config.RequireAuthorization("AdminPolicy");
}

// Require permissions
protected override void Configure(EndpointConfiguration config)
{
    config.RequirePermissions("Tenants.Read", "Tenants.Update");
}

// Allow anonymous
protected override void Configure(EndpointConfiguration config)
{
    config.AllowAnonymousAccess();
}
```

---

## Available Endpoint Base Classes

| Class | HTTP Method | Use Case | Returns |
|-------|-------------|----------|---------|
| `QueryEndpoint` | GET | Query that always returns data | 200 OK |
| `QueryOrNotFoundEndpoint` | GET | Query by ID (may be null) | 200 OK / 404 |
| `QueryCollectionEndpoint` | GET | Query returning collection | 200 OK |
| `CommandEndpoint` | POST | Command without result | 204 NoContent |
| `CommandWithResultEndpoint` | POST | Command with result | 200 OK |
| `CreateCommandEndpoint` | POST | Create resource | 201 Created |
| `UpdateCommandEndpoint` | PUT | Full update | 204 NoContent |
| `UpdateCommandWithResultEndpoint` | PUT | Update with result | 200 OK |
| `PatchCommandEndpoint` | PATCH | Partial update | 204 NoContent |
| `DeleteCommandEndpoint` | DELETE | Delete resource | 204 NoContent |

---

## Type Parameters

**QueryEndpoint:**
- `TRequest` - HTTP request DTO
- `TQuery` - CQRS query
- `TQueryResult` - Domain model from query handler
- `TResponse` - HTTP response DTO

**CommandEndpoint:**
- `TRequest` - HTTP request DTO
- `TCommand` - CQRS command

**CommandWithResultEndpoint:**
- `TRequest` - HTTP request DTO
- `TCommand` - CQRS command
- `TCommandResult` - Result from command handler
- `TResponse` - HTTP response DTO

## Rate limiting

Декларативно через `WithRateLimit(policyName, keySource)`. Source Generator подставит `RateLimitFilter` в сгенерированный endpoint:

```csharp
public class LoginEndpoint : CommandEndpoint<LoginRequest, LoginCommand>
{
    public override string Route => "/api/auth/login";

    protected override void Configure(EndpointConfiguration cfg)
        => cfg
            .AllowAnonymousAccess()
            .WithRateLimit("login", RateLimitKeySource.Ip);  // 5 попыток в минуту по IP
}
```

`RateLimitKeySource`:
- `UserOrIp` (default) — UserId если аутентифицирован, иначе IP
- `UserOnly` — только UserId; анонимный запрос → 401
- `Ip` — RemoteIpAddress (с учётом X-Forwarded-For при правильно настроенном ForwardedHeaders)
- `Global` — один лимит на весь endpoint без partitioning (для admin-операций "не чаще 1 раз в час")

Политика берётся из конфига `RateLimit:Policies:login` (см. `Cheetah.RateLimit.Redis`).

Если `IDistributedRateLimiter` не зарегистрирован — filter fail-open (пропускает запрос). Это удобно для dev-окружения без Redis.

**Headers ответа:**
- `X-RateLimit-Limit` — лимит политики
- `X-RateLimit-Remaining` — сколько осталось в текущем окне
- `Retry-After` (при 429) — секунды до сброса
