using Cheetah.Tenants.Application.Services;
using FluentAssertions;

namespace Cheetah.Tenants.Application.Tests.Services;

public class CurrentTenantTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithNoTenant()
    {
        // Act
        var currentTenant = new CurrentTenant();

        // Assert
        currentTenant.Id.Should().BeNull();
        currentTenant.Name.Should().BeNull();
        currentTenant.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public void SetTenant_WithValidTenantId_ShouldSetTenantId()
    {
        // Arrange
        var currentTenant = new CurrentTenant();
        var tenantId = Guid.NewGuid();

        // Act
        currentTenant.SetTenant(tenantId);

        // Assert
        currentTenant.Id.Should().Be(tenantId);
        currentTenant.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public void SetTenant_WithTenantIdAndName_ShouldSetBoth()
    {
        // Arrange
        var currentTenant = new CurrentTenant();
        var tenantId = Guid.NewGuid();
        var tenantName = "Acme Corporation";

        // Act
        currentTenant.SetTenant(tenantId, tenantName);

        // Assert
        currentTenant.Id.Should().Be(tenantId);
        currentTenant.Name.Should().Be(tenantName);
        currentTenant.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public void SetTenant_WithNullTenantId_ShouldClearTenant()
    {
        // Arrange
        var currentTenant = new CurrentTenant();
        currentTenant.SetTenant(Guid.NewGuid(), "Test Tenant");

        // Act
        currentTenant.SetTenant(null);

        // Assert
        currentTenant.Id.Should().BeNull();
        currentTenant.Name.Should().BeNull();
        currentTenant.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public void SetTenant_CalledMultipleTimes_ShouldUpdateTenant()
    {
        // Arrange
        var currentTenant = new CurrentTenant();
        var firstTenantId = Guid.NewGuid();
        var secondTenantId = Guid.NewGuid();

        // Act
        currentTenant.SetTenant(firstTenantId, "First Tenant");
        currentTenant.SetTenant(secondTenantId, "Second Tenant");

        // Assert
        currentTenant.Id.Should().Be(secondTenantId);
        currentTenant.Name.Should().Be("Second Tenant");
        currentTenant.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public void IsAvailable_WhenTenantIsSet_ShouldReturnTrue()
    {
        // Arrange
        var currentTenant = new CurrentTenant();
        var tenantId = Guid.NewGuid();

        // Act
        currentTenant.SetTenant(tenantId);

        // Assert
        currentTenant.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public void IsAvailable_WhenTenantIsNotSet_ShouldReturnFalse()
    {
        // Arrange
        var currentTenant = new CurrentTenant();

        // Assert
        currentTenant.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public void SetTenant_WithOnlyTenantId_ShouldLeaveNameNull()
    {
        // Arrange
        var currentTenant = new CurrentTenant();
        var tenantId = Guid.NewGuid();

        // Act
        currentTenant.SetTenant(tenantId);

        // Assert
        currentTenant.Id.Should().Be(tenantId);
        currentTenant.Name.Should().BeNull();
        currentTenant.IsAvailable.Should().BeTrue();
    }
}
