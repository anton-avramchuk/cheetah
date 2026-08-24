using Cheetah.Core.Domain.Exceptions;
using Cheetah.Modules.Identity.Application.Abstractions;
using Cheetah.Modules.Identity.Application.Commands;
using Cheetah.Modules.Identity.Application.Exceptions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Shouldly;

namespace Cheetah.Modules.Identity.Application.Tests.Commands;

public class UpdateUserCommandHandlerTests
{
    private readonly Mock<UserManager<StubUser>> _userManagerMock;
    private readonly Mock<RoleManager<StubRole>> _roleManagerMock;
    private readonly UpdateUserCommandHandler<StubUser, StubRole, StubUpdateUserCommand> _handler;

    public UpdateUserCommandHandlerTests()
    {
        var userStore = new Mock<IUserStore<StubUser>>();
        _userManagerMock = new Mock<UserManager<StubUser>>(userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        var roleStore = new Mock<IRoleStore<StubRole>>();
        _roleManagerMock = new Mock<RoleManager<StubRole>>(roleStore.Object, null!, null!, null!, null!);

        _handler = new UpdateUserCommandHandler<StubUser, StubRole, StubUpdateUserCommand>(
            _userManagerMock.Object, _roleManagerMock.Object, new NullEventBus(),
            new DelegateUpdateUserApplier<StubUser, StubRole, StubUpdateUserCommand>());
    }

    [Fact]
    public async Task HandleAsync_WithExistingUser_ShouldUpdateUserNameAndEmail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new StubUser("oldname", "old@example.com");
        var command = new StubUpdateUserCommand(userId, "newname", "new@example.com");

        _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync([]);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        user.UserName.ShouldBe("newname");
        user.Email.ShouldBe("new@example.com");
        _userManagerMock.Verify(m => m.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithExistingUser_ShouldRefreshSecurityStamp()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new StubUser("oldname", "old@example.com");
        var originalStamp = user.SecurityStamp;
        var command = new StubUpdateUserCommand(userId, "newname", "new@example.com");

        _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync([]);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        user.SecurityStamp.ShouldNotBe(originalStamp);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistingUser_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync((StubUser?)null);

        // Act & Assert
        await Should.ThrowAsync<EntityNotFoundException>(
            () => _handler.HandleAsync(new StubUpdateUserCommand(userId, "name", "email@example.com")).AsTask());
    }

    [Fact]
    public async Task HandleAsync_WhenUpdateFails_ShouldThrowIdentityException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new StubUser("oldname", "old@example.com");
        var failed = IdentityResult.Failed(new IdentityError { Code = "Error", Description = "Update failed" });

        _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.UpdateAsync(It.IsAny<StubUser>())).ReturnsAsync(failed);

        // Act & Assert
        await Should.ThrowAsync<IdentityException>(
            () => _handler.HandleAsync(new StubUpdateUserCommand(userId, "newname", "new@example.com")).AsTask());
    }

    [Fact]
    public async Task HandleAsync_WhenRoleIdsIsNull_ShouldNotModifyRoles()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new StubUser("johndoe", "john@example.com");
        var command = new StubUpdateUserCommand(userId, "johndoe", "john@example.com", RoleIds: null);

        _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _userManagerMock.Verify(m => m.GetRolesAsync(It.IsAny<StubUser>()), Times.Never);
        _userManagerMock.Verify(m => m.RemoveFromRolesAsync(It.IsAny<StubUser>(), It.IsAny<IEnumerable<string>>()), Times.Never);
        _userManagerMock.Verify(m => m.AddToRolesAsync(It.IsAny<StubUser>(), It.IsAny<IEnumerable<string>>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenRoleIdsIsEmpty_ShouldRemoveCurrentRolesAndNotAddNew()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new StubUser("johndoe", "john@example.com");
        var command = new StubUpdateUserCommand(userId, "johndoe", "john@example.com", RoleIds: new List<Guid>());

        _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(["admin"]);
        _userManagerMock.Setup(m => m.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>())).ReturnsAsync(IdentityResult.Success);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _userManagerMock.Verify(m => m.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()), Times.Once);
        _userManagerMock.Verify(m => m.AddToRolesAsync(It.IsAny<StubUser>(), It.IsAny<IEnumerable<string>>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithNewRoleIds_ShouldReplaceRoles()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new StubUser("johndoe", "john@example.com");
        var roleId = Guid.NewGuid();
        var role = new StubRole(roleId, "manager");
        var command = new StubUpdateUserCommand(userId, "johndoe", "john@example.com", RoleIds: [roleId]);

        _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(["admin"]);
        _userManagerMock.Setup(m => m.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>())).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(m => m.AddToRolesAsync(user, It.IsAny<IEnumerable<string>>())).ReturnsAsync(IdentityResult.Success);
        _roleManagerMock.Setup(m => m.Roles).Returns(new[] { role }.AsAsyncQueryable());

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _userManagerMock.Verify(m => m.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()), Times.Once);
        _userManagerMock.Verify(m => m.AddToRolesAsync(
            user,
            It.Is<IEnumerable<string>>(roles => roles.Contains("manager"))),
            Times.Once);
    }
}
