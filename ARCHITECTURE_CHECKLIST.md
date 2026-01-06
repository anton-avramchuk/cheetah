# Cheetah CRM - Architecture Compliance Checklist

**Quick reference for ensuring compliance with Claude.md guidelines**

---

## ✅ Module Creation Checklist

When creating a new module, follow this checklist:

### 1. Project Structure (11 Assemblies)

- [ ] `Cheetah.{ModuleName}.Events` - Domain events (NO dependencies except Core.Events)
- [ ] `Cheetah.{ModuleName}.Shared` - Constants, enums (depends ONLY on Core)
- [ ] `Cheetah.{ModuleName}.Contracts` - DTOs, ViewModels (depends on Core + Shared)
- [ ] `Cheetah.{ModuleName}.Domain` - Entities, value objects (depends on Events)
- [ ] `Cheetah.{ModuleName}.Application` - CQRS handlers (depends on Domain)
- [ ] `Cheetah.{ModuleName}.DataAccess` - EF Core, migrations (depends on Domain)
- [ ] `Cheetah.{ModuleName}.Api` - Minimal API (depends on Application + Contracts)
- [ ] `Cheetah.{ModuleName}.Frontend` - Blazor components (depends on Contracts)
- [ ] `Cheetah.{ModuleName}.Client` - Backend HTTP client (depends on Contracts)
- [ ] `Cheetah.{ModuleName}.Frontend.Client` - Blazor CQRS handlers (depends on Client)
- [ ] Test projects:
  - [ ] `Cheetah.{ModuleName}.Domain.Tests`
  - [ ] `Cheetah.{ModuleName}.Application.Tests`
  - [ ] `Cheetah.{ModuleName}.Client.Tests`
  - [ ] `Cheetah.{ModuleName}.Frontend.Client.Tests`

### 2. Add Projects to Solution

- [ ] All projects added to `Cheetah.slnx`
- [ ] Projects organized in `/Modules/{ModuleName}/` folder

---

## ✅ Events Project Checklist

- [ ] **NO dependencies** except `Cheetah.Core.Events`
- [ ] Events are pure data contracts (records)
- [ ] Events inherit from `EventBase` or implement `IEvent`
- [ ] Event naming: `{Entity}{Action}Event` (e.g., `TenantCreatedEvent`)

**Example:**
```csharp
namespace Cheetah.{ModuleName}.Events;

public record EntityCreatedEvent(
    Guid EntityId,
    string Name,
    DateTime OccurredAt
) : EventBase;
```

---

## ✅ Domain Project Checklist

### Entities

- [ ] Inherit from `Entity<TId>` or `AggregateRoot<TId>`
- [ ] **All properties have private setters**
- [ ] Private parameterless constructor for EF Core
- [ ] Static factory methods for creation
- [ ] Domain events raised using `AddDomainEvent()`
- [ ] No public setters, no direct state mutation

**Example:**
```csharp
public class Tenant : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public string Name { get; private set; } = null!; // ✅ Private setter
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    private Tenant() { } // ✅ For EF Core

    public static Tenant Create(string name) // ✅ Factory method
    {
        var tenant = new Tenant { Id = Guid.NewGuid(), Name = name };
        tenant.AddDomainEvent(new TenantCreatedEvent(tenant.Id, name)); // ✅ Domain event
        return tenant;
    }
}
```

### Module Class

- [ ] Inherits from `CrmModule`
- [ ] Marked as `partial`
- [ ] Has `[DependsOn]` attributes for dependencies
- [ ] Depends on Events project

**Example:**
```csharp
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmDomainModule))]
[DependsOn(typeof(Crm{ModuleName}EventsModule))]
public partial class Crm{ModuleName}DomainModule : CrmModule
{
}
```

---

## ✅ Application Project Checklist

### Commands

- [ ] Commands return `Guid` or `void`
- [ ] Record types preferred
- [ ] Implement `ICommand` or `ICommand<TResult>`

**Example:**
```csharp
public record CreateEntityCommand(string Name) : ICommand<Guid>;
```

### Command Handlers

