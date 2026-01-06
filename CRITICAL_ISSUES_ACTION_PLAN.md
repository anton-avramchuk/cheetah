# Cheetah CRM - Critical Issues Action Plan

**Date:** 2026-01-06  
**Priority:** 🔴 HIGH  
**Estimated Total Effort:** 10-18 hours

---

## Issue #1: Missing `DomainEvents` Ignore in EF Configurations

### **Severity:** 🔴 CRITICAL
### **Estimated Effort:** 1-2 hours
### **Risk:** High - Can cause runtime errors and database schema issues

### Problem
According to `Claude.md` (Line 128), **ALL** entity configurations must include:
```csharp
builder.Ignore(t => t.DomainEvents); // CRITICAL!
```

This is currently **MISSING** from all entity configurations across all modules.

### Why It's Critical
- `DomainEvents` is an in-memory collection used for event sourcing
- EF Core should **never** attempt to persist this collection
- Without `Ignore()`, EF Core may:
  - Try to create a database table for domain events
  - Cause migration errors
  - Throw runtime exceptions during SaveChanges
  - Corrupt the domain model

### Affected Files

#### Tenants Module
- `src/Cheetah.Tenants/Cheetah.Tenants.DataAccess/Configurations/TenantConfiguration.cs`
- `src/Cheetah.Tenants/Cheetah.Tenants.DataAccess/Configurations/TenantConnectionStringConfiguration.cs`

#### Identity Module (Needs Verification)
- `src/Cheetah.Identity/Cheetah.Identity.DataAccess/Configurations/UserConfiguration.cs`
- `src/Cheetah.Identity/Cheetah.Identity.DataAccess/Configurations/RoleConfiguration.cs`
- `src/Cheetah.Identity/Cheetah.Identity.DataAccess/Configurations/UserClaimConfiguration.cs`
- `src/Cheetah.Identity/Cheetah.Identity.DataAccess/Configurations/RoleClaimConfiguration.cs`
- `src/Cheetah.Identity/Cheetah.Identity.DataAccess/Configurations/UserRoleConfiguration.cs`

#### Features Module (Needs Verification)
- All entity configurations in `src/Cheetah.Features/Cheetah.Features.DataAccess/Configurations/`

### Fix Template

#### For Simple Entities (No Base Configuration Class)
```csharp
public class MyEntityConfiguration : IEntityTypeConfiguration<MyEntity>
{
    public void Configure(EntityTypeBuilder<MyEntity> builder)
    {
        // ✅ ADD THIS FIRST!
        builder.Ignore(e => e.DomainEvents);
        
        // Table configuration
        builder.ToTable("MyEntities");
        
        // ... rest of configuration
    }
}
```

#### For Entities Using Base Configuration
If the entity uses a base configuration class (like `TenantEntityConfiguration`), check if the base class already handles this. If not, add it in the override:

```csharp
public class TenantConfiguration : TenantEntityConfiguration<Tenant, ...>
{
    protected override void ConfigureAdditionalProperties(EntityTypeBuilder<Tenant> builder)
    {
        // ✅ ADD THIS FIRST!
        builder.Ignore(t => t.DomainEvents);
        
        // NormalizedName configuration
        builder.Property(t => t.NormalizedName)
            .IsRequired()
            .HasMaxLength(TenantConstants.MaxNormalizedNameLength);
            
        // ... rest of configuration
    }
}
```

### Step-by-Step Action Plan

1. **Identify all entity classes that have `DomainEvents` property**
   ```bash
   # Search for classes inheriting from Entity<T> or AggregateRoot<T>
   grep -r "AggregateRoot<" src/ --include="*.cs"
   grep -r "Entity<" src/ --include="*.cs"
   grep -r ": Entity" src/ --include="*.cs"
   ```

2. **Find their corresponding EF configurations**
   ```bash
   # Search for IEntityTypeConfiguration implementations
   grep -r "IEntityTypeConfiguration<" src/ --include="*.cs"
   ```

