using Cheetah.Modules.Identity.Application.Commands;
using Cheetah.Modules.Identity.Application.Exceptions;
using Cheetah.Modules.Identity.Application.Services;
using Cheetah.Modules.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Moq;
using Shouldly;

namespace AppName.Identity.Application.Tests.Commands;

public class LoginCommandHandlerTests
{
    private readonly Mock<UserManager<CrmIdentityUser>> _userManagerMock;
    private readonly Mock<ITokenGenerator> _tokenGeneratorMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _userManagerMock = CreateUserManagerMock();
        _tokenGeneratorMock = new Mock<ITokenGenerator>();
        _handler = new LoginCommandHandler(_userManagerMock.Object, _tokenGeneratorMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCredentials_ShouldReturnToken()
    {
        var user = CrmIdentityUser.Create("johndoe", "john@example.com");
        var command = new LoginCommand("johndoe", "P@ssw0rd!");
        const string expectedToken = "jwt.token.value";

        _userManagerMock.Setup(m => m.FindByNameAsync("johndoe")).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.CheckPasswordAsync(user, "P@ssw0rd!")).ReturnsAsync(true);
        _userManagerMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(["admin"]);
        _tokenGeneratorMock
            .Setup(g => g.GenerateToken(user.Id, user.UserName!, user.Email!, It.IsAny<IEnumerable<string>>()))
            .Returns(new TokenResult(expectedToken, 3600));

        var result = await _handler.HandleAsync(command);

        result.Token.ShouldBe(expectedToken);
        result.ExpiresInSeconds.ShouldBe(3600);
    }

    [Fact]
    public async Task HandleAsync_WithUnknownUserName_ShouldThrowInvalidCredentialsException()
    {
        var command = new LoginCommand("unknown", "P@ssw0rd!");
        _userManagerMock.Setup(m => m.FindByNameAsync("unknown")).ReturnsAsync((CrmIdentityUser?)null);

        await Should.ThrowAsync<InvalidCredentialsException>(async () => await _handler.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_WithWrongPassword_ShouldThrowInvalidCredentialsException()
    {
        var user = CrmIdentityUser.Create("johndoe", "john@example.com");
        var command = new LoginCommand("johndoe", "WrongPassword");

        _userManagerMock.Setup(m => m.FindByNameAsync("johndoe")).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.CheckPasswordAsync(user, "WrongPassword")).ReturnsAsync(false);

        await Should.ThrowAsync<InvalidCredentialsException>(async () => await _handler.HandleAsync(command));
    }

    private static Mock<UserManager<CrmIdentityUser>> CreateUserManagerMock()
    {
        var userStoreMock = new Mock<IUserStore<CrmIdentityUser>>();
        return new Mock<UserManager<CrmIdentityUser>>(
            userStoreMock.Object, null, null, null, null, null, null, null, null);
    }
}
