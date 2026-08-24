using Cheetah.Modules.Identity.Application.Abstractions;
using Cheetah.Modules.Identity.Application.Commands;
using Cheetah.Modules.Identity.Application.Exceptions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Shouldly;

namespace Cheetah.Modules.Identity.Application.Tests.Commands;

public class CreateRoleCommandHandlerTests
{
    private readonly Mock<RoleManager<StubRole>> _roleManagerMock;
    private readonly CreateRoleCommandHandler<StubRole, StubCreateRoleCommand> _handler;

    public CreateRoleCommandHandlerTests()
    {
        var store = new Mock<IRoleStore<StubRole>>();
        _roleManagerMock = new Mock<RoleManager<StubRole>>(store.Object, null!, null!, null!, null!);
        _handler = new CreateRoleCommandHandler<StubRole, StubCreateRoleCommand>(
            _roleManagerMock.Object,
            new DelegateCreateRoleFactory<StubRole, StubCreateRoleCommand>(c => new StubRole(Guid.NewGuid(), c.Name)));
    }

    [Fact]
    public async Task HandleAsync_WithValidName_ShouldCreateRoleAndReturnId()
    {
        // Arrange
        var command = new StubCreateRoleCommand("admin");
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
        var command = new StubCreateRoleCommand(name!);

        await Should.ThrowAsync<ArgumentException>(() => _handler.HandleAsync(command).AsTask());
    }

    [Fact]
    public async Task HandleAsync_WhenCreateFails_ShouldThrowIdentityException()
    {
        // Arrange
        var command = new StubCreateRoleCommand("admin");
        var failed = IdentityResult.Failed(new IdentityError { Code = "Duplicate", Description = "Already exists" });
        _roleManagerMock.Setup(m => m.CreateAsync(It.IsAny<StubRole>())).ReturnsAsync(failed);

        // Act & Assert
        await Should.ThrowAsync<IdentityException>(() => _handler.HandleAsync(command).AsTask());
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnRoleIdFromCreatedRole()
    {
        // Arrange
        var command = new StubCreateRoleCommand("manager");
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
