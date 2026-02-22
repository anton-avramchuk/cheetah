using Cheetah.Backend.Jwt.Abstractions;
using Crm.Identity.Application.Commands;
using Crm.Identity.Application.Exceptions;
using Crm.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Moq;
using Shouldly;

namespace Crm.Identity.Application.Tests.Commands;

public class LoginCommandHandlerTests
{
    private readonly Mock<UserManager<CrmUser>> _userManagerMock;
    private readonly Mock<IJwtTokenGenerator> _tokenGeneratorMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        var userStoreMock = new Mock<IUserStore<CrmUser>>();
        _userManagerMock = new Mock<UserManager<CrmUser>>(
            userStoreMock.Object, null, null, null, null, null, null, null, null);

        _tokenGeneratorMock = new Mock<IJwtTokenGenerator>();

        _handler = new LoginCommandHandler(
            _userManagerMock.Object,
            _tokenGeneratorMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCredentials_ShouldReturnToken()
    {
        // Arrange
        var user = CrmUser.Create("johndoe", "john@example.com");
        var command = new LoginCommand("johndoe", "P@ssw0rd!");
        var expectedToken = "jwt.token.value";

        _userManagerMock
            .Setup(m => m.FindByNameAsync("johndoe"))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(m => m.CheckPasswordAsync(user, "P@ssw0rd!"))
            .ReturnsAsync(true);

        _userManagerMock
            .Setup(m => m.GetRolesAsync(user))
            .ReturnsAsync(["admin"]);

        _tokenGeneratorMock
            .Setup(g => g.GenerateToken(user.Id, user.UserName!, user.Email!, It.IsAny<IEnumerable<string>>(), null))
            .Returns(new TokenGenerationResult(expectedToken, 3600));

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Token.ShouldBe(expectedToken);
        result.ExpiresInSeconds.ShouldBe(3600);
    }

    [Fact]
    public async Task HandleAsync_WithUnknownUserName_ShouldThrowInvalidCredentialsException()
    {
        // Arrange
        var command = new LoginCommand("unknown", "P@ssw0rd!");

        _userManagerMock
            .Setup(m => m.FindByNameAsync("unknown"))
            .ReturnsAsync((CrmUser?)null);

        // Act
        var act = async () => await _handler.HandleAsync(command);

        // Assert
        await Should.ThrowAsync<InvalidCredentialsException>(act);
    }

    [Fact]
    public async Task HandleAsync_WithWrongPassword_ShouldThrowInvalidCredentialsException()
    {
        // Arrange
        var user = CrmUser.Create("johndoe", "john@example.com");
        var command = new LoginCommand("johndoe", "WrongPassword");

        _userManagerMock
            .Setup(m => m.FindByNameAsync("johndoe"))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(m => m.CheckPasswordAsync(user, "WrongPassword"))
            .ReturnsAsync(false);

        // Act
        var act = async () => await _handler.HandleAsync(command);

        // Assert
        await Should.ThrowAsync<InvalidCredentialsException>(act);
    }

    [Fact]
    public async Task HandleAsync_WithValidCredentials_ShouldPassRolesToTokenGenerator()
    {
        // Arrange
        var user = CrmUser.Create("johndoe", "john@example.com");
        var command = new LoginCommand("johndoe", "P@ssw0rd!");
        var roles = new List<string> { "admin", "recruiter" };

        _userManagerMock
            .Setup(m => m.FindByNameAsync("johndoe"))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(m => m.CheckPasswordAsync(user, "P@ssw0rd!"))
            .ReturnsAsync(true);

        _userManagerMock
            .Setup(m => m.GetRolesAsync(user))
            .ReturnsAsync(roles);

        _tokenGeneratorMock
            .Setup(g => g.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), null))
            .Returns(new TokenGenerationResult("token", 3600));

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _tokenGeneratorMock.Verify(g =>
            g.GenerateToken(user.Id, user.UserName!, user.Email!, roles, null), Times.Once);
    }
}
