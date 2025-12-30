using Cheetah.Identity.Client;
using Cheetah.Identity.Frontend.Client.Queries;
using FluentAssertions;
using Moq;

namespace Cheetah.Identity.Frontend.Client.Tests.Queries;

public class GetUserPermissionsQueryHandlerTests
{
    private readonly Mock<IIdentityClient> _identityClientMock;
    private readonly GetUserPermissionsQueryHandler _handler;

    public GetUserPermissionsQueryHandlerTests()
    {
        _identityClientMock = new Mock<IIdentityClient>();
        _handler = new GetUserPermissionsQueryHandler(_identityClientMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ReturnsListOfPermissions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserPermissionsQuery(userId);

        var expectedPermissions = new List<string>
        {
            "Users.View",
            "Users.Create",
            "Users.Edit",
            "Users.Delete"
        };

        _identityClientMock
            .Setup(x => x.GetUserPermissionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPermissions);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(4);
        result.Should().Contain("Users.View");
        result.Should().Contain("Users.Create");
        result.Should().Contain("Users.Edit");
        result.Should().Contain("Users.Delete");

        _identityClientMock.Verify(
            x => x.GetUserPermissionsAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenNoPermissions_ReturnsEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserPermissionsQuery(userId);

        _identityClientMock
            .Setup(x => x.GetUserPermissionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string>());

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();

        _identityClientMock.Verify(
            x => x.GetUserPermissionsAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
