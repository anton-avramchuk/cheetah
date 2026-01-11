using Cheetah.Tenants.Domain.Entities;
using Cheetah.Tenants.Events;
using FluentAssertions;

namespace Cheetah.Tenants.Domain.Tests;

public class TenantTests
{
    [Fact]
    public void Create_WithValidName_ShouldCreateTenant()
    {
        // Arrange
        var tenantName = "Acme Corporation";

        // Act
        var tenant = Tenant.Create(tenantName);

        // Assert
        tenant.Should().NotBeNull();
        tenant.Id.Should().NotBe(Guid.Empty);
        tenant.Name.Should().Be(tenantName);
        tenant.NormalizedName.Should().Be(tenantName.ToUpperInvariant());
        tenant.IsActive.Should().BeFalse(); // New tenants start inactive
        tenant.CreatedAt.Should().NotBeNull();
        tenant.ConnectionStrings.Should().BeEmpty();
        tenant.Subdomain.Should().BeNull();
    }

    [Fact]
    public void Create_WithSubdomain_ShouldNormalizeSubdomain()
    {
        // Arrange
        var tenantName = "Acme Corporation";
        var subdomain = "ACME";

        // Act
        var tenant = Tenant.Create(tenantName, subdomain);

        // Assert
        tenant.Subdomain.Should().Be("acme"); // Lowercased
    }

    [Fact]
    public void Create_ShouldRaiseTenantCreatedEvent()
    {
        // Arrange
        var tenantName = "Acme Corporation";

        // Act
        var tenant = Tenant.Create(tenantName);

        // Assert
        tenant.DomainEvents.Should().HaveCount(1);
        var domainEvent = tenant.DomainEvents.First();
        domainEvent.Should().BeOfType<TenantCreatedEvent>();
        var createdEvent = domainEvent as TenantCreatedEvent;
        createdEvent!.TenantId.Should().Be(tenant.Id);
        createdEvent.Name.Should().Be(tenantName);
    }

    [Fact]
    public void Activate_WhenInactive_ShouldActivateTenant()
    {
        // Arrange
        var tenant = Tenant.Create("Test Tenant");
        tenant.ClearDomainEvents();

        // Act
        tenant.Activate();

        // Assert
        tenant.IsActive.Should().BeTrue();
        tenant.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Activate_WhenInactive_ShouldRaiseTenantActivatedEvent()
    {
        // Arrange
        var tenant = Tenant.Create("Test Tenant");
        tenant.ClearDomainEvents();

        // Act
        tenant.Activate();

        // Assert
        tenant.DomainEvents.Should().HaveCount(1);
        var domainEvent = tenant.DomainEvents.First();
        domainEvent.Should().BeOfType<TenantActivatedEvent>();
        var activatedEvent = domainEvent as TenantActivatedEvent;
        activatedEvent!.TenantId.Should().Be(tenant.Id);
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var tenant = Tenant.Create("Test Tenant");
        tenant.Activate();

        // Act
        Action act = () => tenant.Activate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"*{tenant.Id}*already active*");
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldDeactivateTenant()
    {
        // Arrange
        var tenant = Tenant.Create("Test Tenant");
        tenant.Activate();
        tenant.ClearDomainEvents();

        // Act
        tenant.Deactivate();

        // Assert
        tenant.IsActive.Should().BeFalse();
        tenant.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldRaiseTenantDeactivatedEvent()
    {
        // Arrange
        var tenant = Tenant.Create("Test Tenant");
        tenant.Activate();
        tenant.ClearDomainEvents();

        // Act
        tenant.Deactivate();

        // Assert
        tenant.DomainEvents.Should().HaveCount(1);
        var domainEvent = tenant.DomainEvents.First();
        domainEvent.Should().BeOfType<TenantDeactivatedEvent>();
        var deactivatedEvent = domainEvent as TenantDeactivatedEvent;
        deactivatedEvent!.TenantId.Should().Be(tenant.Id);
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var tenant = Tenant.Create("Test Tenant");

        // Act
        Action act = () => tenant.Deactivate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"*{tenant.Id}*already inactive*");
    }

    [Fact]
    public void AddConnectionString_WithValidData_ShouldAddConnectionString()
    {
        // Arrange
        var tenant = Tenant.Create("Test Tenant");
        var connectionName = "Identity";
        var connectionString = "Server=localhost;Database=test;";

        // Act
        tenant.AddConnectionString(connectionName, connectionString);

        // Assert
        tenant.ConnectionStrings.Should().HaveCount(1);
        var cs = tenant.ConnectionStrings.First();
        cs.Name.Should().Be(connectionName);
        cs.ConnectionString.Should().Be(connectionString);
        cs.IsDefault.Should().BeFalse();
        tenant.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void AddConnectionString_WithDefaultFlag_ShouldSetAsDefault()
    {
        // Arrange
        var tenant = Tenant.Create("Test Tenant");
        var connectionName = "Default";
        var connectionString = "Server=localhost;Database=test;";

        // Act
        tenant.AddConnectionString(connectionName, connectionString, isDefault: true);

        // Assert
        var cs = tenant.ConnectionStrings.First();
        cs.IsDefault.Should().BeTrue();
    }

    [Fact]
    public void AddConnectionString_AsDefault_ShouldUnsetOtherDefaults()
    {
        // Arrange
        var tenant = Tenant.Create("Test Tenant");
        tenant.AddConnectionString("First", "Server=localhost;Database=first;", isDefault: true);

        // Act
        tenant.AddConnectionString("Second", "Server=localhost;Database=second;", isDefault: true);

        // Assert
        tenant.ConnectionStrings.Should().HaveCount(2);
        var first = tenant.ConnectionStrings.First(cs => cs.Name == "First");
        var second = tenant.ConnectionStrings.First(cs => cs.Name == "Second");
        first.IsDefault.Should().BeFalse();
        second.IsDefault.Should().BeTrue();
    }

    [Fact]
    public void AddConnectionString_WithDuplicateName_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var tenant = Tenant.Create("Test Tenant");
        var connectionName = "Identity";
        tenant.AddConnectionString(connectionName, "Server=localhost;Database=first;");

        // Act
        Action act = () => tenant.AddConnectionString(connectionName, "Server=localhost;Database=second;");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"*'{connectionName}'*already exists*");
    }

