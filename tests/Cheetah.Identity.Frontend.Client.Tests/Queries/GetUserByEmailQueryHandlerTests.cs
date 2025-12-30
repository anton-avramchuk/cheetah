using Cheetah.Identity.Client;
using Cheetah.Identity.Frontend.Client.Queries;
using Cheetah.Identity.Shared.ViewModels;
using FluentAssertions;
using Moq;

namespace Cheetah.Identity.Frontend.Client.Tests.Queries;

public class GetUserByEmailQueryHandlerTests
{
    private readonly Mock<IIdentityClient> _identityClientMock;
    private readonly GetUserByEmailQueryHandler _handler;

    public GetUserByEmailQueryHandlerTests()
    {
        _identityClientMock = new Mock<IIdentityClient>();
        _handler = new GetUserByEmailQueryHandler(_identityClientMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenUserExists_ReturnsUser()
    {
        // Arrange
        var email = "test@example.com";
        var query = new GetUserByEmailQuery(email);

        var expectedUser = new UserViewModel
        {
            Id = Guid.NewGuid(),
            Email = email,
            FirstName = "John",
            LastName = "Doe",
            IsActive = true,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        _identityClientMock
            .Setup(x => x.GetUserByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
        result.FirstName.Should().Be(expectedUser.FirstName);
        result.LastName.Should().Be(expectedUser.LastName);

        _identityClientMock.Verify(
            x => x.GetUserByEmailAsync(email, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ReturnsNull()
    {
        // Arrange
        var email = "nonexistent@example.com";
        var query = new GetUserByEmailQuery(email);

        _identityClientMock
            .Setup(x => x.GetUserByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserViewModel?)null);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();

        _identityClientMock.Verify(
            x => x.GetUserByEmailAsync(email, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
