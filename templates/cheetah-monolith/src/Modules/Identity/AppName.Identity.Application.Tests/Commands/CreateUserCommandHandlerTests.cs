using AppName.Identity.Application.Commands;
using AppName.Identity.Domain;
using Cheetah.Modules.Identity.Application.Commands;
using Cheetah.Modules.Identity.DataAccess.Exceptions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Shouldly;

namespace AppName.Identity.Application.Tests.Commands;

public class CreateUserCommandHandlerTests
{
    private readonly Mock<UserManager<AppNameIdentityUser>> _userManagerMock;
    private readonly Mock<RoleManager<AppNameIdentityRole>> _roleManagerMock;
    private readonly CreateUserCommandHandler _handler;

    public CreateUserCommandHandlerTests()
    {
        var userStoreMock = new Mock<IUserStore<AppNameIdentityUser>>();
        _userManagerMock = new Mock<UserManager<AppNameIdentityUser>>(
            userStoreMock.Object, null, null, null, null, null, null, null, null);

        var roleStoreMock = new Mock<IRoleStore<AppNameIdentityRole>>();
        _roleManagerMock = new Mock<RoleManager<AppNameIdentityRole>>(
            roleStoreMock.Object, null, null, null, null);

        _handler = new CreateUserCommandHandler(_userManagerMock.Object, _roleManagerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldCreateUserAndReturnId()
    {
        var command = new CreateUserCommand("johndoe", "john@example.com", "P@ssw0rd!");

        _userManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<AppNameIdentityUser>(), command.Password))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _handler.HandleAsync(command);

        result.ShouldNotBe(Guid.Empty);
        _userManagerMock.Verify(m => m.CreateAsync(It.IsAny<AppNameIdentityUser>(), command.Password), Times.Once);
    }

    [Theory]
    [InlineData(null, "john@example.com")]
    [InlineData("", "john@example.com")]
    [InlineData("   ", "john@example.com")]
    [InlineData("johndoe", null)]
    [InlineData("johndoe", "")]
    [InlineData("johndoe", "   ")]
    public async Task HandleAsync_WithInvalidUserNameOrEmail_ShouldThrowArgumentException(string? userName, string? email)
    {
        var command = new CreateUserCommand(userName!, email!, "P@ssw0rd!");
        await Should.ThrowAsync<ArgumentException>(async () => await _handler.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_WhenCreateFails_ShouldThrowIdentityException()
    {
        var command = new CreateUserCommand("johndoe", "john@example.com", "P@ssw0rd!");
        var failedResult = IdentityResult.Failed(new IdentityError { Code = "PasswordTooWeak", Description = "Password is too weak" });

        _userManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<AppNameIdentityUser>(), It.IsAny<string>()))
            .ReturnsAsync(failedResult);

        await Should.ThrowAsync<IdentityException>(async () => await _handler.HandleAsync(command));
    }
}
