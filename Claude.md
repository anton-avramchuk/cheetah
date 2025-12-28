# Cheetah CRM Framework - Developer Guide

## 🎯 General Concept

Cheetah is a modular framework for building CRM systems on .NET 10.0 with event-driven architecture. Current implementation is a monolith with preparation for transition to microservices.

**Frontend:** Blazor WebAssembly (WASM) application for maximum interactivity and client-side performance.

## 🏗️ Architectural Principles

### 1. Modular System

**Core Concepts:**
- Project consists of independent modules
- Modules communicate via events (Redis Pub/Sub for backend, In-Memory for frontend)
- Each module has its own database
- Automatic service registration via Source Generators
- Topological sorting of module dependencies

**Base Module Class:**
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

**Lifecycle Hooks (in order):**
1. `PreConfigureServices` - before service registration
2. `ConfigureServices` - main registration
3. `PostConfigureServices` - after registration
4. `OnPreApplicationInitialization` - before app initialization
5. `OnApplicationInitialization` - main initialization
6. `OnPostApplicationInitialization` - after initialization

### 2. Large Module Structure

Each large module (Tenants, Features, Permissions, Identity) consists of 6 assemblies:

```
Cheetah.{ModuleName}/
├── Cheetah.{ModuleName}.Domain/        # Entities, Events, Value Objects
├── Cheetah.{ModuleName}.Application/   # CQRS, Services, Business Logic
├── Cheetah.{ModuleName}.DataAccess/    # EF Core, DbContext, Migrations
├── Cheetah.{ModuleName}.Api/           # Controllers, Endpoints
├── Cheetah.{ModuleName}.Shared/        # DTOs, ViewModels (shared with frontend)
└── Cheetah.{ModuleName}.Frontend/      # Blazor WASM components
```

**Layer Dependencies:**
```
Api → Application → Domain
DataAccess → Domain
Shared (independent)
Frontend → Shared (Blazor WASM uses Shared DTOs)
```

### 3. Domain Layer (DDD)

**Base Classes:**
- `Entity<TId>` - entity with identifier
- `AggregateRoot<TId>` - aggregate root (with domain events)
- `ValueObject` - value object (immutable)

**Audit Interfaces:**
- `ICreateAtEntity` - automatically sets CreatedAt
- `IUpdatedAtEntity` - automatically sets UpdatedAt
- `IRemovedAtEntity` - for soft delete (RemovedAt)

**Entity Example:**
```csharp
public class Tenant : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public string Name { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    private Tenant() { } // For EF Core

    public static Tenant Create(string name)
    {
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = name
        };
        tenant.AddDomainEvent(new TenantCreatedEvent(tenant.Id, name));
        return tenant;
    }

    public void Activate()
    {
        IsActive = true;
        AddDomainEvent(new TenantActivatedEvent(Id));
    }
}
```

**Important:** Always use `private set` for properties and methods for state changes with event generation.

### 4. Application Layer (CQRS)

**Commands (change state):**
```csharp
// Command
public record CreateTenantCommand(string Name, string? Subdomain) : ICommand<Guid>;

// Handler
[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateTenantCommand, Guid>))]
public class CreateTenantCommandHandler : ICommandHandler<CreateTenantCommand, Guid>
{
    private readonly MyDbContext _dbContext;
    private readonly IEventBus _eventBus;

    public async ValueTask<Guid> HandleAsync(CreateTenantCommand command, CancellationToken ct)
    {
        var tenant = Tenant.Create(command.Name);
        _dbContext.Tenants.Add(tenant);
        await _dbContext.SaveChangesAsync(ct);

        // Publish domain events
        foreach (var domainEvent in tenant.DomainEvents)
        {
            await _eventBus.PublishAsync(domainEvent, ct);
        }
        tenant.ClearDomainEvents();

        return tenant.Id;
    }
}
```

**Queries (read data):**
```csharp
// Query
public record GetTenantByIdQuery(Guid Id) : IQuery<TenantViewModel?>;

// Handler
[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetTenantByIdQuery, TenantViewModel?>))]
public class GetTenantByIdQueryHandler : IQueryHandler<GetTenantByIdQuery, TenantViewModel?>
{
    private readonly MyDbContext _dbContext;

    public async ValueTask<TenantViewModel?> HandleAsync(GetTenantByIdQuery query, CancellationToken ct)
    {
        return await _dbContext.Tenants
            .Where(t => t.Id == query.Id)
            .Select(t => new TenantViewModel
            {
                Id = t.Id,
                Name = t.Name
            })
            .FirstOrDefaultAsync(ct);
    }
}
```

