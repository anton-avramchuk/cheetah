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

**Module Structure (11 assemblies per large module):**
```
Cheetah.{ModuleName}/
├── Events/              # Domain Events (contracts, NO dependencies except Core.Events)
├── Shared/              # Constants, Enums (depends ONLY on Core)
├── Contracts/           # DTOs, ViewModels, Requests (depends on Core + Shared)
├── Domain/              # Entities, Value Objects (depends on Events)
├── Application/         # CQRS, Business Logic (depends on Domain)
├── DataAccess/          # EF Core, Migrations (depends on Domain)
├── Api/                 # Minimal API (depends on Application + Contracts)
├── Frontend/            # Blazor components (depends on Contracts)
├── Client/              # Backend HTTP client (depends on Contracts)
├── Frontend.Client/     # Blazor CQRS handlers (depends on Client)
└── Tests/
    ├── Domain.Tests/          # Unit tests for Domain layer
    ├── Application.Tests/     # Unit tests for Application layer
    ├── Client.Tests/          # Unit tests for Client library
    └── Frontend.Client.Tests/ # Unit tests for Frontend.Client library
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

**CRITICAL: Application layer MUST NOT use DbContext directly. Use Repository pattern with Specifications.**

```csharp
public record CreateTenantCommand(string Name) : ICommand<Guid>;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateTenantCommand, Guid>))]
public class CreateTenantCommandHandler : ICommandHandler<CreateTenantCommand, Guid>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IEventBus _eventBus;

    public async ValueTask<Guid> HandleAsync(CreateTenantCommand cmd, CancellationToken ct)
    {
        var tenant = Tenant.Create(cmd.Name);
        _tenantRepository.Add(tenant);
        await _tenantRepository.SaveChangesAsync(ct);

        foreach (var e in tenant.DomainEvents)
            await _eventBus.PublishAsync(e, ct);
        tenant.ClearDomainEvents();

        return tenant.Id;
    }
}
```

### Repository Pattern & Specifications

**Application layer uses repositories, NOT DbContext directly.**

**Repository Interface (Domain layer):**
```csharp
public interface ITenantRepository
{
    ValueTask<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default);
    ValueTask<Tenant?> GetBySpecAsync(ISpecification<Tenant> spec, CancellationToken ct = default);
    ValueTask<List<Tenant>> GetAllAsync(ISpecification<Tenant>? spec = null, CancellationToken ct = default);
    ValueTask<bool> ExistsAsync(ISpecification<Tenant> spec, CancellationToken ct = default);
    void Add(Tenant tenant);
    void Update(Tenant tenant);
    void Delete(Tenant tenant);
    ValueTask<int> SaveChangesAsync(CancellationToken ct = default);
    IQueryable<Tenant> AsQueryable();
    IQueryable<Tenant> AsNoTrackingQueryable();
}
```

**Specification (Domain layer):**
```csharp
public class TenantByNameSpecification : Specification<Tenant>
{
    private readonly string _normalizedName;

    public TenantByNameSpecification(string name)
    {
        _normalizedName = name.ToUpperInvariant();
    }

    public override Expression<Func<Tenant, bool>> ToExpression()
    {
        return tenant => tenant.NormalizedName == _normalizedName;
    }
}

public class ActiveTenantsSpecification : Specification<Tenant>
{
    public override Expression<Func<Tenant, bool>> ToExpression()
    {
        return tenant => tenant.IsActive;
    }
}
```

**Repository Implementation (DataAccess layer):**
```csharp
[Export(LifetimeType.Scoped, typeof(ITenantRepository))]
public class TenantRepository : ITenantRepository
{
    private readonly MyDbContext _context;

    public TenantRepository(MyDbContext context)
    {
        _context = context;
    }

