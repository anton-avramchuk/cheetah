using Cheetah.Identity.Client;
using Cheetah.Identity.Frontend.Client.Commands;
using FluentAssertions;
using Moq;

namespace Cheetah.Identity.Frontend.Client.Tests.Commands;

public class AssignRoleToUserCommandHandlerTests
{
    private readonly Mock<IIdentityClient> _identityClientMock;
    private readonly AssignRoleToUserCommandHandler _handler;

    public AssignRoleToUserCommandHandlerTests()
    {
        _identityClientMock = new Mock<IIdentityClient>();
        _handler = new AssignRoleToUserCommandHandler(_identityClientMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenSuccessful_CallsClient()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var command = new AssignRoleToUserCommand(userId, roleId);

        _identityClientMock
            .Setup(x => x.AssignRoleToUserAsync(userId, roleId, It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        _identityClientMock.Verify(
            x => x.AssignRoleToUserAsync(userId, roleId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
