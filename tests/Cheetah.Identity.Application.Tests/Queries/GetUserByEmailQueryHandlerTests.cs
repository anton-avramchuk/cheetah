using Cheetah.Identity.Application.Queries;
using Cheetah.Identity.Application.Tests.Helpers;
using Cheetah.Identity.Contracts.ViewModels;
using Cheetah.Identity.Domain.Entities;
using Cheetah.Identity.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace Cheetah.Identity.Application.Tests.Queries;

public class GetUserByEmailQueryHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly GetUserByEmailQueryHandler _handler;

    public GetUserByEmailQueryHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _handler = new GetUserByEmailQueryHandler(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var email = "test@example.com";
        var query = new GetUserByEmailQuery(email);

        var user = User.Create(email, "hash", "John", "Doe");
        user.ConfirmEmail();

        var users = new TestAsyncEnumerable<User>(new List<User> { user });

        _userRepositoryMock.Setup(x => x.AsNoTrackingQueryable())
            .Returns(users);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.EmailConfirmed.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        var email = "nonexistent@example.com";
        var query = new GetUserByEmailQuery(email);

        var users = new TestAsyncEnumerable<User>(new List<User>());

        _userRepositoryMock.Setup(x => x.AsNoTrackingQueryable())
            .Returns(users);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_ShouldBeCaseInsensitive_WhenSearchingByEmail()
    {
        // Arrange
        var email = "Test@Example.COM";
        var query = new GetUserByEmailQuery(email);

        var user = User.Create("test@example.com", "hash", "Jane", "Smith");

        var users = new TestAsyncEnumerable<User>(new List<User> { user });

        _userRepositoryMock.Setup(x => x.AsNoTrackingQueryable())
            .Returns(users);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnUserViewModel_WithAllProperties()
    {
        // Arrange
        var email = "complete@example.com";
        var query = new GetUserByEmailQuery(email);

        var user = User.Create(email, "hash", "Complete", "User");
        user.ConfirmEmail();
        user.RecordLogin();

        var users = new TestAsyncEnumerable<User>(new List<User> { user });

        _userRepositoryMock.Setup(x => x.AsNoTrackingQueryable())
            .Returns(users);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Email.Should().Be(email);
        result.FirstName.Should().Be("Complete");
        result.LastName.Should().Be("User");
        result.IsActive.Should().BeTrue();
        result.EmailConfirmed.Should().BeTrue();
        result.LastLoginAt.Should().NotBeNull();
        result.CreatedAt.Should().NotBe(default);
    }
}
