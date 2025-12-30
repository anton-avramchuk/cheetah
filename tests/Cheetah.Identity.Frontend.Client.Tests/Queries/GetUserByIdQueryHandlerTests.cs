using Cheetah.Identity.Client;
using Cheetah.Identity.Frontend.Client.Queries;
using Cheetah.Identity.Shared.ViewModels;
using FluentAssertions;
using Moq;

namespace Cheetah.Identity.Frontend.Client.Tests.Queries;

public class GetUserByIdQueryHandlerTests
{
    private readonly Mock<IIdentityClient> _identityClientMock;
    private readonly GetUserByIdQueryHandler _handler;

    public GetUserByIdQueryHandlerTests()
    {
        _identityClientMock = new Mock<IIdentityClient>();
        _handler = new GetUserByIdQueryHandler(_identityClientMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenUserExists_ReturnsUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);

        var expectedUser = new UserViewModel
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            IsActive = true,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        _identityClientMock
            .Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(userId);
        result.Email.Should().Be(expectedUser.Email);
        result.FirstName.Should().Be(expectedUser.FirstName);
        result.LastName.Should().Be(expectedUser.LastName);

        _identityClientMock.Verify(
            x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);

        _identityClientMock
            .Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserViewModel?)null);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();

        _identityClientMock.Verify(
            x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