**Important:**
- Commands return Guid or void
- Queries return ViewModels (from Shared project)
- Use `ValueTask<T>` instead of `Task<T>` for hot paths
- Always pass CancellationToken

### 5. DataAccess Layer (EF Core)

**DbContext:**
```csharp
public class TenantsDbContext : CrmDbContext
{
    public DbSet<Tenant> Tenants => Set<Tenant>();

    public TenantsDbContext(DbContextOptions<TenantsDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TenantsDbContext).Assembly);
    }
}
```

**Entity Configuration:**
```csharp
public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(t => t.NormalizedName)
            .IsUnique();

        // Always ignore DomainEvents
        builder.Ignore(t => t.DomainEvents);
    }
}
```

**Module Registration:**
```csharp
[DependsOn(typeof(MyDomainModule))]
[DependsOn(typeof(CrmEntityFrameworkModule))]
public partial class MyDataAccessModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);

        context.Services.AddDbContext<MyDbContext>(options =>
        {
            var connectionString = context.Services.GetConfiguration()
                .GetConnectionString("MyModule");
            options.UseSqlServer(connectionString);
        });
    }
}
```

**Important:**
- Each module has its own DB and DbContext
- ConnectionString is taken from configuration with module name
- Always use `builder.Ignore(t => t.DomainEvents)`

### 6. Api Layer

**Controller:**
```csharp
[ApiController]
[Route("api/[controller]")]
public class TenantsController : ControllerBase
{
    private readonly IDispatcher _dispatcher;

    public TenantsController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [HttpPost]
    public async Task<ActionResult<TenantViewModel>> Create(
        [FromBody] CreateTenantRequest request,
        CancellationToken ct)
    {
        var command = new CreateTenantCommand(request.Name, request.Subdomain);
        var id = await _dispatcher.SendAsync(command, ct);

        var query = new GetTenantByIdQuery(id);
        var viewModel = await _dispatcher.SendAsync(query, ct);

        return CreatedAtAction(nameof(GetById), new { id }, viewModel);
    }
}
```

**Important:**
- API works only with Dispatcher (mediator)
- Use Request/Response from Shared project
- Always return ViewModels, not Domain entities

### 7. Shared Layer

**ViewModels (for returning data):**
```csharp
public class TenantViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public bool IsActive { get; set; }
}
```

**Request DTOs (for receiving data):**
```csharp
public class CreateTenantRequest
{
    public string Name { get; set; } = null!;
    public string? Subdomain { get; set; }
}
```

**Important:** Shared project should have no dependencies (pure DTOs). Used by both Backend API and Blazor WASM frontend.

### 8. Frontend Layer (Blazor WASM)

**Blazor components use:**
- `Cheetah.Frontend.CQRS` - client-side Dispatcher
- `Cheetah.Frontend.Events` - in-memory Event Bus
- `Cheetah.{Module}.Shared` - ViewModels and Request DTOs

**Component Example:**
```razor
@page "/tenants"
@using Cheetah.Tenants.Shared.ViewModels
@inject IDispatcher Dispatcher

<h3>Tenants</h3>

@if (tenants == null)
{
    <p>Loading...</p>
}
else
{
    <table class="table">
        <thead>
            <tr>
                <th>Name</th>
                <th>Status</th>
            </tr>
        </thead>
        <tbody>
            @foreach (var tenant in tenants)
            {
                <tr>
                    <td>@tenant.Name</td>
                    <td>@(tenant.IsActive ? "Active" : "Inactive")</td>
                </tr>
            }
        </tbody>
    </table>
}

@code {
    private List<TenantViewModel>? tenants;

    protected override async Task OnInitializedAsync()
    {
        var query = new GetAllTenantsQuery();
        tenants = await Dispatcher.SendAsync(query);
    }
}
```

**Important:**
- Blazor WASM works via HTTP API (not directly with DB)
- Uses the same CQRS pattern as backend
- Frontend Dispatcher makes HTTP requests to API

### 9. Event-Driven Architecture

**Event Definition (in Domain):**
```csharp
public record TenantCreatedEvent(
    Guid TenantId,
    string Name,
    string? Subdomain
) : EventBase;
```

**Publishing Event (in Application):**
```csharp
// After saving to DB
foreach (var domainEvent in tenant.DomainEvents)
{
    await _eventBus.PublishAsync(domainEvent, ct);
}
tenant.ClearDomainEvents();
```

**Handling Event (in another module):**
```csharp
[Export(LifetimeType.Scoped, typeof(TenantCreatedEventHandler))]
public class TenantCreatedEventHandler : IEventHandler<TenantCreatedEvent>
{
    public async ValueTask HandleAsync(TenantCreatedEvent @event, CancellationToken ct)
    {
        // Create default features for new tenant
        await _featureManager.EnableAsync(@event.TenantId, "Users.Create", ct);
    }
}
```

