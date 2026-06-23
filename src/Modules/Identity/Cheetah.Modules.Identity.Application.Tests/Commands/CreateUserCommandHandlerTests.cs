using Cheetah.Core.Events;
using Cheetah.Modules.Identity.Application.Commands;
using Cheetah.Modules.Identity.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Shouldly;

namespace Cheetah.Modules.Identity.Application.Tests.Commands;

public sealed class StubCreateUserCommandHandler(
    UserManager<StubUser> userManager, RoleManager<StubRole> roleManager, IEventBus eventBus)
    : CreateUserCommandHandler<StubUser, StubRole>(userManager, roleManager, eventBus)
{
    protected override StubUser CreateUser(CreateUserCommand command) => new(command.UserName, command.Email);
}

public class CreateUserCommandHandlerTests
{
    private readonly Mock<UserManager<StubUser>> _userManagerMock;
    private readonly Mock<RoleManager<StubRole>> _roleManagerMock;
    private readonly StubCreateUserCommandHandler _handler;

    public CreateUserCommandHandlerTests()
    {
        var userStore = new Mock<IUserStore<StubUser>>();
        _userManagerMock = new Mock<UserManager<StubUser>>(userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        var roleStore = new Mock<IRoleStore<StubRole>>();
        _roleManagerMock = new Mock<RoleManager<StubRole>>(roleStore.Object, null!, null!, null!, null!);

        _handler = new StubCreateUserCommandHandler(_userManagerMock.Object, _roleManagerMock.Object, new NullEventBus());
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldCreateUserAndReturnId()
    {
        // Arrange
        var command = new CreateUserCommand("johndoe", "john@example.com", "P@ssw0rd!");
        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<StubUser>(), command.Password)).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.ShouldNotBe(Guid.Empty);
        _userManagerMock.Verify(m => m.CreateAsync(It.IsAny<StubUser>(), command.Password), Times.Once);
    }

    [Theory]
    [InlineData(null, "john@example.com")]
    [InlineData("", "john@example.com")]
    [InlineData("   ", "john@example.com")]
    [InlineData("johndoe", null)]
    [InlineData("johndoe", "")]
    [InlineData("johndoe", "   ")]
    public async Task HandleAsync_WithInvalidUserNameOrEmail_ShouldThrowArgumentException(
        string? userName, string? email)
    {
        var command = new CreateUserCommand(userName!, email!, "P@ssw0rd!");

        await Should.ThrowAsync<ArgumentException>(() => _handler.HandleAsync(command).AsTask());
    }

    [Fact]
    public async Task HandleAsync_WhenCreateFails_ShouldThrowIdentityException()
    {
        // Arrange
        var command = new CreateUserCommand("johndoe", "john@example.com", "P@ssw0rd!");
        var failed = IdentityResult.Failed(new IdentityError { Code = "Weak", Description = "Password too weak" });
        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<StubUser>(), It.IsAny<string>())).ReturnsAsync(failed);

        // Act & Assert
        await Should.ThrowAsync<IdentityException>(() => _handler.HandleAsync(command).AsTask());
    }

    [Fact]
    public async Task HandleAsync_WithoutRoleIds_ShouldNotCallAddToRoles()
    {
        // Arrange
        var command = new CreateUserCommand("johndoe", "john@example.com", "P@ssw0rd!");
        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<StubUser>(), command.Password)).ReturnsAsync(IdentityResult.Success);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _userManagerMock.Verify(m => m.AddToRolesAsync(It.IsAny<StubUser>(), It.IsAny<IEnumerable<string>>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithRoleIds_ShouldAssignMatchingRoles()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var role = new StubRole(roleId, "admin");
        var command = new CreateUserCommand("johndoe", "john@example.com", "P@ssw0rd!", [roleId]);

        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<StubUser>(), command.Password)).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(m => m.AddToRolesAsync(It.IsAny<StubUser>(), It.IsAny<IEnumerable<string>>())).ReturnsAsync(IdentityResult.Success);
        _roleManagerMock.Setup(m => m.Roles).Returns(new[] { role }.AsAsyncQueryable());

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _userManagerMock.Verify(m => m.AddToRolesAsync(
            It.IsAny<StubUser>(),
            It.Is<IEnumerable<string>>(roles => roles.Contains("admin"))),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenAddToRolesFails_ShouldThrowIdentityException()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var role = new StubRole(roleId, "admin");
        var command = new CreateUserCommand("johndoe", "john@example.com", "P@ssw0rd!", [roleId]);
        var failed = IdentityResult.Failed(new IdentityError { Code = "Error", Description = "Role assignment failed" });

        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<StubUser>(), command.Password)).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(m => m.AddToRolesAsync(It.IsAny<StubUser>(), It.IsAny<IEnumerable<string>>())).ReturnsAsync(failed);
        _roleManagerMock.Setup(m => m.Roles).Returns(new[] { role }.AsAsyncQueryable());

        // Act & Assert
        await Should.ThrowAsync<IdentityException>(() => _handler.HandleAsync(command).AsTask());
    }
}
