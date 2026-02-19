using Crm.Identity.Application.Commands;
using Crm.Identity.Domain;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Shouldly;
using Moq;

namespace Crm.Identity.Application.Tests.Commands;

public class CreateUserIdentityCommandHandlerTests
{
    private readonly Mock<IRepository<UserIdentity, Guid>> _repositoryMock;
    private readonly Mock<IEventBus> _eventBusMock;
    private readonly CreateUserIdentityCommandHandler _handler;

    public CreateUserIdentityCommandHandlerTests()
    {
        _repositoryMock = new Mock<IRepository<UserIdentity, Guid>>();
        _eventBusMock = new Mock<IEventBus>();
        _handler = new CreateUserIdentityCommandHandler(_repositoryMock.Object, _eventBusMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldCreateEntityAndReturnId()
    {
        // Arrange
        var command = new CreateUserIdentityCommand("Test Entity", "Test Description");
        UserIdentity? capturedEntity = null;

        _repositoryMock
            .Setup(r => r.Add(It.IsAny<UserIdentity>()))
            .Callback<UserIdentity>(e => capturedEntity = e);

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

        _repositoryMock.Verify(r => r.Add(It.IsAny<UserIdentity>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyName_ShouldThrowArgumentException()
    {
        // Arrange
        var command = new CreateUserIdentityCommand("", "Description");

        // Act
        var act = async () => await _handler.HandleAsync(command);

        // Assert
        await Should.ThrowAsync<ArgumentException>(act);
    }
}