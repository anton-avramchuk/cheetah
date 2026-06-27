using System.Security.Claims;
using Cheetah.Backend.Rsa.Abstractions;
using Cheetah.Modules.Identity.Application.Commands;
using Cheetah.Modules.Identity.Application.Exceptions;
using Cheetah.Modules.Identity.Application.Services;
using Microsoft.AspNetCore.Identity;
using Moq;
using Shouldly;

namespace Cheetah.Modules.Identity.Application.Tests.Commands;

public class LoginCommandHandlerTests
{
    private readonly Mock<UserManager<StubUser>> _userManagerMock;
    private readonly Mock<ITokenGenerator> _tokenGeneratorMock;
    private readonly Mock<IPasswordDecryptor> _passwordDecryptorMock;
    private readonly LoginCommandHandler<StubUser, StubRole> _handler;
    private readonly Mock<RoleManager<StubRole>> _roleManagerMock;

    public LoginCommandHandlerTests()
    {
        _userManagerMock = CreateUserManagerMock();
        _tokenGeneratorMock = new Mock<ITokenGenerator>();
        _passwordDecryptorMock = new Mock<IPasswordDecryptor>();
        _passwordDecryptorMock.Setup(d => d.Decrypt(It.IsAny<string>())).Returns<string>(s => s);
        _roleManagerMock = CreateRoleManagerMock();
        _handler = new LoginCommandHandler<StubUser, StubRole>(
            _userManagerMock.Object,
            _roleManagerMock.Object,
            _tokenGeneratorMock.Object,
            _passwordDecryptorMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCredentials_ShouldReturnToken()
    {
        // Arrange
        var user = new StubUser("johndoe", "john@example.com");
        var command = new LoginCommand("johndoe", "P@ssw0rd!");

        _userManagerMock.Setup(m => m.FindByNameAsync("johndoe")).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.CheckPasswordAsync(user, "P@ssw0rd!")).ReturnsAsync(true);
        _userManagerMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(["admin"]);
        _userManagerMock.Setup(m => m.GetClaimsAsync(user)).ReturnsAsync([]);
        _tokenGeneratorMock
            .Setup(g => g.GenerateToken(user.Id, user.UserName!, user.Email!, It.IsAny<IEnumerable<string>>(),
                It.IsAny<IEnumerable<Claim>>()))
            .Returns(new TokenResult("jwt.token", 3600));

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Token.ShouldBe("jwt.token");
        result.ExpiresInSeconds.ShouldBe(3600);
    }

    [Fact]
    public async Task HandleAsync_WithUnknownUser_ShouldThrowInvalidCredentialsException()
    {
        _userManagerMock.Setup(m => m.FindByNameAsync("unknown")).ReturnsAsync((StubUser?)null);

        await Should.ThrowAsync<InvalidCredentialsException>(() =>
            _handler.HandleAsync(new LoginCommand("unknown", "pass")).AsTask());
    }

    [Fact]
    public async Task HandleAsync_WithWrongPassword_ShouldThrowInvalidCredentialsException()
    {
        var user = new StubUser("johndoe", "john@example.com");
        _userManagerMock.Setup(m => m.FindByNameAsync("johndoe")).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.CheckPasswordAsync(user, "wrong")).ReturnsAsync(false);

        await Should.ThrowAsync<InvalidCredentialsException>(() =>
            _handler.HandleAsync(new LoginCommand("johndoe", "wrong")).AsTask());
    }

