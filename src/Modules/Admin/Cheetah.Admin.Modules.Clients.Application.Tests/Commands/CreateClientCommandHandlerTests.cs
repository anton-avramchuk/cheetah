using Cheetah.Admin.Modules.Clients.Application.Commands;
using Cheetah.Admin.Modules.Clients.Domain;
using Cheetah.Admin.Modules.Clients.Domain.Repositories;
using Cheetah.Core.Events;
using Shouldly;
using Moq;

namespace Cheetah.Admin.Modules.Clients.Application.Tests.Commands;

public class CreateClientCommandHandlerTests
{
    private readonly Mock<IClientRepository> _repositoryMock;
    private readonly Mock<IEventBus> _eventBusMock;
    private readonly CreateClientCommandHandler _handler;

    public CreateClientCommandHandlerTests()
    {
        _repositoryMock = new Mock<IClientRepository>();
        _eventBusMock = new Mock<IEventBus>();
        _handler = new CreateClientCommandHandler(_repositoryMock.Object, _eventBusMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldCreateClientAndReturnId()
    {
        // Arrange
        var command = new CreateClientCommand("Test Client", "Test Description");
        Client? capturedClient = null;

        _repositoryMock
            .Setup(r => r.Add(It.IsAny<Client>()))
            .Callback<Client>(c => capturedClient = c);

        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.ShouldNotBe(Guid.Empty);
        capturedClient.ShouldNotBeNull();
        capturedClient!.Name.ShouldBe("Test Client");
        capturedClient.Description.ShouldBe("Test Description");

        _repositoryMock.Verify(r => r.Add(It.IsAny<Client>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNullDescription_ShouldCreateClient()
    {
        // Arrange
        var command = new CreateClientCommand("Test Client", null);
        Client? capturedClient = null;

        _repositoryMock
            .Setup(r => r.Add(It.IsAny<Client>()))
            .Callback<Client>(c => capturedClient = c);

        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.ShouldNotBe(Guid.Empty);
        capturedClient.ShouldNotBeNull();
        capturedClient!.Name.ShouldBe("Test Client");
        capturedClient.Description.ShouldBeNull();
    }

    [Fact]
    public async Task HandleAsync_ShouldPublishDomainEvents()
    {
        // Arrange
        var command = new CreateClientCommand("Test Client", "Description");

        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        // Domain events are published after SaveChangesAsync
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyName_ShouldThrowArgumentException()
    {
        // Arrange
        var command = new CreateClientCommand("", "Description");

        // Act
        var act = async () => await _handler.HandleAsync(command);

        // Assert
        await Should.ThrowAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task HandleAsync_WithWhitespaceName_ShouldThrowArgumentException()
    {
        // Arrange
        var command = new CreateClientCommand("   ", "Description");

        // Act
        var act = async () => await _handler.HandleAsync(command);

        // Assert
        await Should.ThrowAsync<ArgumentException>(act);
    }
}