- [ ] Implement `ICommandHandler<TCommand, TResult>`
- [ ] Use `[Export(LifetimeType.Scoped, typeof(...))]` attribute
- [ ] Return `ValueTask<TResult>` for hot paths
- [ ] Always accept `CancellationToken`
- [ ] Publish domain events AFTER `SaveChangesAsync()`
- [ ] Clear domain events after publishing

**Example:**
```csharp
[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateEntityCommand, Guid>))]
public class CreateEntityCommandHandler : ICommandHandler<CreateEntityCommand, Guid>
{
    private readonly IRepository<Entity, Guid> _repository;
    private readonly IEventBus _eventBus;

    public async ValueTask<Guid> HandleAsync(CreateEntityCommand cmd, CancellationToken ct)
    {
        var entity = Entity.Create(cmd.Name);
        await _repository.InsertAsync(entity, ct);

        foreach (var e in entity.DomainEvents)
            await _eventBus.PublishAsync(e, ct); // ✅ After save
        entity.ClearDomainEvents(); // ✅ Clear events

        return entity.Id;
    }
}
```

### Queries

- [ ] Queries return ViewModels (NOT domain entities)
- [ ] Implement `IQuery<TResult>`
- [ ] Query handlers use `AsNoTracking()` for read-only operations
- [ ] Use projection (`.Select()`) instead of returning full entities

**Example:**
```csharp
public record GetEntityByIdQuery(Guid Id) : IQuery<EntityViewModel?>;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetEntityByIdQuery, EntityViewModel?>))]
public class GetEntityByIdQueryHandler : IQueryHandler<GetEntityByIdQuery, EntityViewModel?>
{
    private readonly MyDbContext _db;

    public async ValueTask<EntityViewModel?> HandleAsync(GetEntityByIdQuery query, CancellationToken ct)
    {
        return await _db.Entities
            .Where(e => e.Id == query.Id)
            .Select(e => new EntityViewModel { ... }) // ✅ Projection
            .AsNoTracking() // ✅ Read-only
            .FirstOrDefaultAsync(ct);
    }
}
```

### Module Class

- [ ] Depends on Domain module
- [ ] Depends on `CrmCQRSCoreModule`
- [ ] Depends on `CrmEventsCoreModule`
- [ ] Calls `RegisterServices(context.Services)` in `ConfigureServices`

---

## ✅ DataAccess Project Checklist

### Module Class

- [ ] Depends on Domain module
- [ ] **MUST** depend on `CrmEntityFrameworkModule`
- [ ] **MUST** depend on `CrmEntityFrameworkPostgreSqlModule`
- [ ] Registers DbContext using `AddDbContext<T>()` or `AddTenantsDbContext<T>()`
- [ ] Configures PostgreSQL: `options.UseNpgsql<TDbContext>()`
- [ ] Registers database migrator: `AddDatabaseMigrator<TDbContext>()`

**Example:**
```csharp
[DependsOn(typeof(Crm{ModuleName}DomainModule))]
[DependsOn(typeof(CrmEntityFrameworkModule))]
[DependsOn(typeof(CrmEntityFrameworkPostgreSqlModule))]
public partial class Crm{ModuleName}DataAccessModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);

        context.Services.AddDbContext<MyDbContext>(options =>
            options.UseNpgsql(context.Services.GetConfiguration().GetConnectionString("MyModule")));

        Configure<CrmDbContextOptions>(options => 
            options.UseNpgsql<MyDbContext>());

        context.Services.AddDatabaseMigrator<MyDbContext>();
    }
}
```

### Entity Configurations

- [ ] Inherit from `IEntityTypeConfiguration<T>` or base classes
- [ ] **CRITICAL:** `builder.Ignore(t => t.DomainEvents);` MUST be present
- [ ] Table name specified
- [ ] All properties configured
- [ ] Indexes created where appropriate
- [ ] Relationships configured

**Example:**
```csharp
public class EntityConfiguration : IEntityTypeConfiguration<Entity>
{
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        builder.Ignore(e => e.DomainEvents); // ✅ CRITICAL!

        builder.ToTable("Entities");

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(e => e.Name)
            .IsUnique();
    }
}
```

