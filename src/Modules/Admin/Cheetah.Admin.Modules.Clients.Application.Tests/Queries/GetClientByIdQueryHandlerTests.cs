using Cheetah.Admin.Modules.Clients.Application.Queries;
using Cheetah.Admin.Modules.Clients.Domain;
using Cheetah.Admin.Modules.Clients.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace Cheetah.Admin.Modules.Clients.Application.Tests.Queries;

public class GetClientByIdQueryHandlerTests
{
    private readonly Mock<IClientRepository> _repositoryMock;
    private readonly GetClientByIdQueryHandler _handler;

    public GetClientByIdQueryHandlerTests()
    {
        _repositoryMock = new Mock<IClientRepository>();
        _handler = new GetClientByIdQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithExistingClient_ShouldReturnClientModel()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var client = CreateClient(clientId, "Test Client", "Test Description", tenantId);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        var query = new GetClientByIdQuery(clientId);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(clientId);
        result.Name.Should().Be("Test Client");
        result.Tenant.Id.Should().Be(tenantId);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistingClient_ShouldReturnNull()
    {
        // Arrange
        var clientId = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client?)null);

        var query = new GetClientByIdQuery(clientId);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WithNullTenantId_ShouldMapToEmptyGuid()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var client = CreateClient(clientId, "Test Client", "Test Description", null);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        var query = new GetClientByIdQuery(clientId);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result!.Tenant.Id.Should().Be(Guid.Empty);
    }

    [Fact]
    public async Task HandleAsync_ShouldCallRepositoryWithCorrectId()
    {
        // Arrange
        var clientId = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client?)null);

        var query = new GetClientByIdQuery(clientId);

        // Act
        await _handler.HandleAsync(query);

        // Assert
        _repositoryMock.Verify(r => r.GetByIdAsync(clientId, It.IsAny<CancellationToken>()), Times.Once);
    }

    private static Client CreateClient(Guid id, string name, string? description, Guid? tenantId = null)
    {
        var client = Client.Create(name, description, tenantId);
        typeof(Client).GetProperty("Id")!.SetValue(client, id);
        return client;
    }
}
