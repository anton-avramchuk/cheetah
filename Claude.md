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
public class MyEntity : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public string Name { get; private set; } = null!; // Always private set
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    private MyEntity() { } // For EF Core

    public static MyEntity Create(string name)
    {
        var entity = new MyEntity { Id = Guid.NewGuid(), Name = name };
        entity.AddDomainEvent(new MyEntityCreatedEvent(entity.Id, name));
        return entity;
    }
}
```

### Application Layer (CQRS)

**Commands** (state changes) return `Guid` or `void`. **Queries** return ViewModels.
Use `ValueTask<T>` for hot paths, always pass `CancellationToken`.

**CRITICAL: Application layer MUST NOT use DbContext directly. Use Repository pattern with Specifications.**

```csharp
public record CreateMyEntityCommand(string Name) : ICommand<Guid>;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateMyEntityCommand, Guid>))]
public class CreateMyEntityCommandHandler : ICommandHandler<CreateMyEntityCommand, Guid>
{
    private readonly IMyEntityRepository _repository;
    private readonly IEventBus _eventBus;

    public async ValueTask<Guid> HandleAsync(CreateMyEntityCommand cmd, CancellationToken ct)
    {
        var entity = MyEntity.Create(cmd.Name);
        _repository.Add(entity);
        await _repository.SaveChangesAsync(ct);

        foreach (var e in entity.DomainEvents)
            await _eventBus.PublishAsync(e, ct);
        entity.ClearDomainEvents();

        return entity.Id;
    }
}
```

### Repository Pattern & Specifications

**Application layer uses repositories, NOT DbContext directly.**

**Repository Interface (Domain layer):**
```csharp
public interface IMyEntityRepository
{
    ValueTask<MyEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);
    ValueTask<MyEntity?> GetBySpecAsync(ISpecification<MyEntity> spec, CancellationToken ct = default);
    ValueTask<List<MyEntity>> GetAllAsync(ISpecification<MyEntity>? spec = null, CancellationToken ct = default);
    ValueTask<bool> ExistsAsync(ISpecification<MyEntity> spec, CancellationToken ct = default);
    void Add(MyEntity entity);
    void Update(MyEntity entity);
    void Delete(MyEntity entity);
    ValueTask<int> SaveChangesAsync(CancellationToken ct = default);
    IQueryable<MyEntity> AsQueryable();
    IQueryable<MyEntity> AsNoTrackingQueryable();
}
```

**Specification (Domain layer):**
```csharp
public class MyEntityByNameSpecification : Specification<MyEntity>
{
    private readonly string _name;

    public MyEntityByNameSpecification(string name) => _name = name;

    public override Expression<Func<MyEntity, bool>> ToExpression()
        => entity => entity.Name == _name;
}
```

**Repository Implementation (DataAccess layer):**
```csharp
[Export(LifetimeType.Scoped, typeof(IMyEntityRepository))]
public class MyEntityRepository : IMyEntityRepository
{
    private readonly MyDbContext _context;

    public MyEntityRepository(MyDbContext context) => _context = context;

    public async ValueTask<MyEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.MyEntities.FindAsync(new object[] { id }, ct);

    public async ValueTask<MyEntity?> GetBySpecAsync(ISpecification<MyEntity> spec, CancellationToken ct = default)
        => await _context.MyEntities.Where(spec.ToExpression()).FirstOrDefaultAsync(ct);

    public IQueryable<MyEntity> AsNoTrackingQueryable()
        => _context.MyEntities.AsNoTracking();

    // Other methods...
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

public class MyEntityConfiguration : IEntityTypeConfiguration<MyEntity>
{
    public void Configure(EntityTypeBuilder<MyEntity> builder)
    {
        builder.ToTable("MyEntities");
        builder.Ignore(e => e.DomainEvents); // CRITICAL!
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

        routeBuilder.MapPost("/api/my-entities", async (
            [FromBody] CreateMyEntityRequest request,
            [FromServices] IDispatcher dispatcher,
            CancellationToken ct) =>
        {
            var command = mapper.Map<CreateMyEntityCommand>(request);
            var id = await dispatcher.SendAsync(command, ct);
            return Results.Created($"/api/my-entities/{id}", id);
        })
        .WithName("CreateMyEntity")
        .WithOpenApi();
    }
}
```

### Events (Separate Project)

Events in `Cheetah.{ModuleName}.Events` - pure data contracts, NO dependencies.

```csharp
namespace Cheetah.MyModule.Events;

public record MyEntityCreatedEvent(Guid EntityId, string Name) : EventBase;
```

**Event Subscription:**
```csharp
[DependsOn(typeof(CrmMyModuleEventsModule))] // Only depend on Events!
public class CrmOtherModuleApplicationModule : CrmModule
{
    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var eventBus = context.ServiceProvider.GetRequiredService<IEventBus>();
        eventBus.Subscribe<MyEntityCreatedEvent, MyEntityCreatedEventHandler>();
    }
}
```

### Client Libraries

**Backend Client** (`Client/`): HTTP client for server-to-server integration.

```csharp
public interface IMyModuleClient
{
    ValueTask<MyEntityViewModel?> GetByIdAsync(Guid id, CancellationToken ct = default);
    ValueTask<Guid> CreateAsync(CreateMyEntityRequest request, CancellationToken ct = default);
}

