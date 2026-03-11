using Crm.MasterData.Application.Commands;
using Crm.MasterData.Domain;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Shouldly;
using Moq;

namespace Crm.MasterData.Application.Tests.Commands;

public class CreateStackItemCommandHandlerTests
{
    private readonly Mock<IRepository<StackItem, Guid>> _repositoryMock;
    private readonly Mock<IEventBus> _eventBusMock;
    private readonly CreateStackItemCommandHandler _handler;

    public CreateStackItemCommandHandlerTests()
    {
        _repositoryMock = new Mock<IRepository<StackItem, Guid>>();
        _eventBusMock = new Mock<IEventBus>();
        _handler = new CreateStackItemCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldCreateEntityAndReturnId()
    {
        // Arrange
        var command = new CreateStackItemCommand("Test Entity", "Test Description");
        StackItem? capturedEntity = null;

        _repositoryMock
            .Setup(r => r.Add(It.IsAny<StackItem>()))
            .Callback<StackItem>(e => capturedEntity = e);

        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.ShouldNotBe(Guid.Empty);
        capturedEntity.ShouldNotBeNull();
        capturedEntity!.Name.ShouldBe("Test Entity");
        capturedEntity.Description.ShouldBe("Test Description");

        _repositoryMock.Verify(r => r.Add(It.IsAny<StackItem>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyName_ShouldThrowArgumentException()
    {
        // Arrange
        var command = new CreateStackItemCommand("", "Description");

        // Act
        var act = async () => await _handler.HandleAsync(command);

        // Assert
        await Should.ThrowAsync<ArgumentException>(act);
    }
}