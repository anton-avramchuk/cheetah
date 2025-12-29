using Cheetah.Core.DataAccess;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Tenants.Application.Queries;
using Cheetah.Tenants.Domain.Entities;
using FluentAssertions;
using Moq;

namespace Cheetah.Tenants.Application.Tests.Queries;

public class GetAllTenantsQueryHandlerTests
{
    private readonly Mock<IReadOnlyRepository<Tenant, Guid>> _repositoryMock;
    private readonly GetAllTenantsQueryHandler _handler;

    public GetAllTenantsQueryHandlerTests()
    {
        _repositoryMock = new Mock<IReadOnlyRepository<Tenant, Guid>>();
        _handler = new GetAllTenantsQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnAllTenants_WhenTenantsExist()
    {
        // Arrange
        var tenant1 = Tenant.Create("Tenant 1", "tenant1");
        var tenant2 = Tenant.Create("Tenant 2", "tenant2");
        var tenant3 = Tenant.Create("Tenant 3", null);
        var tenants = new List<Tenant> { tenant1, tenant2, tenant3 }.AsReadOnly();

        var query = new GetAllTenantsQuery();

        _repositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenants);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().Contain(tenant1);
        result.Should().Contain(tenant2);
        result.Should().Contain(tenant3);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnEmptyList_WhenNoTenantsExist()
    {
        // Arrange
        var query = new GetAllTenantsQuery();
        var emptyList = new List<Tenant>().AsReadOnly();

        _repositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_ShouldCallRepository()
    {
        // Arrange
        var query = new GetAllTenantsQuery();
        var emptyList = new List<Tenant>().AsReadOnly();

        _repositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        // Act
        await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(
            x => x.GetAllAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
