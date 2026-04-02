using Cheetah.Modules.Identity.Application.Commands;
using Cheetah.Modules.Identity.Application.Tests;
using Cheetah.Modules.Identity.DataAccess.Exceptions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Shouldly;

namespace Cheetah.Modules.Identity.Application.Tests.Commands;

public sealed class StubCreateRoleCommandHandler(RoleManager<StubRole> roleManager)
    : CreateRoleCommandHandler<StubRole>(roleManager)
{
    protected override StubRole CreateRole(CreateRoleCommand command) => new(Guid.NewGuid(), command.Name);
}

public class CreateRoleCommandHandlerTests
{
    private readonly Mock<RoleManager<StubRole>> _roleManagerMock;
    private readonly StubCreateRoleCommandHandler _handler;

    public CreateRoleCommandHandlerTests()
    {
        var store = new Mock<IRoleStore<StubRole>>();
        _roleManagerMock = new Mock<RoleManager<StubRole>>(store.Object, null, null, null, null);
        _handler = new StubCreateRoleCommandHandler(_roleManagerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidName_ShouldCreateRoleAndReturnId()
    {
        // Arrange
        var command = new CreateRoleCommand("admin");
        _roleManagerMock.Setup(m => m.CreateAsync(It.IsAny<StubRole>())).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.ShouldNotBe(Guid.Empty);
        _roleManagerMock.Verify(m => m.CreateAsync(It.IsAny<StubRole>()), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task HandleAsync_WithInvalidName_ShouldThrowArgumentException(string? name)
    {
        var command = new CreateRoleCommand(name!);

        await Should.ThrowAsync<ArgumentException>(() => _handler.HandleAsync(command).AsTask());
    }

    [Fact]
    public async Task HandleAsync_WhenCreateFails_ShouldThrowIdentityException()
    {
        // Arrange
        var command = new CreateRoleCommand("admin");
        var failed = IdentityResult.Failed(new IdentityError { Code = "Duplicate", Description = "Already exists" });
        _roleManagerMock.Setup(m => m.CreateAsync(It.IsAny<StubRole>())).ReturnsAsync(failed);

        // Act & Assert
        await Should.ThrowAsync<IdentityException>(() => _handler.HandleAsync(command).AsTask());
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnRoleIdFromCreatedRole()
    {
        // Arrange
        var command = new CreateRoleCommand("manager");
        Guid capturedId = Guid.Empty;

        _roleManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<StubRole>()))
            .Callback<StubRole>(r => capturedId = r.Id)
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.ShouldBe(capturedId);
    }
}
