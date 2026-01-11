using Cheetah.Core.Events;
using Cheetah.Identity.Application.Commands;
using Cheetah.Identity.Application.Services;
using Cheetah.Identity.Domain.Entities;
using Cheetah.Identity.Domain.Repositories;
using Cheetah.Identity.Events;
using FluentAssertions;
using Moq;

namespace Cheetah.Identity.Application.Tests.Commands;

public class RegisterUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IEventBus> _eventBusMock;
    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _eventBusMock = new Mock<IEventBus>();

        _handler = new RegisterUserCommandHandler(
            _userRepositoryMock.Object,
            _roleRepositoryMock.Object,
            _passwordHasherMock.Object,
            _eventBusMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldCreateUser_WhenCommandIsValid()
    {
        // Arrange
        var command = new RegisterUserCommand(
            "test@example.com",
            "password123",
            "John",
            "Doe");

        var hashedPassword = "hashed_password_123";
        _passwordHasherMock.Setup(x => x.HashPassword(command.Password))
            .Returns(hashedPassword);

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        User? capturedUser = null;
        _userRepositoryMock.Setup(x => x.Add(It.IsAny<User>()))
            .Callback<User>(user => capturedUser = user);

        _userRepositoryMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        capturedUser.Should().NotBeNull();
        capturedUser!.Email.Should().Be(command.Email);
        capturedUser.FirstName.Should().Be(command.FirstName);
        capturedUser.LastName.Should().Be(command.LastName);
        capturedUser.PasswordHash.Should().Be(hashedPassword);

        _passwordHasherMock.Verify(x => x.HashPassword(command.Password), Times.Once);
        _userRepositoryMock.Verify(x => x.Add(It.IsAny<User>()), Times.Once);
        _userRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldPublishUserCreatedEvent_WhenUserIsCreated()
    {
        // Arrange
        var command = new RegisterUserCommand(
            "test@example.com",
            "password123",
            "John",
            "Doe");

        _passwordHasherMock.Setup(x => x.HashPassword(It.IsAny<string>()))
            .Returns("hashed_password");

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _userRepositoryMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        _eventBusMock.Verify(
            x => x.PublishAsync(
                It.Is<IEvent>(e =>
                    (e as UserCreatedEvent) != null &&
                    (e as UserCreatedEvent)!.Email == command.Email),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowInvalidOperationException_WhenUserAlreadyExists()
    {
        // Arrange
        var command = new RegisterUserCommand(
            "existing@example.com",
            "password123",
            "John",
            "Doe");

        var existingUser = User.Create(
            "existing@example.com",
            "old_hash",
            "Jane",
            "Smith");

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        var act = async () => await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{command.Email}*already exists*");

        _userRepositoryMock.Verify(x => x.Add(It.IsAny<User>()), Times.Never);
        _userRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldHashPassword_BeforeCreatingUser()
    {
        // Arrange
        var command = new RegisterUserCommand(
            "test@example.com",
            "plaintext_password",
            null,
            null);

        var hashedPassword = "secure_hashed_password";
        _passwordHasherMock.Setup(x => x.HashPassword("plaintext_password"))
            .Returns(hashedPassword);

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        User? capturedUser = null;
        _userRepositoryMock.Setup(x => x.Add(It.IsAny<User>()))
            .Callback<User>(user => capturedUser = user);

        _userRepositoryMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        capturedUser!.PasswordHash.Should().Be(hashedPassword);
        _passwordHasherMock.Verify(x => x.HashPassword("plaintext_password"), Times.Once);
    }
}
