using Cheetah.Core.Domain.Exceptions;
using Cheetah.Core.Identity.DataAccess.Exceptions;
using Crm.Identity.Application.Commands;
using Crm.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Moq;
using Shouldly;

namespace Crm.Identity.Application.Tests.Commands;

public class UpdateRoleCommandHandlerTests
{
    private readonly Mock<RoleManager<CrmRole>> _roleManagerMock;
    private readonly UpdateRoleCommandHandler _handler;

    public UpdateRoleCommandHandlerTests()
    {
        var roleStoreMock = new Mock<IRoleStore<CrmRole>>();
        _roleManagerMock = new Mock<RoleManager<CrmRole>>(
            roleStoreMock.Object, null, null, null, null);
        _handler = new UpdateRoleCommandHandler(_roleManagerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithExistingRole_ShouldUpdateRoleName()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var existingRole = CrmRole.Create(roleId, "old-name");
        var command = new UpdateRoleCommand(roleId, "new-name");

        _roleManagerMock
            .Setup(m => m.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync(existingRole);

        _roleManagerMock
            .Setup(m => m.UpdateAsync(existingRole))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        existingRole.Name.ShouldBe("new-name");
        _roleManagerMock.Verify(m => m.UpdateAsync(existingRole), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistingRole_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var command = new UpdateRoleCommand(roleId, "new-name");

        _roleManagerMock
            .Setup(m => m.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync((CrmRole?)null);

        // Act
        var act = async () => await _handler.HandleAsync(command);

        // Assert
        await Should.ThrowAsync<EntityNotFoundException>(act);
    }

    [Fact]
    public async Task HandleAsync_WhenUpdateFails_ShouldThrowIdentityException()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var existingRole = CrmRole.Create(roleId, "old-name");
        var command = new UpdateRoleCommand(roleId, "new-name");
        var failedResult = IdentityResult.Failed(new IdentityError { Code = "Error", Description = "Update failed" });

        _roleManagerMock
            .Setup(m => m.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync(existingRole);

        _roleManagerMock
            .Setup(m => m.UpdateAsync(It.IsAny<CrmRole>()))
            .ReturnsAsync(failedResult);

        // Act
        var act = async () => await _handler.HandleAsync(command);

        // Assert
        await Should.ThrowAsync<IdentityException>(act);
    }
}
