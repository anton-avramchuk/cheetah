using Cheetah.Core.Domain.Exceptions;
using Cheetah.Core.Identity.DataAccess.Exceptions;
using Crm.Identity.Application.Commands;
using Crm.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Moq;
using Shouldly;

namespace Crm.Identity.Application.Tests.Commands;

public class UpdateUserCommandHandlerTests
{
    private readonly Mock<UserManager<CrmUser>> _userManagerMock;
    private readonly Mock<RoleManager<CrmRole>> _roleManagerMock;
    private readonly UpdateUserCommandHandler _handler;

    public UpdateUserCommandHandlerTests()
    {
        var userStoreMock = new Mock<IUserStore<CrmUser>>();
        _userManagerMock = new Mock<UserManager<CrmUser>>(
            userStoreMock.Object, null, null, null, null, null, null, null, null);

        var roleStoreMock = new Mock<IRoleStore<CrmRole>>();
        _roleManagerMock = new Mock<RoleManager<CrmRole>>(
            roleStoreMock.Object, null, null, null, null);

        _handler = new UpdateUserCommandHandler(_userManagerMock.Object, _roleManagerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithExistingUser_ShouldUpdateUserNameAndEmail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingUser = CrmUser.Create("oldname", "old@example.com");
        var command = new UpdateUserCommand(userId, "newname", "new@example.com");

        _userManagerMock
            .Setup(m => m.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(existingUser);

        _userManagerMock
            .Setup(m => m.UpdateAsync(existingUser))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock
            .Setup(m => m.GetRolesAsync(existingUser))
            .ReturnsAsync([]);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        existingUser.UserName.ShouldBe("newname");
        existingUser.Email.ShouldBe("new@example.com");
        _userManagerMock.Verify(m => m.UpdateAsync(existingUser), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithExistingUser_ShouldRefreshSecurityStamp()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingUser = CrmUser.Create("oldname", "old@example.com");
        var originalStamp = existingUser.SecurityStamp;
        var command = new UpdateUserCommand(userId, "newname", "new@example.com");

        _userManagerMock
            .Setup(m => m.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(existingUser);

        _userManagerMock
            .Setup(m => m.UpdateAsync(existingUser))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock
            .Setup(m => m.GetRolesAsync(existingUser))
            .ReturnsAsync([]);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        existingUser.SecurityStamp.ShouldNotBe(originalStamp);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistingUser_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserCommand(userId, "newname", "new@example.com");

        _userManagerMock
            .Setup(m => m.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((CrmUser?)null);

        // Act
        var act = async () => await _handler.HandleAsync(command);

        // Assert
        await Should.ThrowAsync<EntityNotFoundException>(act);
    }

    [Fact]
    public async Task HandleAsync_WhenUpdateFails_ShouldThrowIdentityException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingUser = CrmUser.Create("oldname", "old@example.com");
        var command = new UpdateUserCommand(userId, "newname", "new@example.com");
        var failedResult = IdentityResult.Failed(new IdentityError { Code = "Error", Description = "Update failed" });

        _userManagerMock
            .Setup(m => m.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(existingUser);

        _userManagerMock
            .Setup(m => m.UpdateAsync(It.IsAny<CrmUser>()))
            .ReturnsAsync(failedResult);

        // Act
        var act = async () => await _handler.HandleAsync(command);

        // Assert
        await Should.ThrowAsync<IdentityException>(act);
    }
}
