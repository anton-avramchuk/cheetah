using Cheetah.Core.Events;
using Cheetah.Identity.Application.Commands;
using Cheetah.Identity.Application.Services;
using Cheetah.Identity.DataAccess;
using Cheetah.Identity.Domain.Entities;
using Cheetah.Identity.Events;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Cheetah.Identity.Application.Tests.Commands;

public class RegisterUserCommandHandlerTests
{
    private readonly Mock<IIdentityDbContext> _dbContextMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IEventBus> _eventBusMock;
    private readonly Mock<DbSet<User>> _userDbSetMock;
    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        _dbContextMock = new Mock<IIdentityDbContext>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _eventBusMock = new Mock<IEventBus>();
        _userDbSetMock = new Mock<DbSet<User>>();

        _dbContextMock.Setup(x => x.Users).Returns(_userDbSetMock.Object);

        _handler = new RegisterUserCommandHandler(
            _dbContextMock.Object,
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

        var users = new List<User>().AsQueryable();
        _userDbSetMock.As<IQueryable<User>>().Setup(m => m.Provider).Returns(users.Provider);
        _userDbSetMock.As<IQueryable<User>>().Setup(m => m.Expression).Returns(users.Expression);
        _userDbSetMock.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(users.ElementType);
        _userDbSetMock.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

        User? capturedUser = null;
        _dbContextMock.Setup(x => x.Users.Add(It.IsAny<User>()))
            .Callback<User>(user => capturedUser = user);

        _dbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
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
        _dbContextMock.Verify(x => x.Users.Add(It.IsAny<User>()), Times.Once);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
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

        var users = new List<User>().AsQueryable();
        _userDbSetMock.As<IQueryable<User>>().Setup(m => m.Provider).Returns(users.Provider);
        _userDbSetMock.As<IQueryable<User>>().Setup(m => m.Expression).Returns(users.Expression);
        _userDbSetMock.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(users.ElementType);
        _userDbSetMock.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

        _dbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
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

        var users = new List<User> { existingUser }.AsQueryable();
        _userDbSetMock.As<IQueryable<User>>().Setup(m => m.Provider).Returns(users.Provider);
        _userDbSetMock.As<IQueryable<User>>().Setup(m => m.Expression).Returns(users.Expression);
        _userDbSetMock.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(users.ElementType);
        _userDbSetMock.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

        // Act
        var act = async () => await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{command.Email}*already exists*");

        _dbContextMock.Verify(x => x.Users.Add(It.IsAny<User>()), Times.Never);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
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

        var users = new List<User>().AsQueryable();
        _userDbSetMock.As<IQueryable<User>>().Setup(m => m.Provider).Returns(users.Provider);
        _userDbSetMock.As<IQueryable<User>>().Setup(m => m.Expression).Returns(users.Expression);
        _userDbSetMock.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(users.ElementType);
        _userDbSetMock.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

        User? capturedUser = null;
        _dbContextMock.Setup(x => x.Users.Add(It.IsAny<User>()))
            .Callback<User>(user => capturedUser = user);

        _dbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        capturedUser!.PasswordHash.Should().Be(hashedPassword);
        _passwordHasherMock.Verify(x => x.HashPassword("plaintext_password"), Times.Once);
    }
}