3. **For each configuration, add `builder.Ignore(e => e.DomainEvents);`**
   - Place it at the **beginning** of the `Configure` method
   - Or in `ConfigureAdditionalProperties` if using base class

4. **Verify by searching for the Ignore statement**
   ```bash
   grep -r "Ignore.*DomainEvents" src/ --include="*.cs"
   ```

5. **Create and run a new migration** (if schema changes are detected)
   ```bash
   # For each module with changes
   cd src/Cheetah.Tenants/Cheetah.Tenants.DataAccess
   dotnet ef migrations add IgnoreDomainEvents
   ```

6. **Test the changes**
   - Run unit tests for domain entities
   - Verify SaveChanges operations work correctly
   - Ensure no extra tables are created

### Verification Checklist
- [ ] All entities with `DomainEvents` have corresponding `Ignore()` in EF config
- [ ] No migration errors when running `dotnet ef migrations add`
- [ ] SaveChanges operations work without exceptions
- [ ] No `DomainEvents` table created in database
- [ ] Domain events are still being raised and published correctly

---

## Issue #2: Missing Test Projects for Client Libraries

### **Severity:** 🔴 CRITICAL
### **Estimated Effort:** 8-16 hours (including test implementation)
### **Risk:** Medium - Affects code quality and maintainability

### Problem
According to `Claude.md` (Line 214): **"Both MUST have comprehensive tests."**

Currently **MISSING**:
- ❌ `Cheetah.Identity.Client.Tests`
- ❌ `Cheetah.Identity.Frontend.Client.Tests`
- ❌ `Cheetah.Features.Client.Tests`
- ❌ `Cheetah.Features.Frontend.Client.Tests`

### Why It's Critical
- Client libraries are the **public API** for modules
- Breaking changes in clients affect all consumers
- HTTP clients require mocking and integration testing
- Frontend clients need testing for proper CQRS handling

### Required Test Projects

#### Identity Module
```
src/Cheetah.Identity/Tests/
├── Cheetah.Identity.Client.Tests/
│   ├── Cheetah.Identity.Client.Tests.csproj
│   ├── UserClientServiceTests.cs
│   ├── RoleClientServiceTests.cs
│   └── AuthClientServiceTests.cs
│
└── Cheetah.Identity.Frontend.Client.Tests/
    ├── Cheetah.Identity.Frontend.Client.Tests.csproj
    ├── Commands/
    │   ├── CreateUserCommandHandlerTests.cs
    │   ├── UpdateUserCommandHandlerTests.cs
    │   └── DeleteUserCommandHandlerTests.cs
    └── Queries/
        ├── GetUserByIdQueryHandlerTests.cs
        └── GetAllUsersQueryHandlerTests.cs
```

#### Features Module
```
src/Cheetah.Features/Tests/
├── Cheetah.Features.Client.Tests/
│   ├── Cheetah.Features.Client.Tests.csproj
│   └── FeatureClientServiceTests.cs
│
└── Cheetah.Features.Frontend.Client.Tests/
    ├── Cheetah.Features.Frontend.Client.Tests.csproj
    ├── Commands/
    │   └── ...
    └── Queries/
        └── ...
```

### Project Template (.csproj)

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="$(MsPackageVersion)" />
    <PackageReference Include="xunit" Version="2.9.*" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.*" />
    <PackageReference Include="FluentAssertions" Version="7.0.*" />
    <PackageReference Include="Moq" Version="4.20.*" />
  </ItemGroup>

  <ItemGroup>
    <!-- For Client.Tests -->
    <ProjectReference Include="..\Cheetah.{ModuleName}.Client\Cheetah.{ModuleName}.Client.csproj" />
    
    <!-- For Frontend.Client.Tests -->
    <ProjectReference Include="..\Cheetah.{ModuleName}.Frontend.Client\Cheetah.{ModuleName}.Frontend.Client.csproj" />
  </ItemGroup>

</Project>
```

### Test Example: Client.Tests

```csharp
using Cheetah.Identity.Client.Interfaces;
using Cheetah.Identity.Contracts.ViewModels;
using FluentAssertions;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Cheetah.Identity.Client.Tests;

