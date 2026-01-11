using Cheetah.Core.CQRS;
using Cheetah.Tenants.Application.Queries;
using Cheetah.Tenants.Application.Services;
using Cheetah.Tenants.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Cheetah.Tenants.Application.Tests.Services;

public class TenantResolverTests
{
    private readonly Mock<IDispatcher> _dispatcherMock;
    private readonly TenantResolver _resolver;

    public TenantResolverTests()
    {
        _dispatcherMock = new Mock<IDispatcher>();
        _resolver = new TenantResolver(_dispatcherMock.Object);
    }

    [Fact]
    public async Task ResolveTenantIdAsync_WithXTenantIdHeader_ShouldReturnTenantId()
    {
        // Arrange
        var expectedTenantId = Guid.NewGuid();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Tenant-Id"] = expectedTenantId.ToString();

        // Act
        var result = await _resolver.ResolveTenantIdAsync(httpContext);

        // Assert
        result.Should().Be(expectedTenantId);
    }

    [Fact]
    public async Task ResolveTenantIdAsync_WithInvalidXTenantIdHeader_ShouldTrySubdomain()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Tenant-Id"] = "invalid-guid";
        httpContext.Request.Host = new HostString("tenant1.mycrm.com");

        var tenant = Tenant.Create("Tenant 1", "tenant1");
        _dispatcherMock
            .Setup(x => x.QueryAsync<GetTenantBySubdomainQuery, Tenant?>(
                It.Is<GetTenantBySubdomainQuery>(q => q.Subdomain == "tenant1"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        // Act
        var result = await _resolver.ResolveTenantIdAsync(httpContext);

        // Assert
        result.Should().Be(tenant.Id);
    }

    [Fact]
    public async Task ResolveTenantIdAsync_WithSubdomain_ShouldReturnTenantId()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Host = new HostString("acme.mycrm.com");

        var tenant = Tenant.Create("Acme Corp", "acme");
        _dispatcherMock
            .Setup(x => x.QueryAsync<GetTenantBySubdomainQuery, Tenant?>(
                It.Is<GetTenantBySubdomainQuery>(q => q.Subdomain == "acme"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        // Act
        var result = await _resolver.ResolveTenantIdAsync(httpContext);

        // Assert
        result.Should().Be(tenant.Id);
        _dispatcherMock.Verify(
            x => x.QueryAsync<GetTenantBySubdomainQuery, Tenant?>(
                It.IsAny<GetTenantBySubdomainQuery>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ResolveTenantIdAsync_WithNonExistentSubdomain_ShouldReturnNull()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Host = new HostString("nonexistent.mycrm.com");

        _dispatcherMock
            .Setup(x => x.QueryAsync<GetTenantBySubdomainQuery, Tenant?>(
                It.IsAny<GetTenantBySubdomainQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);

        // Act
        var result = await _resolver.ResolveTenantIdAsync(httpContext);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ResolveTenantIdAsync_WithoutSubdomainOrHeader_ShouldReturnNull()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Host = new HostString("mycrm.com"); // No subdomain (only 2 parts)

        // Act
        var result = await _resolver.ResolveTenantIdAsync(httpContext);

        // Assert
        result.Should().BeNull();
        _dispatcherMock.Verify(
            x => x.QueryAsync<GetTenantBySubdomainQuery, Tenant?>(
                It.IsAny<GetTenantBySubdomainQuery>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ResolveTenantIdAsync_WithLocalhostSubdomain_ShouldReturnNull()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Host = new HostString("localhost"); // Single part

        // Act
        var result = await _resolver.ResolveTenantIdAsync(httpContext);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ResolveTenantIdAsync_HeaderTakesPrecedenceOverSubdomain()
    {
        // Arrange
        var headerTenantId = Guid.NewGuid();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Tenant-Id"] = headerTenantId.ToString();
        httpContext.Request.Host = new HostString("tenant1.mycrm.com");

        // Act
        var result = await _resolver.ResolveTenantIdAsync(httpContext);

        // Assert
        result.Should().Be(headerTenantId);
        // Should not query by subdomain if header is present
        _dispatcherMock.Verify(
            x => x.QueryAsync<GetTenantBySubdomainQuery, Tenant?>(
                It.IsAny<GetTenantBySubdomainQuery>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ResolveTenantIdAsync_WithDeepSubdomain_ShouldUseFirstPart()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Host = new HostString("tenant1.app.mycrm.com");

        var tenant = Tenant.Create("Tenant 1", "tenant1");
        _dispatcherMock
            .Setup(x => x.QueryAsync<GetTenantBySubdomainQuery, Tenant?>(
                It.Is<GetTenantBySubdomainQuery>(q => q.Subdomain == "tenant1"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        // Act
        var result = await _resolver.ResolveTenantIdAsync(httpContext);

        // Assert
        result.Should().Be(tenant.Id);
    }

    [Fact]
    public async Task ResolveTenantIdAsync_WithEmptyXTenantIdHeader_ShouldTrySubdomain()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Tenant-Id"] = "";
        httpContext.Request.Host = new HostString("test.mycrm.com");

        var tenant = Tenant.Create("Test", "test");
        _dispatcherMock
            .Setup(x => x.QueryAsync<GetTenantBySubdomainQuery, Tenant?>(
                It.Is<GetTenantBySubdomainQuery>(q => q.Subdomain == "test"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        // Act
        var result = await _resolver.ResolveTenantIdAsync(httpContext);

        // Assert
        result.Should().Be(tenant.Id);
    }
}
