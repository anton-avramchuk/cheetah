using Cheetah.Core.CQRS;
using Cheetah.Tenants.Application.Queries;
using Cheetah.Tenants.Application.Services;
using Cheetah.Tenants.Client.Implementation;
using Cheetah.Tenants.Domain.Entities;
using FluentAssertions;
using Moq;

namespace Cheetah.Tenants.Client.Tests.Implementation;

public class TenantClientServiceTests
{
    private readonly Mock<IDispatcher> _dispatcherMock;
    private readonly Mock<ITenantStore> _tenantStoreMock;
    private readonly TenantClientService _service;

    public TenantClientServiceTests()
    {
        _dispatcherMock = new Mock<IDispatcher>();
        _tenantStoreMock = new Mock<ITenantStore>();
        _service = new TenantClientService(_dispatcherMock.Object, _tenantStoreMock.Object);
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenTenantExists_ShouldReturnViewModel()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenant = Tenant.Create("Test Tenant", "test");
        typeof(Tenant).GetProperty(nameof(Tenant.Id))!.SetValue(tenant, tenantId);

        _dispatcherMock
            .Setup(x => x.QueryAsync<GetTenantByIdQuery, Tenant?>(
                It.Is<GetTenantByIdQuery>(q => q.TenantId == tenantId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        // Act
        var result = await _service.GetByIdAsync(tenantId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(tenantId);
        result.Name.Should().Be("Test Tenant");
        result.Subdomain.Should().Be("test");
    }

    [Fact]
    public async Task GetByIdAsync_WhenTenantNotFound_ShouldReturnNull()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        _dispatcherMock
            .Setup(x => x.QueryAsync<GetTenantByIdQuery, Tenant?>(
                It.IsAny<GetTenantByIdQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);

        // Act
        var result = await _service.GetByIdAsync(tenantId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldCallDispatcherWithCorrectQuery()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        _dispatcherMock
            .Setup(x => x.QueryAsync<GetTenantByIdQuery, Tenant?>(
                It.IsAny<GetTenantByIdQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);

        // Act
        await _service.GetByIdAsync(tenantId);

        // Assert
        _dispatcherMock.Verify(
            x => x.QueryAsync<GetTenantByIdQuery, Tenant?>(
                It.Is<GetTenantByIdQuery>(q => q.TenantId == tenantId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetBySubdomainAsync Tests

    [Fact]
    public async Task GetBySubdomainAsync_WhenTenantExists_ShouldReturnViewModel()
    {
        // Arrange
        var subdomain = "test-subdomain";
        var tenant = Tenant.Create("Test Tenant", subdomain);

        _dispatcherMock
            .Setup(x => x.QueryAsync<GetTenantBySubdomainQuery, Tenant?>(
                It.Is<GetTenantBySubdomainQuery>(q => q.Subdomain == subdomain),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        // Act
        var result = await _service.GetBySubdomainAsync(subdomain);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Tenant");
        result.Subdomain.Should().Be(subdomain);
    }

    [Fact]
    public async Task GetBySubdomainAsync_WhenTenantNotFound_ShouldReturnNull()
    {
        // Arrange
        var subdomain = "nonexistent";

        _dispatcherMock
            .Setup(x => x.QueryAsync<GetTenantBySubdomainQuery, Tenant?>(
                It.IsAny<GetTenantBySubdomainQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);

        // Act
        var result = await _service.GetBySubdomainAsync(subdomain);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBySubdomainAsync_ShouldCallDispatcherWithCorrectQuery()
    {
        // Arrange
        var subdomain = "test";

        _dispatcherMock
            .Setup(x => x.QueryAsync<GetTenantBySubdomainQuery, Tenant?>(
                It.IsAny<GetTenantBySubdomainQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);

        // Act
        await _service.GetBySubdomainAsync(subdomain);

        // Assert
        _dispatcherMock.Verify(
            x => x.QueryAsync<GetTenantBySubdomainQuery, Tenant?>(
                It.Is<GetTenantBySubdomainQuery>(q => q.Subdomain == subdomain),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetAllActiveAsync Tests

    [Fact]
    public async Task GetAllActiveAsync_WhenMultipleTenants_ShouldReturnOnlyActive()
    {
        // Arrange
        var tenant1 = Tenant.Create("Active Tenant 1", "active1");
        tenant1.Activate();

        var tenant2 = Tenant.Create("Inactive Tenant", "inactive");
        // tenant2 is already inactive by default

        var tenant3 = Tenant.Create("Active Tenant 2", "active2");
        tenant3.Activate();

        var tenants = new List<Tenant> { tenant1, tenant2, tenant3 };

        _dispatcherMock
            .Setup(x => x.QueryAsync<GetAllTenantsQuery, IReadOnlyList<Tenant>>(
                It.IsAny<GetAllTenantsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenants);

        // Act
        var result = await _service.GetAllActiveAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(vm => vm.IsActive);
        result.Select(vm => vm.Name).Should().Contain(new[] { "Active Tenant 1", "Active Tenant 2" });
    }

    [Fact]
    public async Task GetAllActiveAsync_WhenNoActiveTenants_ShouldReturnEmptyList()
    {
        // Arrange
        var tenant = Tenant.Create("Inactive Tenant", "inactive");
        // tenant is already inactive by default

        var tenants = new List<Tenant> { tenant };

        _dispatcherMock
            .Setup(x => x.QueryAsync<GetAllTenantsQuery, IReadOnlyList<Tenant>>(
                It.IsAny<GetAllTenantsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenants);

        // Act
        var result = await _service.GetAllActiveAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllActiveAsync_WhenNoTenants_ShouldReturnEmptyList()
    {
        // Arrange
        _dispatcherMock
            .Setup(x => x.QueryAsync<GetAllTenantsQuery, IReadOnlyList<Tenant>>(
                It.IsAny<GetAllTenantsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Tenant>());

        // Act
        var result = await _service.GetAllActiveAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetConnectionStringAsync Tests

    [Fact]
    public async Task GetConnectionStringAsync_WhenTenantExistsWithConnectionString_ShouldReturnConnectionString()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenant = Tenant.Create("Test Tenant", "test");
        typeof(Tenant).GetProperty(nameof(Tenant.Id))!.SetValue(tenant, tenantId);

        tenant.AddConnectionString("Default", "Server=localhost;Database=TestDb");

        _tenantStoreMock
            .Setup(x => x.FindByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        // Act
        var result = await _service.GetConnectionStringAsync(tenantId);

        // Assert
        result.Should().Be("Server=localhost;Database=TestDb");
    }

    [Fact]
    public async Task GetConnectionStringAsync_WhenRequestingSpecificName_ShouldReturnCorrectConnectionString()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenant = Tenant.Create("Test Tenant", "test");
        typeof(Tenant).GetProperty(nameof(Tenant.Id))!.SetValue(tenant, tenantId);

        tenant.AddConnectionString("Default", "Server=localhost;Database=Default");
        tenant.AddConnectionString("Reporting", "Server=localhost;Database=Reporting");

        _tenantStoreMock
            .Setup(x => x.FindByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        // Act
        var result = await _service.GetConnectionStringAsync(tenantId, "Reporting");

        // Assert
        result.Should().Be("Server=localhost;Database=Reporting");
    }

    [Fact]
    public async Task GetConnectionStringAsync_WhenTenantNotFound_ShouldReturnNull()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        _tenantStoreMock
            .Setup(x => x.FindByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);

        // Act
        var result = await _service.GetConnectionStringAsync(tenantId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetConnectionStringAsync_WhenConnectionStringNotFound_ShouldReturnNull()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenant = Tenant.Create("Test Tenant", "test");
        typeof(Tenant).GetProperty(nameof(Tenant.Id))!.SetValue(tenant, tenantId);

        tenant.AddConnectionString("Default", "Server=localhost;Database=Default");

        _tenantStoreMock
            .Setup(x => x.FindByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        // Act
        var result = await _service.GetConnectionStringAsync(tenantId, "NonExistent");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region IsActiveAsync Tests

    [Fact]
    public async Task IsActiveAsync_WhenTenantIsActive_ShouldReturnTrue()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenant = Tenant.Create("Test Tenant", "test");
        typeof(Tenant).GetProperty(nameof(Tenant.Id))!.SetValue(tenant, tenantId);
        tenant.Activate();

        _tenantStoreMock
            .Setup(x => x.FindByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        // Act
        var result = await _service.IsActiveAsync(tenantId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsActiveAsync_WhenTenantIsInactive_ShouldReturnFalse()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenant = Tenant.Create("Test Tenant", "test");
        typeof(Tenant).GetProperty(nameof(Tenant.Id))!.SetValue(tenant, tenantId);
        // tenant is already inactive by default

        _tenantStoreMock
            .Setup(x => x.FindByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        // Act
        var result = await _service.IsActiveAsync(tenantId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsActiveAsync_WhenTenantNotFound_ShouldReturnFalse()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        _tenantStoreMock
            .Setup(x => x.FindByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);

        // Act
        var result = await _service.IsActiveAsync(tenantId);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ExistsAsync Tests

    [Fact]
    public async Task ExistsAsync_WhenTenantExists_ShouldReturnTrue()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenant = Tenant.Create("Test Tenant", "test");
        typeof(Tenant).GetProperty(nameof(Tenant.Id))!.SetValue(tenant, tenantId);

        _tenantStoreMock
            .Setup(x => x.FindByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        // Act
        var result = await _service.ExistsAsync(tenantId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WhenTenantNotFound_ShouldReturnFalse()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        _tenantStoreMock
            .Setup(x => x.FindByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);

        // Act
        var result = await _service.ExistsAsync(tenantId);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region CancellationToken Tests

    [Fact]
    public async Task GetByIdAsync_ShouldPassCancellationToken()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var cts = new CancellationTokenSource();
        CancellationToken receivedToken = default;

        _dispatcherMock
            .Setup(x => x.QueryAsync<GetTenantByIdQuery, Tenant?>(
                It.IsAny<GetTenantByIdQuery>(),
                It.IsAny<CancellationToken>()))
            .Callback<GetTenantByIdQuery, CancellationToken>((_, ct) => receivedToken = ct)
            .ReturnsAsync((Tenant?)null);

        // Act
        await _service.GetByIdAsync(tenantId, cts.Token);

        // Assert
        receivedToken.Should().Be(cts.Token);
    }

    [Fact]
    public async Task GetConnectionStringAsync_ShouldPassCancellationToken()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var cts = new CancellationTokenSource();
        CancellationToken receivedToken = default;

        _tenantStoreMock
            .Setup(x => x.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, CancellationToken>((_, ct) => receivedToken = ct)
            .ReturnsAsync((Tenant?)null);

        // Act
        await _service.GetConnectionStringAsync(tenantId, "Default", cts.Token);

        // Assert
        receivedToken.Should().Be(cts.Token);
    }

    #endregion
}