[Export(LifetimeType.Scoped, typeof(IMyModuleClient))]
public class MyModuleClient : IMyModuleClient
{
    private readonly HttpClient _httpClient;

    public MyModuleClient(IHttpClientFactory factory) =>
        _httpClient = factory.CreateClient("CheetahAPI");
}
```

**Frontend Client** (`Frontend.Client/`): CQRS handlers using Backend Client for Blazor.

```csharp
[DependsOn(typeof(CrmMyModuleClientModule))]
[DependsOn(typeof(CrmFrontendCQRSModule))]
public partial class CrmMyModuleFrontendClientModule : CrmModule { }
```

**Both MUST have comprehensive tests.**

### Shared and Contracts Libraries

**Shared Library** (`{ModuleName}.Shared/`):
- Constants, enums, shared configuration values
- Dependencies: ONLY `Cheetah.Core`

**Contracts Library** (`{ModuleName}.Contracts/`):
- DTOs (ViewModels, Requests, Responses)
- Dependencies: `Cheetah.Core` + `{ModuleName}.Shared`

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
- **Entities:** `MyEntity.cs` (singular)
- **Events:** `MyEntityCreatedEvent.cs` (Event suffix)
- **Commands:** `CreateMyEntityCommand.cs` (verb + entity + Command)
- **Queries:** `GetMyEntityByIdQuery.cs` (Get/List + Query)
- **ViewModels:** `MyEntityViewModel.cs` (ViewModel suffix)
- **DbContext:** `MyEntitiesDbContext.cs` (plural)
- **Modules:** `CrmMyModuleApiModule.cs` (Crm + name + layer + Module)

### Folder Structure
```
Cheetah.MyModule.Application/
├── Commands/        # CreateMyEntityCommand.cs + Handler
├── Queries/         # GetMyEntityByIdQuery.cs + Handler
├── Services/        # IMyService.cs
├── EventHandlers/   # Handlers for OTHER modules' events
└── CrmMyModuleApplicationModule.cs
```

### Project Configuration

**Central Package Management:** Project uses `Directory.Packages.props` at solution root for centralized package versioning.
- When adding a NuGet package, add `<PackageVersion>` to `Directory.Packages.props` if not already present
- In `.csproj` files use `<PackageReference Include="PackageName" />` **without** `Version` attribute
- Do NOT use `$(MsPackageVersion)` — all versions are managed centrally

```xml
<!-- Directory.Packages.props (root) -->
<PackageVersion Include="Microsoft.EntityFrameworkCore" Version="10.0.3" />

<!-- .csproj (no version) -->
<PackageReference Include="Microsoft.EntityFrameworkCore" />
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
6. Shared (constants)
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
11. **Always `builder.Ignore(e => e.DomainEvents)`** in EF config
12. **Module classes are `partial`** - for Source Generators
13. **Always use `[FromServices]`, `[FromRoute]`, `[FromBody]`, `[FromQuery]`**
14. **Blazor WASM via API only** - through Client libraries
15. **Project references MUST match module dependencies**
16. **Repository Pattern MANDATORY** - Application layer MUST NOT use DbContext directly
17. **Specifications for queries** - Use Specification pattern for filtering logic

## 📚 Key Files

- Modularity: `src/Cheetah.Core/Modularity/CrmModule.cs`
- CQRS: `src/Cheetah.Core.CQRS/IDispatcher.cs`
- Events: `src/Cheetah.Core.Events/IEventBus.cs`
- Domain: `src/Cheetah.Core.Domain/AggregateRoot.cs`
- Specifications: `src/Cheetah.Core.Specification/Specification.cs`

## 🎯 Current Modules

1. **Cheetah.Features** - feature flags
2. **Cheetah.Permissions** - RBAC
3. **Cheetah.Identity** - users, JWT auth
