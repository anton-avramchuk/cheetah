using Cheetah.Tenants.Application.Services;
using FluentAssertions;

namespace Cheetah.Tenants.Application.Tests.Services;

public class CurrentTenantTests
{
    private readonly CurrentTenant _currentTenant;

    public CurrentTenantTests()
    {
        _currentTenant = new CurrentTenant();
    }

    [Fact]
    public void Id_ShouldReturnNull_WhenTenantNotSet()
    {
        // Act
        var id = _currentTenant.Id;

        // Assert
        id.Should().BeNull();
    }

    [Fact]
    public void Name_ShouldReturnNull_WhenTenantNotSet()
    {
        // Act
        var name = _currentTenant.Name;

        // Assert
        name.Should().BeNull();
    }

    [Fact]
    public void IsAvailable_ShouldReturnFalse_WhenTenantNotSet()
    {
        // Act
        var isAvailable = _currentTenant.IsAvailable;

        // Assert
        isAvailable.Should().BeFalse();
    }

    [Fact]
    public void SetTenant_ShouldSetTenantId()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        _currentTenant.SetTenant(tenantId);

        // Assert
        _currentTenant.Id.Should().Be(tenantId);
    }

    [Fact]
    public void SetTenant_ShouldSetTenantName()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenantName = "Test Tenant";

        // Act
        _currentTenant.SetTenant(tenantId, tenantName);

        // Assert
        _currentTenant.Name.Should().Be(tenantName);
    }

    [Fact]
    public void IsAvailable_ShouldReturnTrue_WhenTenantIdIsSet()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        _currentTenant.SetTenant(tenantId);

        // Assert
        _currentTenant.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public void SetTenant_ShouldAllowNullTenantId()
    {
        // Arrange
        _currentTenant.SetTenant(Guid.NewGuid());

        // Act
        _currentTenant.SetTenant(null);

        // Assert
        _currentTenant.Id.Should().BeNull();
        _currentTenant.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public void SetTenant_ShouldAllowChangingTenant()
    {
        // Arrange
        var firstTenantId = Guid.NewGuid();
        var secondTenantId = Guid.NewGuid();
        _currentTenant.SetTenant(firstTenantId, "First Tenant");

        // Act
        _currentTenant.SetTenant(secondTenantId, "Second Tenant");

        // Assert
        _currentTenant.Id.Should().Be(secondTenantId);
        _currentTenant.Name.Should().Be("Second Tenant");
    }

    [Fact]
    public void SetTenant_ShouldSetOnlyId_WhenNameNotProvided()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        _currentTenant.SetTenant(tenantId);

        // Assert
        _currentTenant.Id.Should().Be(tenantId);
        _currentTenant.Name.Should().BeNull();
    }
}