    public async ValueTask<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Tenants.FindAsync(new object[] { id }, ct);
    }

    public async ValueTask<Tenant?> GetBySpecAsync(ISpecification<Tenant> spec, CancellationToken ct = default)
    {
        return await _context.Tenants
            .Where(spec.ToExpression())
            .FirstOrDefaultAsync(ct);
    }

    public async ValueTask<List<Tenant>> GetAllAsync(ISpecification<Tenant>? spec = null, CancellationToken ct = default)
    {
        var query = _context.Tenants.AsQueryable();

        if (spec != null)
            query = query.Where(spec.ToExpression());

        return await query.ToListAsync(ct);
    }

    public IQueryable<Tenant> AsNoTrackingQueryable()
    {
        return _context.Tenants.AsNoTracking();
    }

    // Other methods...
}
```

**Usage in Query Handler:**
```csharp
public class GetTenantByNameQueryHandler : IQueryHandler<GetTenantByNameQuery, TenantViewModel?>
{
    private readonly ITenantRepository _tenantRepository;

    public async ValueTask<TenantViewModel?> HandleAsync(GetTenantByNameQuery query, CancellationToken ct)
    {
        var spec = new TenantByNameSpecification(query.Name);
        var tenant = await _tenantRepository.GetBySpecAsync(spec, ct);

        return tenant != null ? new TenantViewModel
        {
            Id = tenant.Id,
            Name = tenant.Name,
            IsActive = tenant.IsActive
        } : null;
    }
}
```

**For complex queries with projections, use AsNoTrackingQueryable():**
```csharp
public class GetAllTenantsQueryHandler : IQueryHandler<GetAllTenantsQuery, List<TenantViewModel>>
{
    private readonly ITenantRepository _tenantRepository;