**Event Subscription (in module):**
```csharp
public class MyModule : CrmModule
{
    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var eventBus = context.ServiceProvider.GetRequiredService<IEventBus>();
        eventBus.Subscribe<TenantCreatedEvent, TenantCreatedEventHandler>();
    }
}
```

**Important:**
- Events are `record` with `Event` suffix
- Inherit from `EventBase`
- Used for inter-module communication
- Backend uses Redis Pub/Sub, Frontend uses In-Memory

### 10. Dependency Injection

**Automatic Registration:**
```csharp
[Export(LifetimeType.Singleton, typeof(IMyService))]
public class MyService : IMyService
{
    // Automatically registered as Singleton
}

[Export(LifetimeType.Scoped, typeof(IMyRepository))]
public class MyRepository : IMyRepository
{
    // Automatically registered as Scoped
}
```

**Lifetime Types:**
- `LifetimeType.Singleton` - one instance per application
- `LifetimeType.Scoped` - one instance per request
- `LifetimeType.Transient` - new instance every time

**Source Generator:**
Automatically generates `RegisterServices()` method in partial module class.

### 11. Multi-tenancy

**Principles:**
- Tenant per Database - each tenant has its own DB
- Master DB (Tenants) stores information about all tenants
- Tenant resolving via subdomain or header

**Tenant Resolving:**
```csharp
// Middleware checks:
// 1. Header "X-Tenant-Id" or "X-Tenant-Subdomain"
// 2. Subdomain from Host (tenant1.mycrm.com -> tenant1)

public interface ICurrentTenant
{
    Guid? Id { get; }
    string? Name { get; }
    bool IsAvailable { get; }
}
```

**Getting Connection String for Tenant:**
```csharp
public interface ITenantStore
{
    Task<string?> GetConnectionStringAsync(Guid tenantId, string name = "Default", CancellationToken ct = default);
}
```

## 📋 Naming Conventions

### Files and Classes
- **Entities:** `Tenant.cs`, `User.cs` (singular, PascalCase)
- **Events:** `TenantCreatedEvent.cs` (Event suffix)
- **Commands:** `CreateTenantCommand.cs` (verb + entity + Command)
- **Queries:** `GetTenantByIdQuery.cs` (Get/List + criteria + Query)
- **Handlers:** `CreateTenantCommandHandler.cs` (command/query name + Handler)
- **ViewModels:** `TenantViewModel.cs` (ViewModel suffix)
- **Requests:** `CreateTenantRequest.cs` (verb + entity + Request)
- **DbContext:** `TenantsDbContext.cs` (plural + DbContext)
- **Modules:** `CrmTenantsApiModule.cs` (Crm prefix + module name + layer + Module)

### Folder Structure
```
Cheetah.Tenants.Domain/
├── Entities/           # Tenant.cs, TenantConnectionString.cs
├── Events/             # TenantCreatedEvent.cs
├── ValueObjects/       # Email.cs, Address.cs
└── CrmTenantsDomainModule.cs

Cheetah.Tenants.Application/
├── Commands/           # CreateTenantCommand.cs, CreateTenantCommandHandler.cs
├── Queries/            # GetTenantByIdQuery.cs, GetTenantByIdQueryHandler.cs
├── Services/           # ITenantResolver.cs, TenantResolver.cs
└── CrmTenantsApplicationModule.cs
```

## 🔧 Technology Stack

**Backend:**
- .NET 10.0
- ASP.NET Core Web API
- Entity Framework Core
- StackExchange.Redis
- Mapster (object mapping)

**Frontend:**
- Blazor WebAssembly (WASM)
- HttpClient for API requests
- Shared DTOs between frontend and backend

**Testing:**
- xUnit
- FluentAssertions
- Moq

**Databases:**
- SQL Server (primary)
- MySQL
- PostgreSQL

## ⚡ Best Practices

### 1. Working with Events
```csharp
// ✅ CORRECT: Publish after saving
await _dbContext.SaveChangesAsync(ct);
foreach (var e in entity.DomainEvents)
    await _eventBus.PublishAsync(e, ct);
entity.ClearDomainEvents();

// ❌ WRONG: Publish before saving
await _eventBus.PublishAsync(new SomeEvent());
await _dbContext.SaveChangesAsync(ct);
```

### 2. Return Types
```csharp
// ✅ CORRECT: ValueTask for hot paths (CQRS handlers)
public ValueTask<User?> HandleAsync(GetUserQuery query, CancellationToken ct);

// ✅ CORRECT: Task for rare operations (initialization)
public Task InitializeAsync();

// ❌ WRONG: Task for CQRS handlers
public Task<User> HandleAsync(...); // Should be ValueTask
```

