using Cheetah.Core.Domain.Exceptions;
using Cheetah.Modules.Identity.Application.Commands;
using Cheetah.Modules.Identity.Application.Exceptions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Shouldly;

namespace Cheetah.Modules.Identity.Application.Tests.Commands;

public class DeleteRoleCommandHandlerTests
{
    private readonly Mock<RoleManager<StubRole>> _roleManagerMock;
    private readonly DeleteRoleCommandHandler<StubRole> _handler;

    public DeleteRoleCommandHandlerTests()
    {
        var store = new Mock<IRoleStore<StubRole>>();
        _roleManagerMock = new Mock<RoleManager<StubRole>>(store.Object, null!, null!, null!, null!);
        _handler = new DeleteRoleCommandHandler<StubRole>(_roleManagerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithExistingRole_ShouldDeleteRole()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var role = new StubRole(roleId, "admin");
        _roleManagerMock.Setup(m => m.FindByIdAsync(roleId.ToString())).ReturnsAsync(role);
        _roleManagerMock.Setup(m => m.DeleteAsync(role)).ReturnsAsync(IdentityResult.Success);

        // Act
        await _handler.HandleAsync(new DeleteRoleCommand(roleId));

        // Assert
        _roleManagerMock.Verify(m => m.DeleteAsync(role), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistingRole_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        _roleManagerMock.Setup(m => m.FindByIdAsync(roleId.ToString())).ReturnsAsync((StubRole?)null);

        // Act & Assert
        await Should.ThrowAsync<EntityNotFoundException>(() => _handler.HandleAsync(new DeleteRoleCommand(roleId)).AsTask());
    }

    [Fact]
    public async Task HandleAsync_WhenDeleteFails_ShouldThrowIdentityException()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var role = new StubRole(roleId, "admin");
        var failed = IdentityResult.Failed(new IdentityError { Code = "Error", Description = "Delete failed" });
        _roleManagerMock.Setup(m => m.FindByIdAsync(roleId.ToString())).ReturnsAsync(role);
        _roleManagerMock.Setup(m => m.DeleteAsync(role)).ReturnsAsync(failed);

        // Act & Assert
        await Should.ThrowAsync<IdentityException>(() => _handler.HandleAsync(new DeleteRoleCommand(roleId)).AsTask());
    }

    [Fact]
    public async Task HandleAsync_WithNonExistingRole_ShouldNotCallDelete()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        _roleManagerMock.Setup(m => m.FindByIdAsync(roleId.ToString())).ReturnsAsync((StubRole?)null);

        // Act
        await Should.ThrowAsync<EntityNotFoundException>(() => _handler.HandleAsync(new DeleteRoleCommand(roleId)).AsTask());

        // Assert
        _roleManagerMock.Verify(m => m.DeleteAsync(It.IsAny<StubRole>()), Times.Never);
    }
}