public class UserClientServiceTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly IUserClientService _userClient;

    public UserClientServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:5000")
        };
        
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock
            .Setup(f => f.CreateClient("CheetahAPI"))
            .Returns(_httpClient);
            
        _userClient = new UserClientService(httpClientFactoryMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingUser_ReturnsViewModel()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedUser = new UserViewModel
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.PathAndQuery == $"/api/users/{userId}"),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedUser)
            });

        // Act
        var result = await _userClient.GetByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(userId);
        result.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingUser_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        // Act
        var result = await _userClient.GetByIdAsync(userId);

        // Assert
        result.Should().BeNull();
    }
}
```

### Test Example: Frontend.Client.Tests

```csharp
using Cheetah.Identity.Frontend.Client.Commands;
using Cheetah.Identity.Client.Interfaces;
using Cheetah.Identity.Contracts.Requests;
using FluentAssertions;
using Moq;
using Xunit;

namespace Cheetah.Identity.Frontend.Client.Tests.Commands;

public class CreateUserCommandHandlerTests
{
    private readonly Mock<IUserClientService> _userClientMock;
    private readonly CreateUserCommandHandler _handler;

    public CreateUserCommandHandlerTests()
    {
        _userClientMock = new Mock<IUserClientService>();
        _handler = new CreateUserCommandHandler(_userClientMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_CreatesUserAndReturnsId()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "newuser@example.com",
            FirstName = "New",
            LastName = "User",
            Password = "SecurePassword123!"
        };

        var expectedUserId = Guid.NewGuid();
        _userClientMock
            .Setup(c => c.CreateAsync(It.IsAny<CreateUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUserId);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().Be(expectedUserId);
        
        _userClientMock.Verify(c => c.CreateAsync(
            It.Is<CreateUserRequest>(r => 
                r.Email == command.Email &&
                r.FirstName == command.FirstName &&
                r.LastName == command.LastName),
            It.IsAny<CancellationToken>()), 
            Times.Once);
    }
}
```

### Step-by-Step Action Plan

1. **Create test project structure**
   ```bash
   # Identity
   mkdir -p src/Cheetah.Identity/Tests/Cheetah.Identity.Client.Tests
   mkdir -p src/Cheetah.Identity/Tests/Cheetah.Identity.Frontend.Client.Tests
   
   # Features
   mkdir -p src/Cheetah.Features/Tests/Cheetah.Features.Client.Tests
   mkdir -p src/Cheetah.Features/Tests/Cheetah.Features.Frontend.Client.Tests
   ```

2. **Create .csproj files** using the template above

3. **Add projects to solution**
   ```bash
   dotnet sln add src/Cheetah.Identity/Tests/Cheetah.Identity.Client.Tests/Cheetah.Identity.Client.Tests.csproj
   dotnet sln add src/Cheetah.Identity/Tests/Cheetah.Identity.Frontend.Client.Tests/Cheetah.Identity.Frontend.Client.Tests.csproj
   # ... same for Features
   ```

4. **Implement tests** for all client methods
   - Start with happy path tests
   - Add edge cases (null, not found, errors)
   - Mock HTTP responses appropriately

5. **Run tests and verify coverage**
   ```bash
   dotnet test --collect:"XPlat Code Coverage"
   ```

6. **Aim for >80% code coverage** in client libraries

### Verification Checklist
- [ ] All 4 test projects created and added to solution
- [ ] All client methods have at least 2 test cases (happy path + error)
- [ ] All frontend command/query handlers have tests
- [ ] Tests pass successfully
- [ ] Code coverage >80% for client libraries

---

## Issue #3 (BONUS): Add Domain and Application Tests

### **Severity:** 🟡 HIGH (Quality)
### **Estimated Effort:** 40-80 hours
### **Risk:** Low - Affects maintainability but not functionality

### Problem
Missing comprehensive unit tests for:
- Domain layer (entities, value objects, domain events)
- Application layer (command/query handlers, services)

### Required Structure
```
src/Cheetah.{ModuleName}/Tests/
├── Cheetah.{ModuleName}.Domain.Tests/
│   ├── Entities/
│   │   ├── TenantTests.cs
│   │   └── ...
│   └── ValueObjects/
│       └── ...
│
└── Cheetah.{ModuleName}.Application.Tests/
    ├── Commands/
    │   ├── CreateTenantCommandHandlerTests.cs
    │   └── ...
    └── Queries/
        ├── GetTenantByIdQueryHandlerTests.cs
        └── ...
```

### Domain Test Example
```csharp
public class TenantTests
{
    [Fact]
    public void Create_ValidParameters_CreatesTenantWithDomainEvent()
    {
        // Arrange
        var name = "Test Tenant";
        var subdomain = "test";

        // Act
        var tenant = Tenant.Create(name, subdomain);

        // Assert
        tenant.Should().NotBeNull();
        tenant.Id.Should().NotBe(Guid.Empty);
        tenant.Name.Should().Be(name);
        tenant.NormalizedName.Should().Be("TEST TENANT");
        tenant.Subdomain.Should().Be("test");
        tenant.IsActive.Should().BeFalse();
        tenant.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TenantCreatedEvent>();
    }

