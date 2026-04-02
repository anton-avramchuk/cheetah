using Cheetah.Backend.Rsa.Abstractions;
using Crm.Identity.Application.Commands;
using Cheetah.Modules.Identity.Application.Commands;
using Cheetah.Modules.Identity.Application.Exceptions;
using Cheetah.Modules.Identity.Application.Services;
using Cheetah.Modules.Identity.Domain;
using Crm.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Moq;
using Shouldly;

namespace Crm.Identity.Application.Tests.Commands;

public class LoginCommandHandlerTests
{
    private readonly Mock<UserManager<CrmIdentityUser>> _userManagerMock;
    private readonly Mock<ITokenGenerator> _tokenGeneratorMock;
    private readonly Mock<IPasswordDecryptor> _passwordDecryptorMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _userManagerMock = CreateUserManagerMock();
        _tokenGeneratorMock = new Mock<ITokenGenerator>();
        _passwordDecryptorMock = new Mock<IPasswordDecryptor>();
        _passwordDecryptorMock.Setup(d => d.Decrypt(It.IsAny<string>())).Returns<string>(s => s);

        _handler = new LoginCommandHandler(
            _userManagerMock.Object,
            _tokenGeneratorMock.Object,
            _passwordDecryptorMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCredentials_ShouldReturnToken()
    {
        // Arrange
        var user = CrmIdentityUser.Create("johndoe", "john@example.com");
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
            .Setup(g => g.GenerateToken(user.Id, user.UserName!, user.Email!, It.IsAny<IEnumerable<string>>()))
            .Returns(new TokenResult(expectedToken, 3600));

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
            .ReturnsAsync((CrmIdentityUser?)null);

        // Act
        var act = async () => await _handler.HandleAsync(command);

        // Assert
        await Should.ThrowAsync<InvalidCredentialsException>(act);
    }

    [Fact]
    public async Task HandleAsync_WithWrongPassword_ShouldThrowInvalidCredentialsException()
    {
        // Arrange
        var user = CrmIdentityUser.Create("johndoe", "john@example.com");
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
        var user = CrmIdentityUser.Create("johndoe", "john@example.com");
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
            .Setup(g => g.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
            .Returns(new TokenResult("token", 3600));

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _tokenGeneratorMock.Verify(g =>
            g.GenerateToken(user.Id, user.UserName!, user.Email!, roles), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldPassDecryptedPasswordToUserManager()
    {
        // Arrange
        var user = CrmIdentityUser.Create("johndoe", "john@example.com");
        var encryptedPassword = "encrypted_blob";
        var plainPassword = "P@ssw0rd!";
        var command = new LoginCommand("johndoe", encryptedPassword);

        _passwordDecryptorMock
            .Setup(d => d.Decrypt(encryptedPassword))
            .Returns(plainPassword);

        _userManagerMock
            .Setup(m => m.FindByNameAsync("johndoe"))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(m => m.CheckPasswordAsync(user, plainPassword))
            .ReturnsAsync(true);

        _userManagerMock
            .Setup(m => m.GetRolesAsync(user))
            .ReturnsAsync([]);

        _tokenGeneratorMock
            .Setup(g => g.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
            .Returns(new TokenResult("token", 3600));

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _userManagerMock.Verify(m => m.CheckPasswordAsync(user, plainPassword), Times.Once);
        _userManagerMock.Verify(m => m.CheckPasswordAsync(user, encryptedPassword), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenDecryptorReturnsWrongPassword_ShouldThrowInvalidCredentialsException()
    {
        // Arrange
        var user = CrmIdentityUser.Create("johndoe", "john@example.com");
        var command = new LoginCommand("johndoe", "encrypted_blob");

        _passwordDecryptorMock
            .Setup(d => d.Decrypt("encrypted_blob"))
            .Returns("wrong_plain_password");

        _userManagerMock
            .Setup(m => m.FindByNameAsync("johndoe"))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(m => m.CheckPasswordAsync(user, "wrong_plain_password"))
            .ReturnsAsync(false);

        // Act
        var act = async () => await _handler.HandleAsync(command);

        // Assert
        await Should.ThrowAsync<InvalidCredentialsException>(act);
    }

    private static Mock<UserManager<CrmIdentityUser>> CreateUserManagerMock()
    {
        var userStoreMock = new Mock<IUserStore<CrmIdentityUser>>();
        return new Mock<UserManager<CrmIdentityUser>>(
            userStoreMock.Object, null, null, null, null, null, null, null, null);
    }
}
