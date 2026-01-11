using Cheetah.Core.Events;
using Cheetah.Identity.Application.Commands;
using Cheetah.Identity.Application.Services;
using Cheetah.Identity.Domain.Entities;
using Cheetah.Identity.Domain.Repositories;
using Cheetah.Identity.Events;
using FluentAssertions;
using Moq;

namespace Cheetah.Identity.Application.Tests.Commands;

public class ChangePasswordCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IEventBus> _eventBusMock;
    private readonly ChangePasswordCommandHandler _handler;

    public ChangePasswordCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _eventBusMock = new Mock<IEventBus>();

        _handler = new ChangePasswordCommandHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _eventBusMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldChangePassword_WhenCurrentPasswordIsCorrect()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var currentPassword = "old_password";
        var newPassword = "new_password";
        var currentPasswordHash = "current_hash";
        var newPasswordHash = "new_hash";

        var user = User.Create("test@example.com", currentPasswordHash, "John", "Doe");

        var command = new ChangePasswordCommand(userId, currentPassword, newPassword);

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock.Setup(x => x.VerifyPassword(currentPassword, currentPasswordHash))
            .Returns(true);

        _passwordHasherMock.Setup(x => x.HashPassword(newPassword))
            .Returns(newPasswordHash);

        _userRepositoryMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        user.PasswordHash.Should().Be(newPasswordHash);
        _userRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldPublishPasswordChangedEvent_WhenPasswordIsChanged()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var currentPassword = "old_password";
        var newPassword = "new_password";
        var currentPasswordHash = "current_hash";

        var user = User.Create("test@example.com", currentPasswordHash, "John", "Doe");

        var command = new ChangePasswordCommand(userId, currentPassword, newPassword);

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock.Setup(x => x.VerifyPassword(currentPassword, currentPasswordHash))
            .Returns(true);

        _passwordHasherMock.Setup(x => x.HashPassword(newPassword))
            .Returns("new_hash");

        _userRepositoryMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        _eventBusMock.Verify(
            x => x.PublishAsync(
                It.Is<IEvent>(e => e.GetType() == typeof(PasswordChangedEvent)),
                It.IsAny<CancellationToken>()),
            Times.Once());
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowInvalidOperationException_WhenUserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ChangePasswordCommand(userId, "old", "new");

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
    public async Task HandleAsync_ShouldThrowInvalidOperationException_WhenCurrentPasswordIsIncorrect()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var currentPassword = "wrong_password";
        var newPassword = "new_password";
        var actualPasswordHash = "actual_hash";

        var user = User.Create("test@example.com", actualPasswordHash, "John", "Doe");

        var command = new ChangePasswordCommand(userId, currentPassword, newPassword);

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock.Setup(x => x.VerifyPassword(currentPassword, actualPasswordHash))
            .Returns(false);

        // Act
        var act = async () => await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*password is incorrect*");

        _userRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _eventBusMock.Verify(x => x.PublishAsync(It.IsAny<IEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
