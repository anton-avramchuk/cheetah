using Cheetah.Core.Domain.Exceptions;
using Cheetah.Modules.Identity.Application.Abstractions;
using Cheetah.Modules.Identity.Application.Commands;
using Cheetah.Modules.Identity.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Shouldly;

namespace Cheetah.Modules.Identity.Application.Tests.Commands;

public class UpdateRoleCommandHandlerTests
{
    private readonly Mock<RoleManager<StubRole>> _roleManagerMock;
    private readonly UpdateRoleCommandHandler<StubRole, StubUpdateRoleCommand> _handler;

    public UpdateRoleCommandHandlerTests()
    {
        var store = new Mock<IRoleStore<StubRole>>();
        _roleManagerMock = new Mock<RoleManager<StubRole>>(store.Object, null!, null!, null!, null!);
        _handler = new UpdateRoleCommandHandler<StubRole, StubUpdateRoleCommand>(
            _roleManagerMock.Object, new DelegateUpdateRoleApplier<StubRole, StubUpdateRoleCommand>());
    }

    [Fact]
    public async Task HandleAsync_WithExistingRole_ShouldUpdateRoleName()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var role = new StubRole(roleId, "old-name");
        var command = new StubUpdateRoleCommand(roleId, "new-name");

        _roleManagerMock.Setup(m => m.FindByIdAsync(roleId.ToString())).ReturnsAsync(role);
        _roleManagerMock.Setup(m => m.UpdateAsync(role)).ReturnsAsync(IdentityResult.Success);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        role.Name.ShouldBe("new-name");
        _roleManagerMock.Verify(m => m.UpdateAsync(role), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistingRole_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        _roleManagerMock.Setup(m => m.FindByIdAsync(roleId.ToString())).ReturnsAsync((StubRole?)null);

        // Act & Assert
        await Should.ThrowAsync<EntityNotFoundException>(
            () => _handler.HandleAsync(new StubUpdateRoleCommand(roleId, "new-name")).AsTask());
    }

    [Fact]
    public async Task HandleAsync_WhenUpdateFails_ShouldThrowIdentityException()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var role = new StubRole(roleId, "old-name");
        var failed = IdentityResult.Failed(new IdentityError { Code = "Error", Description = "Update failed" });

        _roleManagerMock.Setup(m => m.FindByIdAsync(roleId.ToString())).ReturnsAsync(role);
        _roleManagerMock.Setup(m => m.UpdateAsync(It.IsAny<StubRole>())).ReturnsAsync(failed);

        // Act & Assert
        await Should.ThrowAsync<IdentityException>(
            () => _handler.HandleAsync(new StubUpdateRoleCommand(roleId, "new-name")).AsTask());
    }

    [Fact]
    public async Task HandleAsync_WithNonExistingRole_ShouldNotCallUpdate()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        _roleManagerMock.Setup(m => m.FindByIdAsync(roleId.ToString())).ReturnsAsync((StubRole?)null);

        // Act
        await Should.ThrowAsync<EntityNotFoundException>(
            () => _handler.HandleAsync(new StubUpdateRoleCommand(roleId, "new-name")).AsTask());

        // Assert
        _roleManagerMock.Verify(m => m.UpdateAsync(It.IsAny<StubRole>()), Times.Never);
    }
}
