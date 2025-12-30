using Cheetah.Identity.Client;
using Cheetah.Identity.Frontend.Client.Commands;
using Cheetah.Identity.Shared.Requests;
using FluentAssertions;
using Moq;

namespace Cheetah.Identity.Frontend.Client.Tests.Commands;

public class CreateRoleCommandHandlerTests
{
    private readonly Mock<IIdentityClient> _identityClientMock;
    private readonly CreateRoleCommandHandler _handler;

    public CreateRoleCommandHandlerTests()
    {
        _identityClientMock = new Mock<IIdentityClient>();
        _handler = new CreateRoleCommandHandler(_identityClientMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenSuccessful_ReturnsRoleId()
    {
        // Arrange
        var command = new CreateRoleCommand(
            Name: "Manager",
            Description: "Manager role"
        );

        var expectedRoleId = Guid.NewGuid();

        _identityClientMock
            .Setup(x => x.CreateRoleAsync(It.IsAny<CreateRoleRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedRoleId);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().Be(expectedRoleId);

        _identityClientMock.Verify(
            x => x.CreateRoleAsync(
                It.Is<CreateRoleRequest>(r =>
                    r.Name == command.Name &&
                    r.Description == command.Description),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
