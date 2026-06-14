using Cheetah.Core.Domain.Exceptions;
using Cheetah.Core.Events;
using Cheetah.Modules.Identity.Application.Commands;
using Cheetah.Modules.Identity.Application.Tests;
using Cheetah.Modules.Identity.DataAccess.Exceptions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Shouldly;

namespace Cheetah.Modules.Identity.Application.Tests.Commands;

public sealed class StubDeleteUserCommandHandler(UserManager<StubUser> userManager, IEventBus eventBus)
    : DeleteUserCommandHandler<StubUser, StubRole>(userManager, eventBus);

public class DeleteUserCommandHandlerTests
{
    private readonly Mock<UserManager<StubUser>> _userManagerMock;
    private readonly StubDeleteUserCommandHandler _handler;

    public DeleteUserCommandHandlerTests()
    {
        var store = new Mock<IUserStore<StubUser>>();
        _userManagerMock = new Mock<UserManager<StubUser>>(store.Object, null, null, null, null, null, null, null, null);
        _handler = new StubDeleteUserCommandHandler(_userManagerMock.Object, new NullEventBus());
    }

    [Fact]
    public async Task HandleAsync_WithExistingUser_ShouldDeleteUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new StubUser("johndoe", "john@example.com");
        _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);

        // Act
        await _handler.HandleAsync(new DeleteUserCommand(userId));

        // Assert
        _userManagerMock.Verify(m => m.DeleteAsync(user), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistingUser_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync((StubUser?)null);

        // Act & Assert
        await Should.ThrowAsync<EntityNotFoundException>(() => _handler.HandleAsync(new DeleteUserCommand(userId)).AsTask());
    }

    [Fact]
    public async Task HandleAsync_WhenDeleteFails_ShouldThrowIdentityException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new StubUser("johndoe", "john@example.com");
        var failed = IdentityResult.Failed(new IdentityError { Code = "Error", Description = "Delete failed" });
        _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.DeleteAsync(user)).ReturnsAsync(failed);

        // Act & Assert
        await Should.ThrowAsync<IdentityException>(() => _handler.HandleAsync(new DeleteUserCommand(userId)).AsTask());
    }

    [Fact]
    public async Task HandleAsync_WithNonExistingUser_ShouldNotCallDelete()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync((StubUser?)null);

        // Act
        await Should.ThrowAsync<EntityNotFoundException>(() => _handler.HandleAsync(new DeleteUserCommand(userId)).AsTask());

        // Assert
        _userManagerMock.Verify(m => m.DeleteAsync(It.IsAny<StubUser>()), Times.Never);
    }
}
