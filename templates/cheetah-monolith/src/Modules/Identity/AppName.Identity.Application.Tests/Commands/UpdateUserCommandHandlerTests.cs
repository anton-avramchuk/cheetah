using Cheetah.Core.Domain.Exceptions;
using Cheetah.Core.Identity.DataAccess.Exceptions;
using Cheetah.Modules.Identity.Application.Commands;
using Cheetah.Modules.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Moq;
using Shouldly;

namespace AppName.Identity.Application.Tests.Commands;

public class UpdateUserCommandHandlerTests
{
    private readonly Mock<UserManager<CrmIdentityUser>> _userManagerMock;
    private readonly Mock<RoleManager<CrmIdentityRole>> _roleManagerMock;
    private readonly UpdateUserCommandHandler _handler;

    public UpdateUserCommandHandlerTests()
    {
        var userStoreMock = new Mock<IUserStore<CrmIdentityUser>>();
        _userManagerMock = new Mock<UserManager<CrmIdentityUser>>(
            userStoreMock.Object, null, null, null, null, null, null, null, null);

        var roleStoreMock = new Mock<IRoleStore<CrmIdentityRole>>();
        _roleManagerMock = new Mock<RoleManager<CrmIdentityRole>>(
            roleStoreMock.Object, null, null, null, null);

        _handler = new UpdateUserCommandHandler(_userManagerMock.Object, _roleManagerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithExistingUser_ShouldUpdateUserNameAndEmail()
    {
        var userId = Guid.NewGuid();
        var existingUser = CrmIdentityUser.Create("oldname", "old@example.com");
        var command = new UpdateUserCommand(userId, "newname", "new@example.com");

        _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync(existingUser);
        _userManagerMock.Setup(m => m.UpdateAsync(existingUser)).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(m => m.GetRolesAsync(existingUser)).ReturnsAsync([]);

        await _handler.HandleAsync(command);

        existingUser.UserName.ShouldBe("newname");
        existingUser.Email.ShouldBe("new@example.com");
    }

    [Fact]
    public async Task HandleAsync_WithNonExistingUser_ShouldThrowEntityNotFoundException()
    {
        var userId = Guid.NewGuid();
        var command = new UpdateUserCommand(userId, "newname", "new@example.com");

        _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync((CrmIdentityUser?)null);

        await Should.ThrowAsync<EntityNotFoundException>(async () => await _handler.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_WhenUpdateFails_ShouldThrowIdentityException()
    {
        var userId = Guid.NewGuid();
        var existingUser = CrmIdentityUser.Create("oldname", "old@example.com");
        var command = new UpdateUserCommand(userId, "newname", "new@example.com");
        var failedResult = IdentityResult.Failed(new IdentityError { Code = "Error", Description = "Update failed" });

        _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync(existingUser);
        _userManagerMock.Setup(m => m.UpdateAsync(It.IsAny<CrmIdentityUser>())).ReturnsAsync(failedResult);

        await Should.ThrowAsync<IdentityException>(async () => await _handler.HandleAsync(command));
    }
}
