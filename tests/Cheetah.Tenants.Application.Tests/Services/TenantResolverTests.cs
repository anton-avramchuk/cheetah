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
    private readonly TenantResolver _tenantResolver;

    public TenantResolverTests()
    {
        _dispatcherMock = new Mock<IDispatcher>();
        _tenantResolver = new TenantResolver(_dispatcherMock.Object);
    }

    private DefaultHttpContext CreateHttpContext()
    {
        return new DefaultHttpContext();
    }

    [Fact]
    public async Task ResolveTenantIdAsync_ShouldReturnTenantId_WhenXTenantIdHeaderIsPresent()
    {
        // Arrange
        var httpContext = CreateHttpContext();
        var tenantId = Guid.NewGuid();
        httpContext.Request.Headers["X-Tenant-Id"] = tenantId.ToString();

        // Act
        var result = await _tenantResolver.ResolveTenantIdAsync(httpContext);

        // Assert
        result.Should().Be(tenantId);
    }

    [Fact]
    public async Task ResolveTenantIdAsync_ShouldReturnNull_WhenXTenantIdHeaderIsInvalid()
    {
        // Arrange
        var httpContext = CreateHttpContext();
        httpContext.Request.Headers["X-Tenant-Id"] = "invalid-guid";
        httpContext.Request.Host = new HostString("localhost");

        // Act
        var result = await _tenantResolver.ResolveTenantIdAsync(httpContext);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ResolveTenantIdAsync_ShouldResolveTenantByHostSubdomain_WhenNoHeadersPresent()
    {
        // Arrange
        var httpContext = CreateHttpContext();
        var subdomain = "tenant1";
        var tenant = Tenant.Create("Tenant 1", subdomain);
        httpContext.Request.Host = new HostString($"{subdomain}.mycrm.com");

        _dispatcherMock
            .Setup(x => x.QueryAsync<GetTenantBySubdomainQuery, Tenant?>(
                It.Is<GetTenantBySubdomainQuery>(q => q.Subdomain == subdomain),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        // Act
        var result = await _tenantResolver.ResolveTenantIdAsync(httpContext);

        // Assert
        result.Should().Be(tenant.Id);
    }

    [Fact]
    public async Task ResolveTenantIdAsync_ShouldReturnNull_WhenHostDoesNotHaveSubdomain()
    {
        // Arrange
        var httpContext = CreateHttpContext();
        httpContext.Request.Host = new HostString("mycrm.com");

        // Act
        var result = await _tenantResolver.ResolveTenantIdAsync(httpContext);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ResolveTenantIdAsync_ShouldReturnNull_WhenHostSubdomainTenantNotFound()
    {
        // Arrange
        var httpContext = CreateHttpContext();
        httpContext.Request.Host = new HostString("non-existent.mycrm.com");

        _dispatcherMock
            .Setup(x => x.QueryAsync<GetTenantBySubdomainQuery, Tenant?>(
                It.IsAny<GetTenantBySubdomainQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);

        // Act
        var result = await _tenantResolver.ResolveTenantIdAsync(httpContext);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ResolveTenantIdAsync_ShouldPrioritizeXTenantIdHeader_OverHostSubdomain()
    {
        // Arrange
        var httpContext = CreateHttpContext();
        var tenantId = Guid.NewGuid();
        var subdomain = "test-subdomain";
        httpContext.Request.Headers["X-Tenant-Id"] = tenantId.ToString();
        httpContext.Request.Host = new HostString($"{subdomain}.mycrm.com");

        // Act
        var result = await _tenantResolver.ResolveTenantIdAsync(httpContext);

        // Assert
        result.Should().Be(tenantId);
        // Dispatcher should NOT be called because X-Tenant-Id header takes priority
        _dispatcherMock.Verify(
            x => x.QueryAsync<GetTenantBySubdomainQuery, Tenant?>(
                It.IsAny<GetTenantBySubdomainQuery>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
