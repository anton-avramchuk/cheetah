using AppName.Identity.Application.Commands;
using AppName.Identity.Domain;
using Cheetah.Core.Domain.Exceptions;
using Cheetah.Modules.Identity.Application.Commands;
using Cheetah.Modules.Identity.DataAccess.Exceptions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Shouldly;

namespace AppName.Identity.Application.Tests.Commands;

public class UpdateRoleCommandHandlerTests
{
    private readonly Mock<RoleManager<AppNameIdentityRole>> _roleManagerMock;
    private readonly UpdateRoleCommandHandler _handler;

    public UpdateRoleCommandHandlerTests()
    {
        var roleStoreMock = new Mock<IRoleStore<AppNameIdentityRole>>();
        _roleManagerMock = new Mock<RoleManager<AppNameIdentityRole>>(
            roleStoreMock.Object, null, null, null, null);
        _handler = new UpdateRoleCommandHandler(_roleManagerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithExistingRole_ShouldUpdateRoleName()
    {
        var roleId = Guid.NewGuid();
        var existingRole = AppNameIdentityRole.Create(roleId, "old-name");
        var command = new UpdateRoleCommand(roleId, "new-name");

        _roleManagerMock.Setup(m => m.FindByIdAsync(roleId.ToString())).ReturnsAsync(existingRole);
        _roleManagerMock.Setup(m => m.UpdateAsync(existingRole)).ReturnsAsync(IdentityResult.Success);

        await _handler.HandleAsync(command);

        existingRole.Name.ShouldBe("new-name");
        _roleManagerMock.Verify(m => m.UpdateAsync(existingRole), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistingRole_ShouldThrowEntityNotFoundException()
    {
        var roleId = Guid.NewGuid();
        var command = new UpdateRoleCommand(roleId, "new-name");

        _roleManagerMock.Setup(m => m.FindByIdAsync(roleId.ToString())).ReturnsAsync((AppNameIdentityRole?)null);

        await Should.ThrowAsync<EntityNotFoundException>(async () => await _handler.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_WhenUpdateFails_ShouldThrowIdentityException()
    {
        var roleId = Guid.NewGuid();
        var existingRole = AppNameIdentityRole.Create(roleId, "old-name");
        var command = new UpdateRoleCommand(roleId, "new-name");
        var failedResult = IdentityResult.Failed(new IdentityError { Code = "Error", Description = "Update failed" });

        _roleManagerMock.Setup(m => m.FindByIdAsync(roleId.ToString())).ReturnsAsync(existingRole);
        _roleManagerMock.Setup(m => m.UpdateAsync(It.IsAny<AppNameIdentityRole>())).ReturnsAsync(failedResult);

        await Should.ThrowAsync<IdentityException>(async () => await _handler.HandleAsync(command));
    }
}
