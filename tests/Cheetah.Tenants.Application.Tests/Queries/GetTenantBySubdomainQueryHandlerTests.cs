using System.Linq.Expressions;
using Cheetah.Core.DataAccess;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Tenants.Application.Queries;
using Cheetah.Tenants.Domain.Entities;
using FluentAssertions;
using Moq;

namespace Cheetah.Tenants.Application.Tests.Queries;

public class GetTenantBySubdomainQueryHandlerTests
{
    private readonly Mock<IReadOnlyRepository<Tenant, Guid>> _repositoryMock;
    private readonly GetTenantBySubdomainQueryHandler _handler;

    public GetTenantBySubdomainQueryHandlerTests()
    {
        _repositoryMock = new Mock<IReadOnlyRepository<Tenant, Guid>>();
        _handler = new GetTenantBySubdomainQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnTenant_WhenTenantWithSubdomainExists()
    {
        // Arrange
        var tenant = Tenant.Create("Test Tenant", "test-subdomain");
        var query = new GetTenantBySubdomainQuery("test-subdomain");

        _repositoryMock
            .Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Tenant, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(tenant);
        result!.Subdomain.Should().Be("test-subdomain");
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnNull_WhenTenantWithSubdomainNotFound()
    {
        // Arrange
        var query = new GetTenantBySubdomainQuery("non-existent");

        _repositoryMock
            .Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Tenant, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_ShouldConvertSubdomainToLowerCase_BeforeQuery()
    {
        // Arrange
        var query = new GetTenantBySubdomainQuery("TEST-SUBDOMAIN");

        _repositoryMock
            .Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Tenant, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);

        // Act
        await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(
            x => x.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Tenant, bool>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldCallRepository()
    {
        // Arrange
        var query = new GetTenantBySubdomainQuery("test");

        _repositoryMock
            .Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Tenant, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);

        // Act
        await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(
            x => x.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Tenant, bool>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
