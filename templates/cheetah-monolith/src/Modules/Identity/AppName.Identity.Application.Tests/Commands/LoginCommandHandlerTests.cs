using System.Security.Claims;
using AppName.Identity.Application.Commands;
using AppName.Identity.Domain;
using Cheetah.Backend.Rsa.Abstractions;
using Cheetah.Modules.Identity.Application.Commands;
using Cheetah.Modules.Identity.Application.Exceptions;
using Cheetah.Modules.Identity.Application.Services;
using Microsoft.AspNetCore.Identity;
using Moq;
using Shouldly;

namespace AppName.Identity.Application.Tests.Commands;

public class LoginCommandHandlerTests
{
    private readonly Mock<UserManager<AppNameIdentityUser>> _userManagerMock;
    private readonly Mock<RoleManager<AppNameIdentityRole>> _roleManagerMock;
    private readonly Mock<ITokenGenerator> _tokenGeneratorMock;
    private readonly Mock<IPasswordDecryptor> _passwordDecryptorMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _userManagerMock = CreateUserManagerMock();
        _roleManagerMock = CreateRoleManagerMock();
        _tokenGeneratorMock = new Mock<ITokenGenerator>();
        _passwordDecryptorMock = new Mock<IPasswordDecryptor>();
        _passwordDecryptorMock.Setup(d => d.Decrypt(It.IsAny<string>())).Returns<string>(s => s);
        _handler = new LoginCommandHandler(
            _userManagerMock.Object,
            _roleManagerMock.Object,
            _tokenGeneratorMock.Object,
            _passwordDecryptorMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCredentials_ShouldReturnToken()
    {
        var user = AppNameIdentityUser.Create("johndoe", "john@example.com");
        var command = new LoginCommand("johndoe", "P@ssw0rd!");
        const string expectedToken = "jwt.token.value";

        _userManagerMock.Setup(m => m.FindByNameAsync("johndoe")).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.CheckPasswordAsync(user, "P@ssw0rd!")).ReturnsAsync(true);
        _userManagerMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(["admin"]);
        _userManagerMock.Setup(m => m.GetClaimsAsync(user)).ReturnsAsync([]);
        _tokenGeneratorMock
            .Setup(g => g.GenerateToken(user.Id, user.UserName!, user.Email!, It.IsAny<IEnumerable<string>>(),
                It.IsAny<IEnumerable<Claim>>()))
            .Returns(new TokenResult(expectedToken, 3600));

        var result = await _handler.HandleAsync(command);

        result.Token.ShouldBe(expectedToken);
        result.ExpiresInSeconds.ShouldBe(3600);
    }

    [Fact]
    public async Task HandleAsync_WithUnknownUserName_ShouldThrowInvalidCredentialsException()
    {
        var command = new LoginCommand("unknown", "P@ssw0rd!");
        _userManagerMock.Setup(m => m.FindByNameAsync("unknown")).ReturnsAsync((AppNameIdentityUser?)null);

        await Should.ThrowAsync<InvalidCredentialsException>(async () => await _handler.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_WithWrongPassword_ShouldThrowInvalidCredentialsException()
    {
        var user = AppNameIdentityUser.Create("johndoe", "john@example.com");
        var command = new LoginCommand("johndoe", "WrongPassword");

        _userManagerMock.Setup(m => m.FindByNameAsync("johndoe")).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.CheckPasswordAsync(user, "WrongPassword")).ReturnsAsync(false);

        await Should.ThrowAsync<InvalidCredentialsException>(async () => await _handler.HandleAsync(command));
    }

    private static Mock<UserManager<AppNameIdentityUser>> CreateUserManagerMock()
    {
        var userStoreMock = new Mock<IUserStore<AppNameIdentityUser>>();
        return new Mock<UserManager<AppNameIdentityUser>>(
            userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    private static Mock<RoleManager<AppNameIdentityRole>> CreateRoleManagerMock()
    {
        // RoleManager<TRole> ctor: (store, roleValidators, keyNormalizer, errors, logger) — 5 args.
        var roleStoreMock = new Mock<IRoleStore<AppNameIdentityRole>>();
        return new Mock<RoleManager<AppNameIdentityRole>>(roleStoreMock.Object, null!, null!, null!, null!);
    }
}
