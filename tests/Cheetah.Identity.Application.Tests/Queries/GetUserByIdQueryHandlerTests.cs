using Cheetah.Identity.Application.Queries;
using Cheetah.Identity.Application.Tests.Helpers;
using Cheetah.Identity.Contracts.ViewModels;
using Cheetah.Identity.Domain.Entities;
using Cheetah.Identity.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace Cheetah.Identity.Application.Tests.Queries;

public class GetUserByIdQueryHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly GetUserByIdQueryHandler _handler;

    public GetUserByIdQueryHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _handler = new GetUserByIdQueryHandler(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);

        var user = User.Create("test@example.com", "hash", "John", "Doe");
        typeof(User).GetProperty("Id")!.SetValue(user, userId);

        var users = new TestAsyncEnumerable<User>(new List<User> { user });

        _userRepositoryMock.Setup(x => x.AsNoTrackingQueryable())
            .Returns(users);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(userId);
        result.Email.Should().Be("test@example.com");
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);

        var users = new TestAsyncEnumerable<User>(new List<User>());

        _userRepositoryMock.Setup(x => x.AsNoTrackingQueryable())
            .Returns(users);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnUserViewModel_WithAllProperties()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);

        var user = User.Create("complete@example.com", "hash", "Complete", "User");
        typeof(User).GetProperty("Id")!.SetValue(user, userId);
        user.ConfirmEmail();
        user.RecordLogin();

        var users = new TestAsyncEnumerable<User>(new List<User> { user });

        _userRepositoryMock.Setup(x => x.AsNoTrackingQueryable())
            .Returns(users);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(userId);
        result.Email.Should().Be("complete@example.com");
        result.FirstName.Should().Be("Complete");
        result.LastName.Should().Be("User");
        result.IsActive.Should().BeTrue();
        result.EmailConfirmed.Should().BeTrue();
        result.LastLoginAt.Should().NotBeNull();
        result.CreatedAt.Should().NotBe(default);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFirstMatchingUser_WhenMultipleUsersExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);

        var targetUser = User.Create("target@example.com", "hash", "Target", "User");
        typeof(User).GetProperty("Id")!.SetValue(targetUser, userId);

        var otherUser = User.Create("other@example.com", "hash", "Other", "User");
        typeof(User).GetProperty("Id")!.SetValue(otherUser, Guid.NewGuid());

        var users = new TestAsyncEnumerable<User>(new List<User> { otherUser, targetUser });

        _userRepositoryMock.Setup(x => x.AsNoTrackingQueryable())
            .Returns(users);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(userId);
        result.Email.Should().Be("target@example.com");
    }
}