### 3. Domain Methods
```csharp
// ✅ CORRECT: Factory method + private set
public class Tenant : AggregateRoot<Guid>
{
    public string Name { get; private set; }

    private Tenant() { }

    public static Tenant Create(string name)
    {
        var tenant = new Tenant { Name = name };
        tenant.AddDomainEvent(new TenantCreatedEvent(...));
        return tenant;
    }
}

// ❌ WRONG: Public set
public string Name { get; set; } // Breaks encapsulation
```

### 4. Permission Checks
```csharp
// ✅ CORRECT: At the beginning of handler
public async ValueTask HandleAsync(DeleteUserCommand cmd, CancellationToken ct)
{
    await _permissionChecker.RequireAsync("Users.Delete", ct);
    // Logic follows
}

// ❌ WRONG: Check in controller
[Authorize(Policy = "Users.Delete")] // Don't do this, use IPermissionChecker
```

### 5. EF Core Configuration
```csharp
// ✅ CORRECT: IEntityTypeConfiguration
public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");
        builder.Ignore(t => t.DomainEvents); // Important!
    }
}

// ❌ WRONG: Fluent API in OnModelCreating
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Tenant>().HasKey(...); // Bad
}
```

### 6. Blazor WASM Communication
```csharp
// ✅ CORRECT: Via CQRS Dispatcher (which makes HTTP calls)
var query = new GetTenantsQuery();
var tenants = await _dispatcher.SendAsync(query);

// ❌ WRONG: Direct HTTP request in component
var response = await _httpClient.GetAsync("/api/tenants");
```

## 🚀 Common Tasks

### Creating a New Module
1. Create folder structure (Domain, Application, DataAccess, Api, Shared, Frontend)
2. Create Module classes with `[DependsOn]` attributes
3. Define Domain models in Domain project
4. Create DbContext in DataAccess
5. Implement Commands/Queries in Application
6. Create Controllers in Api
7. Add ViewModels to Shared
8. Create Blazor components in Frontend
9. Add all projects to Solution

### Adding a New Feature
1. Define Domain Event (if needed)
2. Create Command/Query in Application
3. Create Handler in Application
4. Create Endpoint in Api
5. Create ViewModel in Shared
6. Publish event after execution
7. Create Blazor component in Frontend
8. Write tests

### Subscribing to Event from Another Module
1. Create EventHandler in Application
2. Mark with `[Export]` attribute
3. Subscribe in module's `OnApplicationInitialization`
4. Handle event asynchronously

## 📚 Key Files for Understanding

**Modularity:**
- `src/Cheetah.Core/Modularity/CrmModule.cs`
- `src/Cheetah.Core/Modularity/ModuleManager.cs`
- `src/Cheetah.Generators.Module/BootstrapperGenerator.cs`

**CQRS:**
- `src/Cheetah.Core.CQRS/IDispatcher.cs`
- `src/Cheetah.Backend.CQRS/Dispatcher.cs` (for API)
- `src/Cheetah.Frontend.CQRS/Dispatcher.cs` (for Blazor WASM)

**Events:**
- `src/Cheetah.Core.Events/IEventBus.cs`
- `src/Cheetah.Backend.Events.Redis/CrmRedisEventBus.cs` (Redis Pub/Sub)
- `src/Cheetah.Frontend.Events/CrmInMemoryEventBus.cs` (In-Memory for Blazor)

**Domain:**
- `src/Cheetah.Core.Domain/Entity.cs`
- `src/Cheetah.Core.Domain/AggregateRoot.cs`

**Multi-tenancy:**
- `src/Cheetah.Core.Tenants/Domain/Tenant.cs`

## ⚠️ Important Constraints

1. **Don't use direct references between modules** - only via events
2. **Don't expose Domain entities** - only ViewModels from Shared
3. **Don't forget ClearDomainEvents()** after publishing events
4. **Don't use Task.Run** in handlers - blocks event loop
5. **Always use CancellationToken** for async operations
6. **Don't use static for services** - only via DI
7. **Module classes are always partial** - for Source Generators to work
8. **Don't skip Entity.DomainEvents ignore in EF config** - will cause errors
9. **Blazor WASM cannot directly work with DB** - only via API

## 🎯 Current Development Focus

Creating 4 base modules:
1. **Cheetah.Tenants** - tenant management, resolving
2. **Cheetah.Features** - feature flags for tenants
3. **Cheetah.Permissions** - RBAC permission system
4. **Cheetah.Identity** - users, authentication (JWT)

Each module follows the structure: Domain → Application → DataAccess → Api → Shared → Frontend.
