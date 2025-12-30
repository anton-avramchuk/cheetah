using Cheetah.Identity.Client;
using Cheetah.Identity.Frontend.Client.Commands;
using Cheetah.Identity.Shared.Requests;
using Cheetah.Identity.Shared.ViewModels;
using FluentAssertions;
using Moq;

namespace Cheetah.Identity.Frontend.Client.Tests.Commands;

public class RegisterUserCommandHandlerTests
{
    private readonly Mock<IIdentityClient> _identityClientMock;
    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        _identityClientMock = new Mock<IIdentityClient>();
        _handler = new RegisterUserCommandHandler(_identityClientMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenSuccessful_ReturnsUser()
    {
        // Arrange
        var command = new RegisterUserCommand(
            Email: "test@example.com",
            Password: "Test@123",
            FirstName: "John",
            LastName: "Doe"
        );

        var expectedUserId = Guid.NewGuid();
        var expectedUser = new UserViewModel
        {
            Id = expectedUserId,
            Email = command.Email,
            FirstName = command.FirstName,
            LastName = command.LastName,
            IsActive = false,
            EmailConfirmed = false,
            CreatedAt = DateTime.UtcNow
        };

        _identityClientMock
            .Setup(x => x.RegisterAsync(It.IsAny<RegisterUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(expectedUserId);
        result.Email.Should().Be(command.Email);
        result.FirstName.Should().Be(command.FirstName);
        result.LastName.Should().Be(command.LastName);

        _identityClientMock.Verify(
            x => x.RegisterAsync(
                It.Is<RegisterUserRequest>(r =>
                    r.Email == command.Email &&
                    r.Password == command.Password &&
                    r.FirstName == command.FirstName &&
                    r.LastName == command.LastName),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenClientReturnsNull_ReturnsNull()
    {
        // Arrange
        var command = new RegisterUserCommand(
            Email: "test@example.com",
            Password: "Test@123",
            FirstName: "John",
            LastName: "Doe"
        );

        _identityClientMock
            .Setup(x => x.RegisterAsync(It.IsAny<RegisterUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserViewModel?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().BeNull();

        _identityClientMock.Verify(
            x => x.RegisterAsync(It.IsAny<RegisterUserRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
