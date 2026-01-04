# Cheetah CRM - Developer Guide

## 🎯 Concept

Modular .NET 10.0 CRM framework with event-driven architecture. Monolith with microservices preparation.
**Frontend:** Blazor WebAssembly. **Target:** 10,000+ RPS.

## 🏗️ Architecture

### Module System

- Independent modules communicate via events (Redis backend, In-Memory frontend)
- Each module has own database (PostgreSQL)
- Auto-registration via Source Generators
- Topological dependency sorting

**Module Structure (9 assemblies per large module):**
```
Cheetah.{ModuleName}/
├── Events/              # Domain Events (contracts, NO dependencies)
├── Domain/              # Entities, Value Objects (depends on Events)
├── Application/         # CQRS, Business Logic (depends on Domain)
├── DataAccess/          # EF Core, Migrations (depends on Domain)
├── Api/                 # Minimal API (depends on Application)
├── Shared/              # DTOs, ViewModels (NO dependencies)
├── Frontend/            # Blazor components (depends on Shared)
├── Client/              # Backend HTTP client (depends on Shared)
├── Frontend.Client/     # Blazor CQRS handlers (depends on Client)
└── Tests/
    ├── Client.Tests/
    └── Frontend.Client.Tests/
```

**Solution Organization:** All projects in `/Modules/{ModuleName}/` folder.

**Module Class:**
```csharp
[DependsOn(typeof(SomeOtherModule))]
public partial class MyModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services); // Auto-generated
    }
}
```

**Lifecycle:** PreConfigureServices → ConfigureServices → PostConfigureServices → OnPreApplicationInitialization → OnApplicationInitialization → OnPostApplicationInitialization

### Domain Layer (DDD)

**Base Classes:** `Entity<TId>`, `AggregateRoot<TId>`, `ValueObject`
**Audit:** `ICreateAtEntity`, `IUpdatedAtEntity`, `IRemovedAtEntity`

```csharp
public class Tenant : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public string Name { get; private set; } = null!; // Always private set
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    private Tenant() { } // For EF Core

    public static Tenant Create(string name)
    {
        var tenant = new Tenant { Id = Guid.NewGuid(), Name = name };
        tenant.AddDomainEvent(new TenantCreatedEvent(tenant.Id, name));
        return tenant;
    }
}
```

### Application Layer (CQRS)

**Commands** (state changes) return `Guid` or `void`. **Queries** return ViewModels.
Use `ValueTask<T>` for hot paths, always pass `CancellationToken`.

```csharp
public record CreateTenantCommand(string Name) : ICommand<Guid>;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateTenantCommand, Guid>))]
public class CreateTenantCommandHandler : ICommandHandler<CreateTenantCommand, Guid>
{
    private readonly MyDbContext _db;
    private readonly IEventBus _eventBus;

    public async ValueTask<Guid> HandleAsync(CreateTenantCommand cmd, CancellationToken ct)
    {
        var tenant = Tenant.Create(cmd.Name);
        _db.Tenants.Add(tenant);
        await _db.SaveChangesAsync(ct);

        foreach (var e in tenant.DomainEvents)
            await _eventBus.PublishAsync(e, ct);
        tenant.ClearDomainEvents();

        return tenant.Id;
    }
}
```

### DataAccess Layer (EF Core)

**MUST** depend on `CrmEntityFrameworkModule` + `CrmEntityFrameworkPostgreSqlModule`.

```csharp
[DependsOn(typeof(MyDomainModule))]
[DependsOn(typeof(CrmEntityFrameworkModule))]
[DependsOn(typeof(CrmEntityFrameworkPostgreSqlModule))]
public partial class MyDataAccessModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
        context.Services.AddDbContext<MyDbContext>(options =>
            options.UseNpgsql(context.Services.GetConfiguration().GetConnectionString("MyModule")));
    }
}

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");
        builder.Ignore(t => t.DomainEvents); // CRITICAL!
    }
}
```

### Api Layer (Minimal API ONLY)

**Controllers are FORBIDDEN.** Depend on `CrmMapsterModule`.

```csharp
[DependsOn(typeof(MyApplicationModule))]
[DependsOn(typeof(CrmMapsterModule))]
public partial class MyApiModule : CrmModule
{
    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var routeBuilder = context.GetRouteBuilder();
        var mapper = context.ServiceProvider.GetRequiredService<IObjectMapper>();

        routeBuilder.MapPost("/api/tenants", async (
            [FromBody] CreateTenantRequest request,
            [FromServices] IDispatcher dispatcher,
            CancellationToken ct) =>
        {
            var command = mapper.Map<CreateTenantCommand>(request);
            var id = await dispatcher.SendAsync(command, ct);
            return Results.Created($"/api/tenants/{id}", await dispatcher.SendAsync(new GetTenantByIdQuery(id), ct));
        })
        .WithName("CreateTenant")
        .WithOpenApi();
    }
}
```

### Events (Separate Project)

Events in `Cheetah.{ModuleName}.Events` - pure data contracts, NO dependencies.

```csharp
namespace Cheetah.Tenants.Events;

public record TenantCreatedEvent(Guid TenantId, string Name) : EventBase;
```

