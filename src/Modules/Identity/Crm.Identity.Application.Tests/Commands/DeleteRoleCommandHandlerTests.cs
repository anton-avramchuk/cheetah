using Cheetah.Core.Domain.Exceptions;
using Cheetah.Modules.Identity.DataAccess.Exceptions;
using Cheetah.Modules.Identity.Application.Commands;
using Cheetah.Modules.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Moq;
using Shouldly;

namespace Crm.Identity.Application.Tests.Commands;

public class DeleteRoleCommandHandlerTests
{
    private readonly Mock<RoleManager<CrmIdentityRole>> _roleManagerMock;
    private readonly DeleteRoleCommandHandler _handler;

    public DeleteRoleCommandHandlerTests()
    {
        var roleStoreMock = new Mock<IRoleStore<CrmIdentityRole>>();
        _roleManagerMock = new Mock<RoleManager<CrmIdentityRole>>(
            roleStoreMock.Object, null, null, null, null);
        _handler = new DeleteRoleCommandHandler(_roleManagerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithExistingRole_ShouldDeleteRole()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var existingRole = CrmIdentityRole.Create(roleId, "admin");
        var command = new DeleteRoleCommand(roleId);

        _roleManagerMock
            .Setup(m => m.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync(existingRole);

        _roleManagerMock
            .Setup(m => m.DeleteAsync(existingRole))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _roleManagerMock.Verify(m => m.DeleteAsync(existingRole), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistingRole_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var command = new DeleteRoleCommand(roleId);

        _roleManagerMock
            .Setup(m => m.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync((CrmIdentityRole?)null);

        // Act
        var act = async () => await _handler.HandleAsync(command);

        // Assert
        await Should.ThrowAsync<EntityNotFoundException>(act);
    }

    [Fact]
    public async Task HandleAsync_WhenDeleteFails_ShouldThrowIdentityException()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var existingRole = CrmIdentityRole.Create(roleId, "admin");
        var command = new DeleteRoleCommand(roleId);
        var failedResult = IdentityResult.Failed(new IdentityError { Code = "Error", Description = "Delete failed" });

        _roleManagerMock
            .Setup(m => m.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync(existingRole);

        _roleManagerMock
            .Setup(m => m.DeleteAsync(existingRole))
            .ReturnsAsync(failedResult);

        // Act
        var act = async () => await _handler.HandleAsync(command);

        // Assert
        await Should.ThrowAsync<IdentityException>(act);
    }
}