    [Fact]
    public void UpdateConnectionString_WithExistingName_ShouldUpdateConnectionString()
    {
        // Arrange
        var tenant = Tenant.Create("Test Tenant");
        var connectionName = "Identity";
        var oldConnectionString = "Server=localhost;Database=old;";
        var newConnectionString = "Server=localhost;Database=new;";
        tenant.AddConnectionString(connectionName, oldConnectionString);

        // Act
        tenant.UpdateConnectionString(connectionName, newConnectionString);

        // Assert
        var cs = tenant.ConnectionStrings.First();
        cs.ConnectionString.Should().Be(newConnectionString);
        tenant.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateConnectionString_WithNonExistingName_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var tenant = Tenant.Create("Test Tenant");
        var connectionName = "NonExisting";

        // Act
        Action act = () => tenant.UpdateConnectionString(connectionName, "Server=localhost;");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"*'{connectionName}'*not found*");
    }

    [Fact]
    public void SetDefaultConnectionString_WithExistingName_ShouldSetAsDefault()
    {
        // Arrange
        var tenant = Tenant.Create("Test Tenant");
        tenant.AddConnectionString("First", "Server=localhost;Database=first;", isDefault: true);
        tenant.AddConnectionString("Second", "Server=localhost;Database=second;");

        // Act
        tenant.SetDefaultConnectionString("Second");

        // Assert
        var first = tenant.ConnectionStrings.First(cs => cs.Name == "First");
        var second = tenant.ConnectionStrings.First(cs => cs.Name == "Second");
        first.IsDefault.Should().BeFalse();
        second.IsDefault.Should().BeTrue();
        tenant.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void SetDefaultConnectionString_ShouldUnsetOtherDefaults()
    {
        // Arrange
        var tenant = Tenant.Create("Test Tenant");
        tenant.AddConnectionString("First", "Server=localhost;Database=first;", isDefault: true);
        tenant.AddConnectionString("Second", "Server=localhost;Database=second;", isDefault: false);
        tenant.AddConnectionString("Third", "Server=localhost;Database=third;", isDefault: false);

        // Act
        tenant.SetDefaultConnectionString("Third");

        // Assert
        tenant.ConnectionStrings.Count(cs => cs.IsDefault).Should().Be(1);
        var third = tenant.ConnectionStrings.First(cs => cs.Name == "Third");
        third.IsDefault.Should().BeTrue();
    }

    [Fact]
    public void SetDefaultConnectionString_WithNonExistingName_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var tenant = Tenant.Create("Test Tenant");
        var connectionName = "NonExisting";

        // Act
        Action act = () => tenant.SetDefaultConnectionString(connectionName);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"*'{connectionName}'*not found*");
    }

    [Fact]
    public void TenantConnectionStrings_ShouldBeReadOnly()
    {
        // Arrange
        var tenant = Tenant.Create("Test Tenant");

        // Assert
        tenant.ConnectionStrings.Should().BeAssignableTo<IReadOnlyCollection<TenantConnectionString>>();
    }
}