---

## ✅ Api Project Checklist

### Module Class

- [ ] Depends on Application module
- [ ] Depends on Contracts module
- [ ] Depends on `CrmAspNetCoreModule`
- [ ] **MUST** depend on `CrmMapsterModule`
- [ ] Endpoints defined in `OnApplicationInitialization` (NOT in `ConfigureServices`)

### Endpoints

- [ ] **NO Controllers** - only Minimal API
- [ ] Use `routeBuilder.MapGet/MapPost/MapPut/MapDelete`
- [ ] **Always** use `[FromServices]`, `[FromBody]`, `[FromRoute]`, `[FromQuery]`
- [ ] Use `IDispatcher` to send commands/queries
- [ ] Use `IObjectMapper` for DTO ↔ Command/Query mapping
- [ ] Return ViewModels, NOT domain entities
- [ ] Use `.WithName()` and `.WithOpenApi()` for documentation

**Example:**
```csharp
public override void OnApplicationInitialization(ApplicationInitializationContext context)
{
    var routeBuilder = context.GetRouteBuilder();
    var mapper = context.ServiceProvider.GetRequiredService<IObjectMapper>();

    // GET /api/entities/{id}
    routeBuilder.MapGet("/api/entities/{id:guid}", async (
        [FromRoute] Guid id,
        [FromServices] IDispatcher dispatcher,
        CancellationToken ct) =>
    {
        var entity = await dispatcher.QueryAsync<GetEntityByIdQuery, EntityViewModel?>(
            new GetEntityByIdQuery(id), ct);

        return entity == null ? Results.NotFound() : Results.Ok(entity);
    })
    .WithName("GetEntityById")
    .Produces<EntityViewModel>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .WithOpenApi();

    // POST /api/entities
    routeBuilder.MapPost("/api/entities", async (
        [FromBody] CreateEntityRequest request,
        [FromServices] IDispatcher dispatcher,
        CancellationToken ct) =>
    {
        var command = mapper.Map<CreateEntityCommand>(request);
        var id = await dispatcher.SendAsync(command, ct);
        return Results.CreatedAtRoute("GetEntityById", new { id }, id);
    })
    .WithName("CreateEntity")
    .Produces<Guid>(StatusCodes.Status201Created)
    .WithOpenApi();
}
```

### Mapping Profile

- [ ] Implement `IMapsterMappingProfile`
- [ ] Register with `[Export(LifetimeType.Singleton, typeof(IMapsterMappingProfile))]`
- [ ] Configure Request → Command mappings
- [ ] Configure Entity → ViewModel mappings

**Example:**
```csharp
[Export(LifetimeType.Singleton, typeof(IMapsterMappingProfile))]
public class EntityMappingProfile : IMapsterMappingProfile
{
    public void Configure(TypeAdapterConfig config)
    {
        // Request to Command
        config.NewConfig<CreateEntityRequest, CreateEntityCommand>();

        // Entity to ViewModel
        config.NewConfig<Entity, EntityViewModel>();
    }
}
```

---

## ✅ Event Handling Checklist

### Publishing Events

- [ ] Events published **AFTER** `SaveChangesAsync()`
- [ ] Publish all domain events from entity
- [ ] Clear domain events after publishing

**Example:**
```csharp
await _repository.InsertAsync(entity, ct);

foreach (var domainEvent in entity.DomainEvents)
    await _eventBus.PublishAsync(domainEvent, ct);
    
entity.ClearDomainEvents();
```

### Subscribing to Events

- [ ] Only depend on **Events** project (NOT Domain or Application)
- [ ] Subscribe in `OnApplicationInitialization`
- [ ] Handler implements `IEventHandler<TEvent>`
- [ ] Handler registered with `[Export]` attribute

