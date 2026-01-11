using Cheetah.Core.Events;
using Cheetah.Identity.Application.Commands;
using Cheetah.Identity.Domain.Entities;
using Cheetah.Identity.Domain.Repositories;
using Cheetah.Identity.Events;
using FluentAssertions;
using Moq;

namespace Cheetah.Identity.Application.Tests.Commands;

public class AssignRoleToUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IEventBus> _eventBusMock;
    private readonly AssignRoleToUserCommandHandler _handler;

    public AssignRoleToUserCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _eventBusMock = new Mock<IEventBus>();

        _handler = new AssignRoleToUserCommandHandler(
            _userRepositoryMock.Object,
            _roleRepositoryMock.Object,
            _eventBusMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldAssignRoleToUser_WhenBothExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var command = new AssignRoleToUserCommand(userId, roleId);

        var user = User.Create("test@example.com", "hash", "John", "Doe");
        var role = Role.Create("Admin", "Administrator role");

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _roleRepositoryMock.Setup(x => x.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        _userRepositoryMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        user.Roles.Should().ContainSingle(r => r.RoleId == role.Id);
        _userRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldPublishUserRoleAssignedEvent_WhenRoleIsAssigned()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var command = new AssignRoleToUserCommand(userId, roleId);

        var user = User.Create("test@example.com", "hash", "John", "Doe");
        var role = Role.Create("Manager", "Manager role");

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _roleRepositoryMock.Setup(x => x.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        _userRepositoryMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        _eventBusMock.Verify(
            x => x.PublishAsync(
                It.Is<IEvent>(e =>
                    (e as UserRoleAssignedEvent) != null &&
                    (e as UserRoleAssignedEvent)!.UserId == user.Id &&
                    (e as UserRoleAssignedEvent)!.RoleId == role.Id &&
                    (e as UserRoleAssignedEvent)!.RoleName == role.Name),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowInvalidOperationException_WhenUserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var command = new AssignRoleToUserCommand(userId, roleId);

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var act = async () => await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{userId}*not found*");

        _userRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _eventBusMock.Verify(x => x.PublishAsync(It.IsAny<IEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowInvalidOperationException_WhenRoleNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var command = new AssignRoleToUserCommand(userId, roleId);

        var user = User.Create("test@example.com", "hash", "John", "Doe");

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _roleRepositoryMock.Setup(x => x.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        // Act
        var act = async () => await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{roleId}*not found*");

        _userRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _eventBusMock.Verify(x => x.PublishAsync(It.IsAny<IEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
