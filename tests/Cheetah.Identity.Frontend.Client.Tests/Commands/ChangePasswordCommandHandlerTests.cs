using Cheetah.Identity.Client;
using Cheetah.Identity.Frontend.Client.Commands;
using Cheetah.Identity.Shared.Requests;
using FluentAssertions;
using Moq;

namespace Cheetah.Identity.Frontend.Client.Tests.Commands;

public class ChangePasswordCommandHandlerTests
{
    private readonly Mock<IIdentityClient> _identityClientMock;
    private readonly ChangePasswordCommandHandler _handler;

    public ChangePasswordCommandHandlerTests()
    {
        _identityClientMock = new Mock<IIdentityClient>();
        _handler = new ChangePasswordCommandHandler(_identityClientMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenSuccessful_CallsClient()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ChangePasswordCommand(
            UserId: userId,
            CurrentPassword: "OldPassword@123",
            NewPassword: "NewPassword@123"
        );

        _identityClientMock
            .Setup(x => x.ChangePasswordAsync(userId, It.IsAny<ChangePasswordRequest>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        _identityClientMock.Verify(
            x => x.ChangePasswordAsync(
                userId,
                It.Is<ChangePasswordRequest>(r =>
                    r.CurrentPassword == command.CurrentPassword &&
                    r.NewPassword == command.NewPassword),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
