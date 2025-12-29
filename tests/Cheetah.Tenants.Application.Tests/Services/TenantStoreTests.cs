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
    public async Task FindByIdAsync_ShouldReturnTenant_WhenTenantExists()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var expectedTenant = Tenant.Create("Test Tenant", "test-subdomain");

        _dispatcherMock
            .Setup(x => x.QueryAsync<GetTenantByIdQuery, Tenant?>(
                It.Is<GetTenantByIdQuery>(q => q.TenantId == tenantId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTenant);

        // Act
        var result = await _tenantStore.FindByIdAsync(tenantId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(expectedTenant);
    }

    [Fact]
    public async Task FindByIdAsync_ShouldReturnNull_WhenTenantNotFound()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        _dispatcherMock
            .Setup(x => x.QueryAsync<GetTenantByIdQuery, Tenant?>(
                It.IsAny<GetTenantByIdQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);

        // Act
        var result = await _tenantStore.FindByIdAsync(tenantId, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task FindByIdAsync_ShouldCallDispatcherWithCorrectQuery()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        _dispatcherMock
            .Setup(x => x.QueryAsync<GetTenantByIdQuery, Tenant?>(
                It.IsAny<GetTenantByIdQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);

        // Act
        await _tenantStore.FindByIdAsync(tenantId, CancellationToken.None);

        // Assert
        _dispatcherMock.Verify(
            x => x.QueryAsync<GetTenantByIdQuery, Tenant?>(
                It.Is<GetTenantByIdQuery>(q => q.TenantId == tenantId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task FindBySubdomainAsync_ShouldReturnTenant_WhenTenantExists()
    {
        // Arrange
        var subdomain = "test-subdomain";
        var expectedTenant = Tenant.Create("Test Tenant", subdomain);

        _dispatcherMock
            .Setup(x => x.QueryAsync<GetTenantBySubdomainQuery, Tenant?>(
                It.Is<GetTenantBySubdomainQuery>(q => q.Subdomain == subdomain),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTenant);

        // Act
        var result = await _tenantStore.FindBySubdomainAsync(subdomain, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(expectedTenant);
    }

    [Fact]
    public async Task FindBySubdomainAsync_ShouldReturnNull_WhenTenantNotFound()
    {
        // Arrange
        var subdomain = "non-existent";

        _dispatcherMock
            .Setup(x => x.QueryAsync<GetTenantBySubdomainQuery, Tenant?>(
                It.IsAny<GetTenantBySubdomainQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);

        // Act
        var result = await _tenantStore.FindBySubdomainAsync(subdomain, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task FindBySubdomainAsync_ShouldCallDispatcherWithCorrectQuery()
    {
        // Arrange
        var subdomain = "test-subdomain";

        _dispatcherMock
            .Setup(x => x.QueryAsync<GetTenantBySubdomainQuery, Tenant?>(
                It.IsAny<GetTenantBySubdomainQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);

        // Act
        await _tenantStore.FindBySubdomainAsync(subdomain, CancellationToken.None);

        // Assert
        _dispatcherMock.Verify(
            x => x.QueryAsync<GetTenantBySubdomainQuery, Tenant?>(
                It.Is<GetTenantBySubdomainQuery>(q => q.Subdomain == subdomain),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllTenants()
    {
        // Arrange
        var tenant1 = Tenant.Create("Tenant 1", "tenant1");
        var tenant2 = Tenant.Create("Tenant 2", "tenant2");
        var expectedTenants = new List<Tenant> { tenant1, tenant2 }.AsReadOnly();

        _dispatcherMock
            .Setup(x => x.QueryAsync<GetAllTenantsQuery, IReadOnlyList<Tenant>>(
                It.IsAny<GetAllTenantsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTenants);

        // Act
        var result = await _tenantStore.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(tenant1);
        result.Should().Contain(tenant2);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoTenants()
    {
        // Arrange
        var emptyList = new List<Tenant>().AsReadOnly();

        _dispatcherMock
            .Setup(x => x.QueryAsync<GetAllTenantsQuery, IReadOnlyList<Tenant>>(
                It.IsAny<GetAllTenantsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        // Act
        var result = await _tenantStore.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_ShouldCallDispatcherWithCorrectQuery()
    {
        // Arrange
        var emptyList = new List<Tenant>().AsReadOnly();

        _dispatcherMock
            .Setup(x => x.QueryAsync<GetAllTenantsQuery, IReadOnlyList<Tenant>>(
                It.IsAny<GetAllTenantsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        // Act
        await _tenantStore.GetAllAsync(CancellationToken.None);

        // Assert
        _dispatcherMock.Verify(
            x => x.QueryAsync<GetAllTenantsQuery, IReadOnlyList<Tenant>>(
                It.IsAny<GetAllTenantsQuery>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
