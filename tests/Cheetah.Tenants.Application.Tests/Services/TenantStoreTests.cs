using Cheetah.Core.CQRS;
using Cheetah.Tenants.Application.Queries;
using Cheetah.Tenants.Application.Services;
using Cheetah.Tenants.Domain.Entities;
using FluentAssertions;
using Moq;

namespace Cheetah.Tenants.Application.Tests.Services;

public class TenantStoreTests
{
    private readonly Mock<IDispatcher> _dispatcherMock;
    private readonly TenantStore _tenantStore;

    public TenantStoreTests()
    {
        _dispatcherMock = new Mock<IDispatcher>();
        _tenantStore = new TenantStore(_dispatcherMock.Object);
    }

    [Fact]
    public async Task FindByIdAsync_WithExistingTenant_ShouldReturnTenant()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var expectedTenant = Tenant.Create("Test Tenant");

        _dispatcherMock
            .Setup(x => x.QueryAsync<GetTenantByIdQuery, Tenant?>(
                It.Is<GetTenantByIdQuery>(q => q.TenantId == tenantId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTenant);

        // Act
        var result = await _tenantStore.FindByIdAsync(tenantId);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(expectedTenant);
    }

    [Fact]
    public async Task FindByIdAsync_WithNonExistentTenant_ShouldReturnNull()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        _dispatcherMock
            .Setup(x => x.QueryAsync<GetTenantByIdQuery, Tenant?>(
                It.IsAny<GetTenantByIdQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);

        // Act
        var result = await _tenantStore.FindByIdAsync(tenantId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllTenants()
    {
        // Arrange
        var tenants = new List<Tenant>
        {
            Tenant.Create("Tenant 1"),
            Tenant.Create("Tenant 2"),
            Tenant.Create("Tenant 3")
        };

        _dispatcherMock
            .Setup(x => x.QueryAsync<GetAllTenantsQuery, IReadOnlyList<Tenant>>(
                It.IsAny<GetAllTenantsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenants);

        // Act
        var result = await _tenantStore.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(tenants);
    }

    [Fact]
    public async Task GetConnectionStringAsync_WithExistingTenant_ShouldReturnConnectionString()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenant = Tenant.Create("Test Tenant");
        tenant.AddConnectionString("Default", "Server=localhost;Database=test;");

        _dispatcherMock
            .Setup(x => x.QueryAsync<GetTenantByIdQuery, Tenant?>(
                It.Is<GetTenantByIdQuery>(q => q.TenantId == tenantId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        // Act
        var result = await _tenantStore.GetConnectionStringAsync(tenantId);

        // Assert
        result.Should().Be("Server=localhost;Database=test;");
    }

    [Fact]
    public async Task GetAllActiveTenantsAsync_ShouldReturnOnlyActiveTenants()
    {
        // Arrange
        var tenant1 = Tenant.Create("Tenant 1");
        tenant1.Activate();

        var tenant2 = Tenant.Create("Tenant 2");

        var tenant3 = Tenant.Create("Tenant 3");
        tenant3.Activate();

        var allTenants = new List<Tenant> { tenant1, tenant2, tenant3 };

        _dispatcherMock
            .Setup(x => x.QueryAsync<GetAllTenantsQuery, IReadOnlyList<Tenant>>(
                It.IsAny<GetAllTenantsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(allTenants);

        // Act
        var result = await _tenantStore.GetAllActiveTenantsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(tenant1);
        result.Should().Contain(tenant3);
        result.Should().NotContain(tenant2);
    }

    [Fact]
    public async Task FindBySubdomainAsync_WithExistingSubdomain_ShouldReturnTenant()
    {
        // Arrange
        var subdomain = "acme";
        var expectedTenant = Tenant.Create("Acme Corporation", subdomain);

        _dispatcherMock
            .Setup(x => x.QueryAsync<GetTenantBySubdomainQuery, Tenant?>(
                It.Is<GetTenantBySubdomainQuery>(q => q.Subdomain == subdomain),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTenant);

        // Act
        var result = await _tenantStore.FindBySubdomainAsync(subdomain);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(expectedTenant);
        result!.Subdomain.Should().Be(subdomain);
    }

    [Fact]
    public async Task FindBySubdomainAsync_WithNonExistentSubdomain_ShouldReturnNull()
    {
        // Arrange
        var subdomain = "nonexistent";

        _dispatcherMock
            .Setup(x => x.QueryAsync<GetTenantBySubdomainQuery, Tenant?>(
                It.IsAny<GetTenantBySubdomainQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);

        // Act
        var result = await _tenantStore.FindBySubdomainAsync(subdomain);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetConnectionStringAsync_WithSpecificName_ShouldReturnCorrectConnectionString()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenant = Tenant.Create("Test Tenant");
        tenant.AddConnectionString("Default", "Server=localhost;Database=default;");
        tenant.AddConnectionString("Identity", "Server=localhost;Database=identity;");
        tenant.AddConnectionString("Features", "Server=localhost;Database=features;");

        _dispatcherMock
            .Setup(x => x.QueryAsync<GetTenantByIdQuery, Tenant?>(
                It.Is<GetTenantByIdQuery>(q => q.TenantId == tenantId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        // Act
        var result = await _tenantStore.GetConnectionStringAsync(tenantId, "Identity");

        // Assert
        result.Should().Be("Server=localhost;Database=identity;");
    }

    [Fact]
    public async Task GetConnectionStringAsync_WithNonExistentName_ShouldReturnNull()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenant = Tenant.Create("Test Tenant");
        tenant.AddConnectionString("Default", "Server=localhost;Database=test;");

        _dispatcherMock
            .Setup(x => x.QueryAsync<GetTenantByIdQuery, Tenant?>(
                It.Is<GetTenantByIdQuery>(q => q.TenantId == tenantId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        // Act
        var result = await _tenantStore.GetConnectionStringAsync(tenantId, "NonExistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetConnectionStringAsync_WithNonExistentTenant_ShouldReturnNull()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        _dispatcherMock
            .Setup(x => x.QueryAsync<GetTenantByIdQuery, Tenant?>(
                It.IsAny<GetTenantByIdQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);

        // Act
        var result = await _tenantStore.GetConnectionStringAsync(tenantId, "Default");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllActiveTenantsAsync_WithEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        var emptyList = new List<Tenant>();

        _dispatcherMock
            .Setup(x => x.QueryAsync<GetAllTenantsQuery, IReadOnlyList<Tenant>>(
                It.IsAny<GetAllTenantsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        // Act
        var result = await _tenantStore.GetAllActiveTenantsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllActiveTenantsAsync_WhenAllTenantsInactive_ShouldReturnEmptyList()
    {
        // Arrange
        var tenant1 = Tenant.Create("Tenant 1");
        var tenant2 = Tenant.Create("Tenant 2");
        var tenant3 = Tenant.Create("Tenant 3");

        var allTenants = new List<Tenant> { tenant1, tenant2, tenant3 };

        _dispatcherMock
            .Setup(x => x.QueryAsync<GetAllTenantsQuery, IReadOnlyList<Tenant>>(
                It.IsAny<GetAllTenantsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(allTenants);

        // Act
        var result = await _tenantStore.GetAllActiveTenantsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_WithEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        var emptyList = new List<Tenant>();

        _dispatcherMock
            .Setup(x => x.QueryAsync<GetAllTenantsQuery, IReadOnlyList<Tenant>>(
                It.IsAny<GetAllTenantsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        // Act
        var result = await _tenantStore.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }
}