    public async ValueTask<List<TenantViewModel>> HandleAsync(GetAllTenantsQuery query, CancellationToken ct)
    {
        return await _tenantRepository.AsNoTrackingQueryable()
            .Select(t => new TenantViewModel
            {
                Id = t.Id,
                Name = t.Name,
                IsActive = t.IsActive
            })
            .ToListAsync(ct);
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

### Shared and Contracts Libraries

For large modules with complex data structures and validation rules, use separate **Shared** and **Contracts** libraries as demonstrated in the Tenants module.

**Shared Library** (`{ModuleName}.Shared/`):
- **Purpose**: Constants, enums, shared configuration values
- **Dependencies**: ONLY depends on `Cheetah.Core`
- **NO business logic, NO DTOs**

```csharp
// Cheetah.Tenants.Shared/Constants/TenantConstants.cs
namespace Cheetah.Tenants.Shared.Constants;

public static class TenantConstants
{
    public const int MaxNameLength = 256;
    public const int MaxNormalizedNameLength = 256;
    public const int MaxSubdomainLength = 128;
    public const int MaxDescriptionLength = 2000;
}
```

**Contracts Library** (`{ModuleName}.Contracts/`):
- **Purpose**: DTOs (ViewModels, Requests, Responses)
- **Dependencies**: `Cheetah.Core` + `{ModuleName}.Shared`
- **NO business logic, ONLY data contracts**

```csharp
// Cheetah.Tenants.Contracts/ViewModels/TenantViewModel.cs
namespace Cheetah.Tenants.Contracts.ViewModels;

public class TenantViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
}

// Cheetah.Tenants.Contracts/Requests/CreateTenantRequest.cs
namespace Cheetah.Tenants.Contracts.Requests;

public class CreateTenantRequest
{
    public string Name { get; set; } = null!;
    public string? Subdomain { get; set; }
}
```

**When to Use Shared/Contracts:**
- ✅ Large modules with many DTOs (5+ ViewModels/Requests)
- ✅ Shared constants referenced by Domain, Application, and API layers
- ✅ Modules that need clear separation of concerns
- ❌ Small modules with 2-3 simple DTOs (use single Contracts library)
- ❌ Modules with no reusable constants (skip Shared)

**Dependency Order:**
```
Events (no deps)
  ↓
Shared (Core only)
  ↓
Contracts (Core + Shared)
  ↓
Domain (Events) → Application (Domain) → Api (Application + Contracts)
  ↓                 ↓
DataAccess (Domain)  ↓
                     ↓
Client (Contracts) → Frontend.Client (Client) → Frontend (Contracts)
```

**Example Module Structure (Tenants):**
```
Cheetah.Tenants/
├── Cheetah.Tenants.Events/          # TenantCreatedEvent
├── Cheetah.Tenants.Shared/          # TenantConstants
├── Cheetah.Tenants.Contracts/       # TenantViewModel, CreateTenantRequest
├── Cheetah.Tenants.Domain/          # Tenant entity
├── Cheetah.Tenants.Application/     # CreateTenantCommandHandler
├── Cheetah.Tenants.DataAccess/      # TenantsDbContext
├── Cheetah.Tenants.Api/             # Minimal API endpoints
├── Cheetah.Tenants.Client/          # ITenantClient
├── Cheetah.Tenants.Frontend.Client/ # Frontend CQRS handlers
└── Cheetah.Tenants.Frontend/        # Blazor components
```

### Tenant-Based Modules & Database Provisioning

Each tenant-based module (Identity, Features, etc.) has its own database per tenant. Connection strings are automatically generated and managed.

**Step 1: Register Module Connection String Provider**

Every module that needs a tenant-specific database MUST register `IModuleConnectionStringProvider`:

```csharp
// Cheetah.Identity.Application/CrmIdentityApplicationModule.cs
[DependsOn(typeof(CrmIdentityDomainModule))]
public partial class CrmIdentityApplicationModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);

        // Register connection string provider
        context.Services.AddSingleton<IModuleConnectionStringProvider>(
            new DefaultModuleConnectionStringProvider("Identity"));
    }
}
```

**Step 2: Register Tenant-Based DbContext Provider**

Create a provider that implements `ITenantBasedDbContext<TenantCreatedEvent>`:

```csharp
// Cheetah.Identity.DataAccess/IdentityTenantDbContextProvider.cs
[Export(LifetimeType.Singleton, typeof(ITenantBasedDbContext<TenantCreatedEvent>))]
public class IdentityTenantDbContextProvider : ITenantBasedDbContext<TenantCreatedEvent>
{
    public string ModuleName => "Identity";

    public ITenantBasedDbContext<TenantCreatedEvent> CreateForTenant(string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<IdentityDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        var dbContext = new IdentityDbContext(optionsBuilder.Options);
        return new TenantIdentityDbContext(dbContext);
    }
}
```

**Step 3: Configuration (appsettings.json)**

```json
{
  "ConnectionStrings": {
    "TenantTemplate": "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=template"
  }
}
```

**Automatic Workflow:**

1. User creates tenant: `POST /api/tenants { "name": "Acme Corp" }`
2. `CreateTenantCommandHandler` creates `Tenant` entity and publishes `TenantCreatedEvent`
3. `GenerateTenantConnectionStringsEventHandler` automatically:
   - Discovers all `IModuleConnectionStringProvider` instances
   - Generates connection strings for each module:
     - Identity: `"Host=...;Database=tenant_123abc_identity"`
     - Features: `"Host=...;Database=tenant_123abc_features"`
   - Adds `TenantConnectionString` records to tenant
4. `TenantCreatedEventHandler` automatically:
   - Discovers all `ITenantBasedDbContext<TenantCreatedEvent>` instances
   - For each module: creates database and runs migrations using `TenantDatabaseMigrationManager`

**Result:** Each tenant gets separate databases for each module with automatic provisioning.

**Custom Connection String Generation:**

```csharp
public class CustomConnectionStringProvider : IModuleConnectionStringProvider
{
    public string ModuleName => "Identity";

    public string GenerateConnectionString(Guid tenantId, string tenantName, string baseConnectionString)
    {
        var sanitizedName = tenantName.ToLowerInvariant().Replace(" ", "_");
        var builder = new DbConnectionStringBuilder { ConnectionString = baseConnectionString };
        builder["Database"] = $"{sanitizedName}_identity";
        return builder.ConnectionString;
    }
}
```

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

### Testing Application Layer

**Use custom TestAsyncQueryProvider for testing EF Core async operations:**

```csharp
// Test helper for mocking IQueryable with async support
internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    internal TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(Expression expression)
    {
        return new TestAsyncEnumerable<TEntity>(expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new TestAsyncEnumerable<TElement>(expression);
    }

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        var resultType = typeof(TResult).GetGenericArguments()[0];
        var executionResult = typeof(IQueryProvider)
            .GetMethod(nameof(IQueryProvider.Execute), 1, new[] { typeof(Expression) })!
            .MakeGenericMethod(resultType)
            .Invoke(this, new[] { expression });

        return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
            .MakeGenericMethod(resultType)
            .Invoke(null, new[] { executionResult })!;
    }
}

internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
    public TestAsyncEnumerable(Expression expression) : base(expression) { }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
}
```

**Example test:**
```csharp
public class GetTenantByIdQueryHandlerTests
{
    private readonly Mock<ITenantRepository> _tenantRepositoryMock;
    private readonly GetTenantByIdQueryHandler _handler;

    public GetTenantByIdQueryHandlerTests()
    {
        _tenantRepositoryMock = new Mock<ITenantRepository>();
        _handler = new GetTenantByIdQueryHandler(_tenantRepositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnTenant_WhenTenantExists()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var query = new GetTenantByIdQuery(tenantId);

        var tenant = Tenant.Create("Test Tenant");
        typeof(Tenant).GetProperty("Id")!.SetValue(tenant, tenantId);

        var tenants = new TestAsyncEnumerable<Tenant>(new List<Tenant> { tenant });

        _tenantRepositoryMock.Setup(x => x.AsNoTrackingQueryable())
            .Returns(tenants);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(tenantId);
        result.Name.Should().Be("Test Tenant");
    }
}
```

### Project Configuration

Use `$(MsPackageVersion)` for Microsoft packages (defined in `src/Directory.Build.props`):

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="$(MsPackageVersion)" />
```

**Solution File Format:**
- Project uses `.slnx` (XML-based solution format) instead of legacy `.sln`
- Add projects to solution using: `dotnet sln add <path-to-csproj>`
- All module projects MUST be added to `Cheetah.slnx` in `/Modules/{ModuleName}/` folder structure

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

**For Tenant-Based Modules (with separate database per tenant):**

12. In Application module: Register `IModuleConnectionStringProvider` with module name
13. In DataAccess module: Create and register `ITenantBasedDbContext<TenantCreatedEvent>` provider
14. Connection strings will be auto-generated on tenant creation
15. Databases will be auto-created and migrated via `TenantDatabaseMigrationManager`

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
15. **Project references MUST match module dependencies** - When adding a `<ProjectReference>` to project B from project A, you MUST add `[DependsOn(typeof(BModule))]` to AModule. Module dependency graph must mirror project reference graph.
16. **Tenant-based modules MUST register providers** - Modules with tenant-specific databases MUST register both `IModuleConnectionStringProvider` (in Application) and `ITenantBasedDbContext<TenantCreatedEvent>` (in DataAccess) for automatic database provisioning.
17. **Repository Pattern MANDATORY** - Application layer MUST NOT use DbContext directly. Use repositories for all data access.
18. **Specifications for queries** - Repositories MUST NOT build queries directly. Use Specification pattern for filtering logic. Only `AsQueryable()`/`AsNoTrackingQueryable()` methods allowed for complex projections in query handlers.

## 📚 Key Files

- Modularity: `src/Cheetah.Core/Modularity/CrmModule.cs`
- CQRS: `src/Cheetah.Core.CQRS/IDispatcher.cs`
- Events: `src/Cheetah.Core.Events/IEventBus.cs`
- Domain: `src/Cheetah.Core.Domain/AggregateRoot.cs`
- Specifications: `src/Cheetah.Core.Specification/Specification.cs`
- Tenant System: `src/Cheetah.Core.Tenants/`
  - `Services/IModuleConnectionStringProvider.cs` - Module database registration
  - `Services/ITenantMigrationService.cs` - Tenant info for migrations
- Tenant Database: `src/Cheetah.Core.EntityFramework.Tenants/`
  - `ITenantBasedDbContext.cs` - Tenant-specific DbContext interface
  - `Migrations/TenantDatabaseMigrationManager.cs` - Automatic multi-tenant migrations
  - `Configurations/TenantEntityConfiguration.cs` - Base configuration for tenant entities

## 🎯 Current Modules

1. **Cheetah.Tenants** - tenant management
2. **Cheetah.Features** - feature flags
3. **Cheetah.Permissions** - RBAC
4. **Cheetah.Identity** - users, JWT auth
