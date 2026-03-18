using AppName.Identity.Application.Commands;
using AppName.Identity.Domain;
using Cheetah.Core.Domain.Exceptions;
using Cheetah.Modules.Identity.Application.Commands;
using Cheetah.Modules.Identity.DataAccess.Exceptions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Shouldly;

namespace AppName.Identity.Application.Tests.Commands;

public class DeleteUserCommandHandlerTests
{
    private readonly Mock<UserManager<AppNameIdentityUser>> _userManagerMock;
    private readonly DeleteUserCommandHandler _handler;

    public DeleteUserCommandHandlerTests()
    {
        var userStoreMock = new Mock<IUserStore<AppNameIdentityUser>>();
        _userManagerMock = new Mock<UserManager<AppNameIdentityUser>>(
            userStoreMock.Object, null, null, null, null, null, null, null, null);
        _handler = new DeleteUserCommandHandler(_userManagerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithExistingUser_ShouldDeleteUser()
    {
        var userId = Guid.NewGuid();
        var existingUser = AppNameIdentityUser.Create("johndoe", "john@example.com");
        var command = new DeleteUserCommand(userId);

        _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync(existingUser);
        _userManagerMock.Setup(m => m.DeleteAsync(existingUser)).ReturnsAsync(IdentityResult.Success);

        await _handler.HandleAsync(command);

        _userManagerMock.Verify(m => m.DeleteAsync(existingUser), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistingUser_ShouldThrowEntityNotFoundException()
    {
        var userId = Guid.NewGuid();
        var command = new DeleteUserCommand(userId);

        _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync((AppNameIdentityUser?)null);

        await Should.ThrowAsync<EntityNotFoundException>(async () => await _handler.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_WhenDeleteFails_ShouldThrowIdentityException()
    {
        var userId = Guid.NewGuid();
        var existingUser = AppNameIdentityUser.Create("johndoe", "john@example.com");
        var command = new DeleteUserCommand(userId);
        var failedResult = IdentityResult.Failed(new IdentityError { Code = "Error", Description = "Delete failed" });

        _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync(existingUser);
        _userManagerMock.Setup(m => m.DeleteAsync(existingUser)).ReturnsAsync(failedResult);

        await Should.ThrowAsync<IdentityException>(async () => await _handler.HandleAsync(command));
    }
}
