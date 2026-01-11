using Cheetah.Core.Events;
using Cheetah.Identity.Application.Commands;
using Cheetah.Identity.Domain.Entities;
using Cheetah.Identity.Domain.Repositories;
using Cheetah.Identity.Events;
using FluentAssertions;
using Moq;

namespace Cheetah.Identity.Application.Tests.Commands;

public class AddPermissionToRoleCommandHandlerTests
{
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IEventBus> _eventBusMock;
    private readonly AddPermissionToRoleCommandHandler _handler;

    public AddPermissionToRoleCommandHandlerTests()
    {
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _eventBusMock = new Mock<IEventBus>();

        _handler = new AddPermissionToRoleCommandHandler(
            _roleRepositoryMock.Object,
            _eventBusMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldAddPermissionToRole_WhenRoleExists()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var permission = "users.read";
        var command = new AddPermissionToRoleCommand(roleId, permission);

        var role = Role.Create("Admin", "Administrator role");

        _roleRepositoryMock.Setup(x => x.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        _roleRepositoryMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        role.HasPermission(permission).Should().BeTrue();
        _roleRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldPublishRoleClaimAddedEvent_WhenPermissionIsAdded()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var permission = "users.write";
        var command = new AddPermissionToRoleCommand(roleId, permission);

        var role = Role.Create("Manager", "Manager role");

        _roleRepositoryMock.Setup(x => x.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        _roleRepositoryMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        _eventBusMock.Verify(
            x => x.PublishAsync(
                It.Is<IEvent>(e =>
                    (e as RoleClaimAddedEvent) != null &&
                    (e as RoleClaimAddedEvent)!.RoleName == role.Name &&
                    (e as RoleClaimAddedEvent)!.ClaimValue == permission),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowInvalidOperationException_WhenRoleNotFound()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var command = new AddPermissionToRoleCommand(roleId, "users.read");

        _roleRepositoryMock.Setup(x => x.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        // Act
        var act = async () => await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{roleId}*not found*");

        _roleRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _eventBusMock.Verify(x => x.PublishAsync(It.IsAny<IEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldNotAddDuplicatePermission_WhenPermissionAlreadyExists()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var permission = "users.delete";
        var command = new AddPermissionToRoleCommand(roleId, permission);

        var role = Role.Create("Admin", "Administrator role");
        role.AddPermission(permission); // Add permission first time
        role.ClearDomainEvents(); // Clear events from first add

        _roleRepositoryMock.Setup(x => x.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        _roleRepositoryMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        role.GetPermissions().Count(p => p == permission).Should().Be(1);
        _roleRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
