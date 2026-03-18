using AppName.Identity.Application.Commands;
using AppName.Identity.Domain;
using Cheetah.Modules.Identity.Application.Commands;
using Cheetah.Modules.Identity.DataAccess.Exceptions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Shouldly;

namespace AppName.Identity.Application.Tests.Commands;

public class CreateRoleCommandHandlerTests
{
    private readonly Mock<RoleManager<AppNameIdentityRole>> _roleManagerMock;
    private readonly CreateRoleCommandHandler _handler;

    public CreateRoleCommandHandlerTests()
    {
        var roleStoreMock = new Mock<IRoleStore<AppNameIdentityRole>>();
        _roleManagerMock = new Mock<RoleManager<AppNameIdentityRole>>(
            roleStoreMock.Object, null, null, null, null);
        _handler = new CreateRoleCommandHandler(_roleManagerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidName_ShouldCreateRoleAndReturnId()
    {
        var command = new CreateRoleCommand("admin");

        _roleManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<AppNameIdentityRole>()))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _handler.HandleAsync(command);

        result.ShouldNotBe(Guid.Empty);
        _roleManagerMock.Verify(m => m.CreateAsync(It.IsAny<AppNameIdentityRole>()), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task HandleAsync_WithInvalidName_ShouldThrowArgumentException(string? name)
    {
        var command = new CreateRoleCommand(name!);
        await Should.ThrowAsync<ArgumentException>(async () => await _handler.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_WhenCreateFails_ShouldThrowIdentityException()
    {
        var command = new CreateRoleCommand("admin");
        var failedResult = IdentityResult.Failed(new IdentityError { Code = "DuplicateRoleName", Description = "Role already exists" });

        _roleManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<AppNameIdentityRole>()))
            .ReturnsAsync(failedResult);

        await Should.ThrowAsync<IdentityException>(async () => await _handler.HandleAsync(command));
    }
}
