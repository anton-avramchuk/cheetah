using Cheetah.Admin.Modules.Clients.Application.Commands;
using Cheetah.Admin.Modules.Clients.Domain;
using Cheetah.Admin.Modules.Clients.Domain.Repositories;
using Cheetah.Core.Domain.Exceptions;
using Cheetah.Core.Events;
using Shouldly;
using Moq;

namespace Cheetah.Admin.Modules.Clients.Application.Tests.Commands;

public class UpdateClientCommandHandlerTests
{
    private readonly Mock<IClientRepository> _repositoryMock;
    private readonly Mock<IEventBus> _eventBusMock;
    private readonly UpdateClientCommandHandler _handler;

    public UpdateClientCommandHandlerTests()
    {
        _repositoryMock = new Mock<IClientRepository>();
        _eventBusMock = new Mock<IEventBus>();
        _handler = new UpdateClientCommandHandler(_repositoryMock.Object, _eventBusMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithExistingClient_ShouldUpdateClient()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var existingClient = Client.Create("Original Name", "Original Description");

        // Use reflection to set the Id since it's set in Create
        typeof(Client).GetProperty("Id")!.SetValue(existingClient, clientId);

        var command = new UpdateClientCommand(clientId, "Updated Name", "Updated Description");

        _repositoryMock
            .Setup(r => r.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingClient);

        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        existingClient.Name.ShouldBe("Updated Name");
        existingClient.Description.ShouldBe("Updated Description");

        _repositoryMock.Verify(r => r.GetByIdAsync(clientId, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistingClient_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var command = new UpdateClientCommand(clientId, "Updated Name", "Updated Description");

        _repositoryMock
            .Setup(r => r.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client?)null);

        // Act
        var act = async () => await _handler.HandleAsync(command);

        // Assert
        await Should.ThrowAsync<EntityNotFoundException>(act);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyName_ShouldThrowArgumentException()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var existingClient = Client.Create("Original Name", "Original Description");
        typeof(Client).GetProperty("Id")!.SetValue(existingClient, clientId);

        var command = new UpdateClientCommand(clientId, "", "Updated Description");

        _repositoryMock
            .Setup(r => r.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingClient);

        // Act
        var act = async () => await _handler.HandleAsync(command);

        // Assert
        await Should.ThrowAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task HandleAsync_WithNullDescription_ShouldUpdateClient()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var existingClient = Client.Create("Original Name", "Original Description");
        typeof(Client).GetProperty("Id")!.SetValue(existingClient, clientId);

        var command = new UpdateClientCommand(clientId, "Updated Name", null);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingClient);

        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        existingClient.Name.ShouldBe("Updated Name");
        existingClient.Description.ShouldBeNull();
    }
}