**Event Subscription:**
```csharp
[DependsOn(typeof(CrmTenantsEventsModule))] // Only depend on Events!
public class CrmFeaturesApplicationModule : CrmModule
{
    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var eventBus = context.ServiceProvider.GetRequiredService<IEventBus>();
        eventBus.Subscribe<TenantCreatedEvent, TenantCreatedEventHandler>();
    }
}
```

### Client Libraries

**Backend Client** (`Client/`): HTTP client for server-to-server integration.

```csharp
public interface ITenantClient
{
    ValueTask<TenantViewModel?> GetByIdAsync(Guid id, CancellationToken ct = default);
    ValueTask<Guid> CreateAsync(CreateTenantRequest request, CancellationToken ct = default);
}

[Export(LifetimeType.Scoped, typeof(ITenantClient))]
public class TenantClient : ITenantClient
{
    private readonly HttpClient _httpClient;

    public TenantClient(IHttpClientFactory factory) =>
        _httpClient = factory.CreateClient("CheetahAPI");
}
```

**Frontend Client** (`Frontend.Client/`): CQRS handlers using Backend Client for Blazor.

```csharp
[DependsOn(typeof(CrmTenantsClientModule))]
[DependsOn(typeof(CrmFrontendCQRSModule))]
public partial class CrmTenantsFrontendClientModule : CrmModule { }
```

**Both MUST have comprehensive tests.**

### Dependency Injection

```csharp
[Export(LifetimeType.Scoped, typeof(IMyService))]
public class MyService : IMyService { }
```

Lifetimes: `Singleton`, `Scoped`, `Transient`

## ⚡ Performance Best Practices

- Use `AsNoTracking()` for read-only queries
- Use `ValueTask<T>` for hot paths (CQRS handlers)
- Avoid N+1 queries - use `Include()` or projection
- Use projection (`Select`) instead of full entities
- Minimize allocations (`ArrayPool`, `Span<T>`, `Memory<T>`)
- Use compiled queries for frequent operations
- Always publish events AFTER `SaveChangesAsync()`

## 📋 Key Rules

### Naming
- **Entities:** `Tenant.cs` (singular)
- **Events:** `TenantCreatedEvent.cs` (Event suffix)
- **Commands:** `CreateTenantCommand.cs` (verb + entity + Command)
- **Queries:** `GetTenantByIdQuery.cs` (Get/List + Query)
- **ViewModels:** `TenantViewModel.cs` (ViewModel suffix)
- **DbContext:** `TenantsDbContext.cs` (plural)
- **Modules:** `CrmTenantsApiModule.cs` (Crm + name + layer + Module)

### Folder Structure
```
Cheetah.Tenants.Application/
├── Commands/        # CreateTenantCommand.cs + Handler
├── Queries/         # GetTenantByIdQuery.cs + Handler
├── Services/        # ITenantResolver.cs
├── EventHandlers/   # Handlers for OTHER modules' events
└── CrmTenantsApplicationModule.cs
```

### Project Configuration

Use `$(MsPackageVersion)` for Microsoft packages (defined in `src/Directory.Build.props`):

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="$(MsPackageVersion)" />
```

## 🚀 Creating New Module

1. **Events** project FIRST (NO dependencies, pure contracts)
2. Domain (depends on Events)
3. DataAccess (depends on `CrmEntityFrameworkModule` + `CrmEntityFrameworkPostgreSqlModule`)
4. Application (CQRS handlers)
5. Api (Minimal API, depends on `CrmMapsterModule`)
6. Shared (pure DTOs)
7. Client (HTTP client)
8. Frontend.Client (CQRS handlers using Client, depends on `CrmFrontendCQRSModule`)
9. Frontend (Blazor components)
10. Tests (Client.Tests + Frontend.Client.Tests)
11. Add ALL projects to solution in `/Modules/{ModuleName}/` folder

## ⚠️ Critical Constraints

1. **ONLY Minimal API** - Controllers forbidden
2. **Endpoints in `OnApplicationInitialization`** - NOT in `ConfigureServices`
3. **Use `CrmMapsterModule`** - inject `IObjectMapper` for mapping
4. **MUST use `CrmEntityFrameworkModule` + `CrmEntityFrameworkPostgreSqlModule`**
5. **PostgreSQL default** - use `UseNpgsql()`
6. **Events project has NO dependencies** (except EventBase from Core)
7. **Other modules depend ONLY on Events** - not Domain/Application
8. **2 client libraries required** - Client + Frontend.Client with tests
9. **All projects in solution** - organized in `/Modules/{ModuleName}/`
10. **Don't expose Domain entities** - only ViewModels
11. **Always `builder.Ignore(t => t.DomainEvents)`** in EF config
12. **Module classes are `partial`** - for Source Generators
13. **Always use `[FromServices]`, `[FromRoute]`, `[FromBody]`, `[FromQuery]`**
14. **Blazor WASM via API only** - through Client libraries

## 📚 Key Files

- Modularity: `src/Cheetah.Core/Modularity/CrmModule.cs`
- CQRS: `src/Cheetah.Core.CQRS/IDispatcher.cs`
- Events: `src/Cheetah.Core.Events/IEventBus.cs`
- Domain: `src/Cheetah.Core.Domain/AggregateRoot.cs`

## 🎯 Current Modules

1. **Cheetah.Tenants** - tenant management
2. **Cheetah.Features** - feature flags
3. **Cheetah.Permissions** - RBAC
4. **Cheetah.Identity** - users, JWT auth