**Example:**
```csharp
// In module class
[DependsOn(typeof(CrmTenantsEventsModule))] // ✅ Only Events!
public class CrmFeaturesApplicationModule : CrmModule
{
    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var eventBus = context.ServiceProvider.GetRequiredService<IEventBus>();
        eventBus.Subscribe<TenantCreatedEvent, TenantCreatedEventHandler>();
    }
}

// Event handler
[Export(LifetimeType.Scoped, typeof(IEventHandler<TenantCreatedEvent>))]
public class TenantCreatedEventHandler : IEventHandler<TenantCreatedEvent>
{
    public async ValueTask HandleAsync(TenantCreatedEvent @event, CancellationToken ct)
    {
        // Handle event
    }
}
```

---

## ✅ Client Libraries Checklist

### Backend Client (`Client/`)

- [ ] Depends on Contracts module
- [ ] HTTP client for server-to-server communication
- [ ] Interface defines all operations
- [ ] Implementation uses `IHttpClientFactory`
- [ ] All methods return `ValueTask<T>`
- [ ] Accept `CancellationToken` parameter

**Example:**
```csharp
public interface IEntityClientService
{
    ValueTask<EntityViewModel?> GetByIdAsync(Guid id, CancellationToken ct = default);
    ValueTask<Guid> CreateAsync(CreateEntityRequest request, CancellationToken ct = default);
}

[Export(LifetimeType.Scoped, typeof(IEntityClientService))]
public class EntityClientService : IEntityClientService
{
    private readonly HttpClient _httpClient;

    public EntityClientService(IHttpClientFactory factory)
    {
        _httpClient = factory.CreateClient("CheetahAPI");
    }

    public async ValueTask<EntityViewModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync($"/api/entities/{id}", ct);
        return response.IsSuccessStatusCode 
            ? await response.Content.ReadFromJsonAsync<EntityViewModel>(ct)
            : null;
    }
}
```

### Frontend Client (`Frontend.Client/`)

- [ ] Depends on Client module
- [ ] Depends on `CrmFrontendCQRSModule`
- [ ] CQRS handlers for Blazor
- [ ] Handlers use Backend Client underneath
- [ ] All handlers registered with `[Export]`

**Example:**
```csharp
[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateEntityCommand, Guid>))]
public class CreateEntityCommandHandler : ICommandHandler<CreateEntityCommand, Guid>
{
    private readonly IEntityClientService _client;

    public CreateEntityCommandHandler(IEntityClientService client)
    {
        _client = client;
    }

    public async ValueTask<Guid> HandleAsync(CreateEntityCommand command, CancellationToken ct)
    {
        var request = new CreateEntityRequest { Name = command.Name };
        return await _client.CreateAsync(request, ct);
    }
}
```

### Tests

- [ ] **BOTH** Client and Frontend.Client MUST have test projects
- [ ] Unit tests for all client methods
- [ ] Mock HTTP responses
- [ ] Test happy paths and error cases
- [ ] >80% code coverage

---

## ✅ Tenant-Based Modules Checklist

For modules that need separate database per tenant:

### 1. Register Connection String Provider (Application)

- [ ] Register `IModuleConnectionStringProvider` in Application module
- [ ] Use `DefaultModuleConnectionStringProvider` or custom implementation

**Example:**
```csharp
public override void ConfigureServices(ServiceConfigurationContext context)
{
    RegisterServices(context.Services);

    context.Services.AddSingleton<IModuleConnectionStringProvider>(
        new DefaultModuleConnectionStringProvider("ModuleName"));
}
```

### 2. Register Tenant-Based DbContext Provider (DataAccess)

- [ ] Create provider implementing `ITenantBasedDbContext<TenantCreatedEvent>`
- [ ] Register with `[Export]` attribute
- [ ] `CreateForTenant` method creates DbContext with tenant connection string

**Example:**
```csharp
[Export(LifetimeType.Singleton, typeof(ITenantBasedDbContext<TenantCreatedEvent>))]
public class ModuleTenantDbContextProvider : ITenantBasedDbContext<TenantCreatedEvent>
{
    public string ModuleName => "ModuleName";

    public ITenantBasedDbContext<TenantCreatedEvent> CreateForTenant(string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MyDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        return new TenantMyDbContext(new MyDbContext(optionsBuilder.Options));
    }
}
```