    [Fact]
    public void Activate_InactiveTenant_ActivatesAndRaisesEvent()
    {
        // Arrange
        var tenant = Tenant.Create("Test", null);
        tenant.ClearDomainEvents();

        // Act
        tenant.Activate();

        // Assert
        tenant.IsActive.Should().BeTrue();
        tenant.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TenantActivatedEvent>();
    }

    [Fact]
    public void Activate_AlreadyActiveTenant_ThrowsInvalidOperationException()
    {
        // Arrange
        var tenant = Tenant.Create("Test", null);
        tenant.Activate();

        // Act
        Action act = () => tenant.Activate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already active*");
    }
}
```

### Application Test Example
```csharp
public class CreateTenantCommandHandlerTests
{
    private readonly Mock<IRepository<Tenant, Guid>> _repositoryMock;
    private readonly Mock<IEventBus> _eventBusMock;
    private readonly CreateTenantCommandHandler _handler;

    public CreateTenantCommandHandlerTests()
    {
        _repositoryMock = new Mock<IRepository<Tenant, Guid>>();
        _eventBusMock = new Mock<IEventBus>();
        _handler = new CreateTenantCommandHandler(
            _repositoryMock.Object,
            _eventBusMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_CreatesTenantAndPublishesEvent()
    {
        // Arrange
        var command = new CreateTenantCommand("Test Tenant", "test");

        // Act
        var tenantId = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        tenantId.Should().NotBe(Guid.Empty);
        
        _repositoryMock.Verify(r => r.InsertAsync(
            It.Is<Tenant>(t => t.Name == "Test Tenant"),
            It.IsAny<CancellationToken>()), 
            Times.Once);
            
        _eventBusMock.Verify(e => e.PublishAsync(
            It.IsAny<TenantCreatedEvent>(),
            It.IsAny<CancellationToken>()), 
            Times.Once);
    }
}
```

---

## Summary

### Critical Issues (Must Fix Immediately)
1. ✅ **Add `builder.Ignore(t => t.DomainEvents)` to all entity configurations** (1-2 hours)
2. ✅ **Create missing Client test projects** (8-16 hours)

### High Priority (Should Fix Soon)
3. ⚠️ **Add Domain and Application tests** (40-80 hours)

### Total Estimated Effort
- **Minimum (Critical only):** 9-18 hours
- **Full (Including Domain/Application tests):** 49-98 hours

### Recommended Approach
1. **Week 1:** Fix Issue #1 (DomainEvents)
2. **Week 2-3:** Fix Issue #2 (Client tests)
3. **Ongoing:** Gradually add Domain and Application tests

---

**Created:** 2026-01-06  
**Next Update:** After Issue #1 is resolved
