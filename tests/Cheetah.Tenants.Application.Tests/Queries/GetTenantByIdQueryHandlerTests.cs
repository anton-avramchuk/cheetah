using Cheetah.Core.DataAccess;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Tenants.Application.Queries;
using Cheetah.Tenants.Domain.Entities;
using FluentAssertions;
using Moq;

namespace Cheetah.Tenants.Application.Tests.Queries;

public class GetTenantByIdQueryHandlerTests
{
    private readonly Mock<IReadOnlyRepository<Tenant, Guid>> _repositoryMock;
    private readonly GetTenantByIdQueryHandler _handler;

    public GetTenantByIdQueryHandlerTests()
    {
        _repositoryMock = new Mock<IReadOnlyRepository<Tenant, Guid>>();
        _handler = new GetTenantByIdQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnTenant_WhenTenantExists()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenant = Tenant.Create("Test Tenant", "test-subdomain");
        var query = new GetTenantByIdQuery(tenantId);

        _repositoryMock
            .Setup(x => x.GetAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(tenant);
        result!.Name.Should().Be("Test Tenant");
        result.Subdomain.Should().Be("test-subdomain");
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnNull_WhenTenantNotFound()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var query = new GetTenantByIdQuery(tenantId);

        _repositoryMock
            .Setup(x => x.GetAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_ShouldCallRepositoryWithCorrectId()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var query = new GetTenantByIdQuery(tenantId);

        _repositoryMock
            .Setup(x => x.GetAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);

        // Act
        await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(
            x => x.GetAsync(tenantId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