### 3. Automatic Provisioning

- [ ] Databases automatically created on tenant creation
- [ ] Migrations run automatically via `TenantDatabaseMigrationManager`
- [ ] Connection strings generated automatically

---

## ✅ Naming Conventions Checklist

- [ ] **Entities:** `Tenant.cs` (singular)
- [ ] **Events:** `TenantCreatedEvent.cs` (Event suffix)
- [ ] **Commands:** `CreateTenantCommand.cs` (verb + entity + Command)
- [ ] **Queries:** `GetTenantByIdQuery.cs` (Get/List + Query)
- [ ] **ViewModels:** `TenantViewModel.cs` (ViewModel suffix)
- [ ] **Requests:** `CreateTenantRequest.cs` (Request suffix)
- [ ] **DbContext:** `TenantsDbContext.cs` (plural)
- [ ] **Modules:** `CrmTenantsApiModule.cs` (Crm + name + layer + Module)

---

## ✅ Performance Best Practices Checklist

- [ ] Use `AsNoTracking()` for read-only queries
- [ ] Use `ValueTask<T>` for hot paths (CQRS handlers)
- [ ] Avoid N+1 queries - use `Include()` or projection
- [ ] Use projection (`.Select()`) instead of full entities
- [ ] Always publish events AFTER `SaveChangesAsync()`
- [ ] Pass `CancellationToken` to all async methods

---

## ✅ Project Configuration Checklist

- [ ] Uses `$(MsPackageVersion)` for Microsoft packages
- [ ] Target framework: `net10.0`
- [ ] `ImplicitUsings`: enabled
- [ ] `Nullable`: enabled
- [ ] Source Generator reference for module registration

**Example .csproj:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="$(MsPackageVersion)" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\Cheetah.Generators.Module\Cheetah.Generators.Module.csproj" 
                      ReferenceOutputAssembly="false" OutputItemType="Analyzer" />
  </ItemGroup>
</Project>
```

---

## ✅ Module Dependencies Checklist

**CRITICAL RULE:** Project references MUST match module dependencies!

- [ ] For every `<ProjectReference>` to project B from project A
- [ ] There MUST be `[DependsOn(typeof(BModule))]` on AModule
- [ ] Module dependency graph mirrors project reference graph

**Example:**
```csharp
// If project has these references:
// <ProjectReference Include="..\Cheetah.Tenants.Application\..." />
// <ProjectReference Include="..\Cheetah.Mapping.Mapster\..." />

// Then module must have:
[DependsOn(typeof(CrmTenantsApplicationModule))]
[DependsOn(typeof(CrmMapsterModule))]
public partial class CrmTenantsApiModule : CrmModule
```

---

## 🚫 Critical Constraints (FORBIDDEN)

- 🚫 **NO Controllers** - Only Minimal API
- 🚫 **NO MediatR** - Use custom lightweight `IDispatcher`
- 🚫 **NO endpoints in ConfigureServices** - Only in `OnApplicationInitialization`
- 🚫 **NO cross-module dependencies** - Only via Events
- 🚫 **NO exposing Domain entities** - Only ViewModels
- 🚫 **NO missing `builder.Ignore(t => t.DomainEvents)`** in EF config
- 🚫 **NO forgetting to clear domain events** after publishing
- 🚫 **NO public setters** on entity properties
- 🚫 **NO direct DbContext usage in API** - Use CQRS handlers

---

## ✅ Final Verification

Before merging/deploying:

- [ ] All tests pass
- [ ] Code compiles without warnings
- [ ] No Controllers in the project
- [ ] All entity configurations have `Ignore(DomainEvents)`
- [ ] All modules have correct `[DependsOn]` attributes
- [ ] Client libraries have tests
- [ ] Database migrations are created and tested
- [ ] Event handlers are subscribed
- [ ] API endpoints return ViewModels (not entities)
- [ ] All async methods accept `CancellationToken`

---

**Last Updated:** 2026-01-06  
**Version:** 1.0  
**Based on:** Claude.md guidelines
