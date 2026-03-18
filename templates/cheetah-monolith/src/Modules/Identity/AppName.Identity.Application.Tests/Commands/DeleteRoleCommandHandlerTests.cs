using AppName.Identity.Application.Commands;
using AppName.Identity.Domain;
using Cheetah.Core.Domain.Exceptions;
using Cheetah.Modules.Identity.Application.Commands;
using Cheetah.Modules.Identity.DataAccess.Exceptions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Shouldly;

namespace AppName.Identity.Application.Tests.Commands;

public class DeleteRoleCommandHandlerTests
{
    private readonly Mock<RoleManager<AppNameIdentityRole>> _roleManagerMock;
    private readonly DeleteRoleCommandHandler _handler;

    public DeleteRoleCommandHandlerTests()
    {
        var roleStoreMock = new Mock<IRoleStore<AppNameIdentityRole>>();
        _roleManagerMock = new Mock<RoleManager<AppNameIdentityRole>>(
            roleStoreMock.Object, null, null, null, null);
        _handler = new DeleteRoleCommandHandler(_roleManagerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithExistingRole_ShouldDeleteRole()
    {
        var roleId = Guid.NewGuid();
        var existingRole = AppNameIdentityRole.Create(roleId, "admin");
        var command = new DeleteRoleCommand(roleId);

        _roleManagerMock.Setup(m => m.FindByIdAsync(roleId.ToString())).ReturnsAsync(existingRole);
        _roleManagerMock.Setup(m => m.DeleteAsync(existingRole)).ReturnsAsync(IdentityResult.Success);

        await _handler.HandleAsync(command);

        _roleManagerMock.Verify(m => m.DeleteAsync(existingRole), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistingRole_ShouldThrowEntityNotFoundException()
    {
        var roleId = Guid.NewGuid();
        var command = new DeleteRoleCommand(roleId);

        _roleManagerMock.Setup(m => m.FindByIdAsync(roleId.ToString())).ReturnsAsync((AppNameIdentityRole?)null);

        await Should.ThrowAsync<EntityNotFoundException>(async () => await _handler.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_WhenDeleteFails_ShouldThrowIdentityException()
    {
        var roleId = Guid.NewGuid();
        var existingRole = AppNameIdentityRole.Create(roleId, "admin");
        var command = new DeleteRoleCommand(roleId);
        var failedResult = IdentityResult.Failed(new IdentityError { Code = "Error", Description = "Delete failed" });

        _roleManagerMock.Setup(m => m.FindByIdAsync(roleId.ToString())).ReturnsAsync(existingRole);
        _roleManagerMock.Setup(m => m.DeleteAsync(existingRole)).ReturnsAsync(failedResult);

        await Should.ThrowAsync<IdentityException>(async () => await _handler.HandleAsync(command));
    }
}
