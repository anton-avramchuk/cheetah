using Cheetah.Core.Events;
using Cheetah.Identity.Application.Commands;
using Cheetah.Identity.Domain.Entities;
using Cheetah.Identity.Domain.Repositories;
using Cheetah.Identity.Events;
using FluentAssertions;
using Moq;

namespace Cheetah.Identity.Application.Tests.Commands;

public class CreateRoleCommandHandlerTests
{
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IEventBus> _eventBusMock;
    private readonly CreateRoleCommandHandler _handler;

    public CreateRoleCommandHandlerTests()
    {
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _eventBusMock = new Mock<IEventBus>();

        _handler = new CreateRoleCommandHandler(
            _roleRepositoryMock.Object,
            _eventBusMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldCreateRole_WhenCommandIsValid()
    {
        // Arrange
        var command = new CreateRoleCommand("Admin", "Administrator role");

        _roleRepositoryMock.Setup(x => x.GetByNameAsync(command.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        Role? capturedRole = null;
        _roleRepositoryMock.Setup(x => x.Add(It.IsAny<Role>()))
            .Callback<Role>(role => capturedRole = role);

        _roleRepositoryMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        capturedRole.Should().NotBeNull();
        capturedRole!.Name.Should().Be(command.Name);
        capturedRole.Description.Should().Be(command.Description);

        _roleRepositoryMock.Verify(x => x.Add(It.IsAny<Role>()), Times.Once);
        _roleRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldPublishRoleCreatedEvent_WhenRoleIsCreated()
    {
        // Arrange
        var command = new CreateRoleCommand("Manager", "Manager role");

        _roleRepositoryMock.Setup(x => x.GetByNameAsync(command.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        _roleRepositoryMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        _eventBusMock.Verify(
            x => x.PublishAsync(
                It.Is<IEvent>(e =>
                    (e as RoleCreatedEvent) != null &&
                    (e as RoleCreatedEvent)!.RoleName == command.Name),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowInvalidOperationException_WhenRoleAlreadyExists()
    {
        // Arrange
        var command = new CreateRoleCommand("ExistingRole", "Description");

        var existingRole = Role.Create("ExistingRole", "Old description");

        _roleRepositoryMock.Setup(x => x.GetByNameAsync(command.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRole);

        // Act
        var act = async () => await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{command.Name}*already exists*");

        _roleRepositoryMock.Verify(x => x.Add(It.IsAny<Role>()), Times.Never);
        _roleRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldCreateRole_WithNullDescription()
    {
        // Arrange
        var command = new CreateRoleCommand("User", null);

        _roleRepositoryMock.Setup(x => x.GetByNameAsync(command.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        Role? capturedRole = null;
        _roleRepositoryMock.Setup(x => x.Add(It.IsAny<Role>()))
            .Callback<Role>(role => capturedRole = role);

        _roleRepositoryMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        capturedRole.Should().NotBeNull();
        capturedRole!.Name.Should().Be(command.Name);
        capturedRole.Description.Should().BeNull();
    }
}