    [Fact]
    public async Task HandleAsync_ShouldDecryptPasswordBeforeValidation()
    {
        // Arrange
        var user = new StubUser("johndoe", "john@example.com");
        var encrypted = "encrypted_blob";
        var plain = "P@ssw0rd!";

        _passwordDecryptorMock.Setup(d => d.Decrypt(encrypted)).Returns(plain);
        _userManagerMock.Setup(m => m.FindByNameAsync("johndoe")).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.CheckPasswordAsync(user, plain)).ReturnsAsync(true);
        _userManagerMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync([]);
        _userManagerMock.Setup(m => m.GetClaimsAsync(user)).ReturnsAsync([]);
        _tokenGeneratorMock
            .Setup(g => g.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<Claim>>()))
            .Returns(new TokenResult("token", 3600));

        // Act
        await _handler.HandleAsync(new LoginCommand("johndoe", encrypted));

        // Assert: plain password was used, not the encrypted one
        _userManagerMock.Verify(m => m.CheckPasswordAsync(user, plain), Times.Once);
        _userManagerMock.Verify(m => m.CheckPasswordAsync(user, encrypted), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldNotCheckPasswordIfUserNotFound()
    {
        _userManagerMock.Setup(m => m.FindByNameAsync(It.IsAny<string>())).ReturnsAsync((StubUser?)null);

        await Should.ThrowAsync<InvalidCredentialsException>(() =>
            _handler.HandleAsync(new LoginCommand("unknown", "pass")).AsTask());

        _passwordDecryptorMock.Verify(d => d.Decrypt(It.IsAny<string>()), Times.Never);
        _userManagerMock.Verify(m => m.CheckPasswordAsync(It.IsAny<StubUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldPassRolesToTokenGenerator()
    {
        // Arrange
        var user = new StubUser("johndoe", "john@example.com");
        var roles = new List<string> { "admin", "manager" };

        _userManagerMock.Setup(m => m.FindByNameAsync("johndoe")).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.CheckPasswordAsync(user, "pass")).ReturnsAsync(true);
        _userManagerMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(roles);
        _userManagerMock.Setup(m => m.GetClaimsAsync(user)).ReturnsAsync([]);
        _tokenGeneratorMock
            .Setup(g => g.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<Claim>>()))
            .Returns(new TokenResult("token", 3600));

        // Act
        await _handler.HandleAsync(new LoginCommand("johndoe", "pass"));

        // Assert
        _tokenGeneratorMock.Verify(g =>
            g.GenerateToken(user.Id, user.UserName!, user.Email!, roles, It.IsAny<IEnumerable<Claim>>()), Times.Once);
    }


    [Fact]
    public async Task HandleAsync_ShouldPassClaimsToTokenGenerator()
    {
        // Arrange
        var user = new StubUser("johndoe", "john@example.com");

        var adminRole = new StubRole(Guid.NewGuid(), "admin");

        var managerRole = new StubRole(Guid.NewGuid(), "manager");

        var roles = new List<string> { "admin", "manager" };

        var userClaims = new List<Claim>()
        {
            new("userClaim1Type", "userClaim1Value"),
            new("userClaim2Type", "userClaim2Value"),
        };

        var adminRoleClaims = new List<Claim>()
        {
            new("adminRoleClaim1Type", "adminRoleClaim1Value"),
            new("adminRoleClaim2Type", "adminRoleClaim2Value"),
        };

        var managerRoleClaims = new List<Claim>()
        {
            new("managerRoleClaim1Type", "managerRoleClaim1Value"),
            new("managerRoleClaim2Type", "managerRoleClaim2Value"),
        };

        _userManagerMock.Setup(m => m.FindByNameAsync("johndoe")).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.CheckPasswordAsync(user, "pass")).ReturnsAsync(true);

        _userManagerMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(roles);


        _roleManagerMock.Setup(x => x.FindByNameAsync("admin")).ReturnsAsync(adminRole);
        _roleManagerMock.Setup(x => x.FindByNameAsync("manager")).ReturnsAsync(managerRole);

        _roleManagerMock.Setup(x => x.GetClaimsAsync(adminRole)).ReturnsAsync(adminRoleClaims);
        _roleManagerMock.Setup(x => x.GetClaimsAsync(managerRole)).ReturnsAsync(managerRoleClaims);

        _userManagerMock.Setup(q => q.GetClaimsAsync(user)).ReturnsAsync(userClaims);

        _tokenGeneratorMock
            .Setup(g => g.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<Claim>>()))
            .Returns(new TokenResult("token", 3600));

        // Act
        await _handler.HandleAsync(new LoginCommand("johndoe", "pass"));

        // Assert
        _tokenGeneratorMock.Verify(g =>
            g.GenerateToken(user.Id, user.UserName!, user.Email!, roles,
                adminRoleClaims.Union(managerRoleClaims).Union(userClaims)), Times.Once);
    }

    private static Mock<UserManager<StubUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<StubUser>>();
        return new Mock<UserManager<StubUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    private static Mock<RoleManager<StubRole>> CreateRoleManagerMock()
    {
        // RoleManager<TRole> ctor: (store, roleValidators, keyNormalizer, errors, logger) — 5 args.
        var store = new Mock<IRoleStore<StubRole>>();
        return new Mock<RoleManager<StubRole>>(store.Object, null!, null!, null!, null!);
    }
}